using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Lindera
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
    public sealed class LinderaTokenizer : IDisposable
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
