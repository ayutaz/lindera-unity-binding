using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LinderaUnityBinding
{
    /// <summary>
    /// プラットフォームに応じたLinderaトークナイザーを作成するファクトリクラス
    /// </summary>
    public static class LinderaTokenizerFactory
    {
        private static bool _webglInitialized;

        /// <summary>
        /// 現在のプラットフォームがWebGLかどうか
        /// </summary>
        public static bool IsWebGL
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// WebGL環境でWASMが初期化済みかどうか
        /// </summary>
        public static bool IsWebGLInitialized
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return LinderaTokenizerWebGL.IsWasmInitialized;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// WebGL環境でWASMモジュールを初期化する
        /// </summary>
        /// <remarks>
        /// WebGL以外のプラットフォームでは何もせずに成功を返します。
        /// </remarks>
        /// <returns>初期化の成否</returns>
        public static async UniTask<bool> InitializeAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_webglInitialized)
            {
                return true;
            }

            var result = await LinderaTokenizerWebGL.InitializeAsync();
            _webglInitialized = result;
            return result;
#else
            // Non-WebGL platforms don't need initialization
            await UniTask.CompletedTask;
            return true;
#endif
        }

        /// <summary>
        /// プラットフォームに適したトークナイザーを作成する
        /// </summary>
        /// <returns>トークナイザーインスタンス</returns>
        /// <exception cref="LinderaException">
        /// WebGL環境で初期化されていない場合、またはトークナイザーの作成に失敗した場合
        /// </exception>
        /// <example>
        /// <code>
        /// // 推奨: 非同期で初期化してから作成
        /// await LinderaTokenizerFactory.InitializeAsync();
        /// using var tokenizer = LinderaTokenizerFactory.Create();
        /// var tokens = tokenizer.Tokenize("東京都");
        /// </code>
        /// </example>
        public static ILinderaTokenizer Create()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_webglInitialized)
            {
                throw new LinderaException(
                    "WebGL WASM module is not initialized. " +
                    "Call LinderaTokenizerFactory.InitializeAsync() before creating a tokenizer.");
            }
            return new LinderaTokenizerWebGL();
#else
            return new LinderaTokenizer();
#endif
        }

        /// <summary>
        /// プラットフォームに適したトークナイザーを非同期で作成する
        /// </summary>
        /// <remarks>
        /// 必要に応じてWASM初期化を行ってからトークナイザーを作成します。
        /// </remarks>
        /// <returns>トークナイザーインスタンス</returns>
        /// <exception cref="LinderaException">トークナイザーの作成に失敗した場合</exception>
        public static async UniTask<ILinderaTokenizer> CreateAsync()
        {
            var initialized = await InitializeAsync();
            if (!initialized)
            {
                throw new LinderaException("Failed to initialize Lindera");
            }

            return Create();
        }
    }
}
