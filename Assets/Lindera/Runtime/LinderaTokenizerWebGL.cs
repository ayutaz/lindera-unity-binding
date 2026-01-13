#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LinderaUnityBinding
{
    /// <summary>
    /// WebGL用Lindera形態素解析エンジンのラッパークラス
    /// </summary>
    /// <remarks>
    /// WebGLビルドでは、lindera-wasmを使用してブラウザ上で形態素解析を行います。
    /// </remarks>
    public sealed class LinderaTokenizerWebGL : ILinderaTokenizer
    {
        [DllImport("__Internal")]
        private static extern void LinderaWebGL_Initialize(Action<int> onComplete);

        [DllImport("__Internal")]
        private static extern int LinderaWebGL_IsInitialized();

        [DllImport("__Internal")]
        private static extern int LinderaWebGL_CreateTokenizer();

        [DllImport("__Internal")]
        private static extern void LinderaWebGL_DestroyTokenizer(int tokenizerId);

        [DllImport("__Internal")]
        private static extern IntPtr LinderaWebGL_Tokenize(int tokenizerId, string text);

        [DllImport("__Internal")]
        private static extern void LinderaWebGL_FreeString(IntPtr ptr);

        [DllImport("__Internal")]
        private static extern IntPtr LinderaWebGL_GetVersion();

        private static bool _globalInitialized;
        private static bool _globalInitializing;
        private static UniTaskCompletionSource<bool> _initCompletionSource;

        private int _tokenizerId;
        private bool _disposed;

        /// <summary>
        /// トークナイザーが有効かどうか
        /// </summary>
        public bool IsValid => _tokenizerId != 0 && !_disposed;

        /// <summary>
        /// WASMモジュールを初期化する（非同期）
        /// </summary>
        /// <returns>初期化の成否</returns>
        public static async UniTask<bool> InitializeAsync()
        {
            if (_globalInitialized)
            {
                return true;
            }

            if (_globalInitializing)
            {
                return await _initCompletionSource.Task;
            }

            _globalInitializing = true;
            _initCompletionSource = new UniTaskCompletionSource<bool>();

            LinderaWebGL_Initialize(OnInitializeComplete);

            return await _initCompletionSource.Task;
        }

        [AOT.MonoPInvokeCallback(typeof(Action<int>))]
        private static void OnInitializeComplete(int success)
        {
            _globalInitialized = success != 0;
            _globalInitializing = false;
            _initCompletionSource?.TrySetResult(_globalInitialized);

            if (_globalInitialized)
            {
                Debug.Log("[LinderaWebGL] Initialization successful");
            }
            else
            {
                Debug.LogError("[LinderaWebGL] Initialization failed");
            }
        }

        /// <summary>
        /// WASMが初期化済みかどうか
        /// </summary>
        public static bool IsWasmInitialized => _globalInitialized || LinderaWebGL_IsInitialized() != 0;

        /// <summary>
        /// トークナイザーを作成
        /// </summary>
        /// <exception cref="LinderaException">WASMが初期化されていない、またはトークナイザーの作成に失敗した場合</exception>
        public LinderaTokenizerWebGL()
        {
            if (!IsWasmInitialized)
            {
                throw new LinderaException("WASM module is not initialized. Call InitializeAsync() first.");
            }

            _tokenizerId = LinderaWebGL_CreateTokenizer();
            if (_tokenizerId == 0)
            {
                throw new LinderaException("Failed to create WebGL tokenizer");
            }
        }

        /// <summary>
        /// テキストを形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <returns>トークンの配列</returns>
        public LinderaToken[] Tokenize(string text)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<LinderaToken>();
            }

            var resultPtr = LinderaWebGL_Tokenize(_tokenizerId, text);
            if (resultPtr == IntPtr.Zero)
            {
                throw new LinderaException("Failed to tokenize text");
            }

            try
            {
                var jsonResult = Marshal.PtrToStringUTF8(resultPtr);
                return ParseTokensFromJson(jsonResult);
            }
            finally
            {
                LinderaWebGL_FreeString(resultPtr);
            }
        }

        /// <summary>
        /// テキストを非同期で形態素解析する
        /// </summary>
        public async UniTask<LinderaToken[]> TokenizeAsync(string text, CancellationToken cancellationToken = default)
        {
            // WebGLではメインスレッドで実行する必要がある
            await UniTask.Yield(cancellationToken);
            return Tokenize(text);
        }

        /// <summary>
        /// テキストから読み仮名を取得する
        /// </summary>
        public string GetReading(string text)
        {
            var tokens = Tokenize(text);
            var sb = new StringBuilder();

            foreach (var token in tokens)
            {
                sb.Append(token.Reading ?? token.Surface);
            }

            return sb.ToString();
        }

        /// <summary>
        /// テキストから読み仮名を非同期で取得する
        /// </summary>
        public async UniTask<string> GetReadingAsync(string text, CancellationToken cancellationToken = default)
        {
            var tokens = await TokenizeAsync(text, cancellationToken);
            var sb = new StringBuilder();

            foreach (var token in tokens)
            {
                sb.Append(token.Reading ?? token.Surface);
            }

            return sb.ToString();
        }

        /// <summary>
        /// JSONからトークン配列をパース
        /// </summary>
        private LinderaToken[] ParseTokensFromJson(string json)
        {
            // Simple JSON parsing for token data
            // Format: {"tokens":[{"surface":"...", "reading":"...", "pos":"..."}]}

            if (string.IsNullOrEmpty(json))
            {
                return Array.Empty<LinderaToken>();
            }

            try
            {
                // Use Unity's JsonUtility or simple parsing
                var wrapper = JsonUtility.FromJson<TokenResultWrapper>(json);
                if (wrapper?.tokens == null)
                {
                    return Array.Empty<LinderaToken>();
                }

                var result = new LinderaToken[wrapper.tokens.Length];
                for (int i = 0; i < wrapper.tokens.Length; i++)
                {
                    var t = wrapper.tokens[i];
                    var details = new[] { t.pos ?? "UNK" };
                    if (!string.IsNullOrEmpty(t.reading))
                    {
                        // Expand details to include reading at index 7 (IPADIC format)
                        details = new[] { t.pos ?? "UNK", "*", "*", "*", "*", "*", t.surface, t.reading, t.reading };
                    }

                    result[i] = new LinderaToken(
                        t.surface ?? "",
                        (uint)t.byte_start,
                        (uint)t.byte_end,
                        (uint)i,
                        details
                    );
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LinderaWebGL] Failed to parse tokens: {e.Message}");
                return Array.Empty<LinderaToken>();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LinderaTokenizerWebGL));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_tokenizerId != 0)
            {
                LinderaWebGL_DestroyTokenizer(_tokenizerId);
                _tokenizerId = 0;
            }

            _disposed = true;
        }

        /// <summary>
        /// JSON deserialization helper
        /// </summary>
        [Serializable]
        private class TokenResultWrapper
        {
            public TokenData[] tokens;
        }

        [Serializable]
        private class TokenData
        {
            public string surface;
            public string reading;
            public string pos;
            public int byte_start;
            public int byte_end;
        }
    }
}
#endif
