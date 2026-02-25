using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LinderaUnityBinding
{
    /// <summary>
    /// Lindera形態素解析エンジンのラッパークラス
    /// </summary>
    /// <remarks>
    /// <para>
    /// このクラスはネイティブライブラリへのハンドルを保持するため、必ずDisposeを呼び出すか、
    /// usingステートメントを使用してリソースを解放してください。
    /// </para>
    /// <para>
    /// <strong>スレッドセーフティ:</strong> このクラスはスレッドセーフではありません。
    /// 同一インスタンスを複数のスレッドから同時に使用しないでください。
    /// マルチスレッド環境では、スレッドごとに別のインスタンスを作成するか、
    /// 適切な同期機構を使用してください。
    /// TokenizeAsyncメソッドはバックグラウンドスレッドで実行されるため、
    /// 同一インスタンスで複数のTokenizeAsync呼び出しを同時に行わないでください。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using (var tokenizer = new LinderaTokenizer())
    /// {
    ///     var tokens = tokenizer.Tokenize("東京都");
    ///     foreach (var token in tokens)
    ///     {
    ///         Debug.Log($"{token.Surface}: {token.Reading}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class LinderaTokenizer : ILinderaTokenizer
    {
        private IntPtr _handle;
        private bool _disposed;

        /// <summary>
        /// トークナイザーが有効かどうか
        /// </summary>
        public bool IsValid => _handle != IntPtr.Zero && !_disposed;

        /// <summary>
        /// トークナイザーを作成
        /// </summary>
        /// <exception cref="LinderaException">トークナイザーの作成に失敗した場合</exception>
        public LinderaTokenizer()
        {
            _handle = NativeMethods.lindera_tokenizer_create();
            if (_handle == IntPtr.Zero)
            {
                throw new LinderaException("Failed to create tokenizer");
            }
        }

        /// <summary>
        /// テキストを形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <returns>トークンの配列</returns>
        /// <exception cref="ObjectDisposedException">トークナイザーが破棄されている場合</exception>
        /// <exception cref="LinderaException">解析に失敗した場合</exception>
        public LinderaToken[] Tokenize(string text)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<LinderaToken>();
            }

            var utf8Bytes = Encoding.UTF8.GetBytes(text);
            IntPtr tokensHandle;

            unsafe
            {
                fixed (byte* textPtr = utf8Bytes)
                {
                    tokensHandle = NativeMethods.lindera_tokenize(_handle, textPtr, utf8Bytes.Length);
                }
            }

            if (tokensHandle == IntPtr.Zero)
            {
                throw new LinderaException("Failed to tokenize text");
            }

            try
            {
                return ExtractTokens(tokensHandle);
            }
            finally
            {
                NativeMethods.lindera_tokens_destroy(tokensHandle);
            }
        }

        /// <summary>
        /// テキストを非同期で形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>トークンの配列</returns>
        public async UniTask<LinderaToken[]> TokenizeAsync(string text, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<LinderaToken>();
            }

            // バックグラウンドスレッドで実行
            return await UniTask.RunOnThreadPool(() => Tokenize(text), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// テキストから読み仮名を取得する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <returns>読み仮名（カタカナ）</returns>
        public string GetReading(string text)
        {
            var tokens = Tokenize(text);
            var sb = new StringBuilder();

            foreach (var token in tokens)
            {
                // 読みがある場合は読みを、ない場合は表層形を使用
                sb.Append(token.Reading ?? token.Surface);
            }

            return sb.ToString();
        }

        /// <summary>
        /// テキストから読み仮名を非同期で取得する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>読み仮名（カタカナ）</returns>
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
        /// テキストをN-Best形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="n">取得する解析パス数</param>
        /// <param name="deduplicate">同一境界の結果を重複排除するか</param>
        /// <param name="costThreshold">コスト閾値（ベストコストからの差分上限）</param>
        /// <returns>N-Best解析結果</returns>
        /// <exception cref="ObjectDisposedException">トークナイザーが破棄されている場合</exception>
        /// <exception cref="ArgumentOutOfRangeException">nが0以下の場合</exception>
        /// <exception cref="LinderaException">解析に失敗した場合</exception>
        public LinderaNBestResult TokenizeNBest(string text, int n, bool deduplicate = true, long? costThreshold = null)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(text))
            {
                return new LinderaNBestResult(Array.Empty<LinderaNBestPath>());
            }

            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "N must be greater than 0");
            }

            var utf8Bytes = Encoding.UTF8.GetBytes(text);
            IntPtr nbestHandle;

            unsafe
            {
                fixed (byte* textPtr = utf8Bytes)
                {
                    nbestHandle = NativeMethods.lindera_tokenize_nbest(_handle, textPtr, utf8Bytes.Length, n, deduplicate, costThreshold);
                }
            }

            if (nbestHandle == IntPtr.Zero)
            {
                throw new LinderaException("Failed to tokenize text with N-Best");
            }

            try
            {
                return ExtractNBestResult(nbestHandle);
            }
            finally
            {
                NativeMethods.lindera_nbest_destroy(nbestHandle);
            }
        }

        /// <summary>
        /// テキストを非同期でN-Best形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="n">取得する解析パス数</param>
        /// <param name="deduplicate">同一境界の結果を重複排除するか</param>
        /// <param name="costThreshold">コスト閾値（ベストコストからの差分上限）</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>N-Best解析結果</returns>
        public async UniTask<LinderaNBestResult> TokenizeNBestAsync(string text, int n, bool deduplicate = true, long? costThreshold = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(text))
            {
                return new LinderaNBestResult(Array.Empty<LinderaNBestPath>());
            }

            return await UniTask.RunOnThreadPool(() => TokenizeNBest(text, n, deduplicate, costThreshold), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// ネイティブトークン結果からマネージド配列にトークンを抽出
        /// </summary>
        /// <param name="tokensHandle">トークン結果のネイティブハンドル</param>
        /// <returns>抽出されたトークンの配列</returns>
        private unsafe LinderaToken[] ExtractTokens(IntPtr tokensHandle)
        {
            int count = NativeMethods.lindera_tokens_count(tokensHandle);
            if (count <= 0)
            {
                return Array.Empty<LinderaToken>();
            }

            var result = new LinderaToken[count];

            for (int i = 0; i < count; i++)
            {
                var tokenPtr = NativeMethods.lindera_tokens_get(tokensHandle, i);
                if (tokenPtr == null)
                {
                    continue;
                }

                var surface = NativeMethods.PtrToStringUTF8(tokenPtr->text);
                var detailsStr = NativeMethods.PtrToStringUTF8(tokenPtr->details);
                var details = string.IsNullOrEmpty(detailsStr)
                    ? Array.Empty<string>()
                    : detailsStr.Split(',');

                result[i] = new LinderaToken(
                    surface,
                    tokenPtr->byte_start,
                    tokenPtr->byte_end,
                    tokenPtr->position,
                    details
                );
            }

            return result;
        }

        /// <summary>
        /// ネイティブN-Best結果からマネージド配列に結果を抽出
        /// </summary>
        /// <param name="nbestHandle">N-Best結果のネイティブハンドル</param>
        /// <returns>抽出されたN-Best結果</returns>
        private unsafe LinderaNBestResult ExtractNBestResult(IntPtr nbestHandle)
        {
            int pathCount = NativeMethods.lindera_nbest_path_count(nbestHandle);
            if (pathCount <= 0)
            {
                return new LinderaNBestResult(Array.Empty<LinderaNBestPath>());
            }

            var paths = new LinderaNBestPath[pathCount];

            for (int p = 0; p < pathCount; p++)
            {
                long cost = NativeMethods.lindera_nbest_get_path_cost(nbestHandle, p);
                int tokenCount = NativeMethods.lindera_nbest_get_path_token_count(nbestHandle, p);

                var tokens = new LinderaToken[tokenCount > 0 ? tokenCount : 0];
                for (int t = 0; t < tokenCount; t++)
                {
                    var tokenPtr = NativeMethods.lindera_nbest_get_path_token(nbestHandle, p, t);
                    if (tokenPtr == null) continue;

                    var surface = NativeMethods.PtrToStringUTF8(tokenPtr->text);
                    var detailsStr = NativeMethods.PtrToStringUTF8(tokenPtr->details);
                    var details = string.IsNullOrEmpty(detailsStr)
                        ? Array.Empty<string>()
                        : detailsStr.Split(',');

                    tokens[t] = new LinderaToken(
                        surface,
                        tokenPtr->byte_start,
                        tokenPtr->byte_end,
                        tokenPtr->position,
                        details
                    );
                }

                paths[p] = new LinderaNBestPath(tokens, cost);
            }

            return new LinderaNBestResult(paths);
        }

        /// <summary>
        /// 破棄済みの場合に例外をスロー
        /// </summary>
        /// <exception cref="ObjectDisposedException">既に破棄されている場合</exception>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LinderaTokenizer));
            }
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (_handle != IntPtr.Zero)
            {
                NativeMethods.lindera_tokenizer_destroy(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }
}
