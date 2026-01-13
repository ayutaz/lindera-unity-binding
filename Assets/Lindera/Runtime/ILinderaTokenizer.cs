using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LinderaUnityBinding
{
    /// <summary>
    /// Lindera形態素解析エンジンのインターフェース
    /// </summary>
    /// <remarks>
    /// プラットフォーム（ネイティブ/WebGL）に依存しない共通インターフェースを提供します。
    /// </remarks>
    public interface ILinderaTokenizer : IDisposable
    {
        /// <summary>
        /// トークナイザーが有効かどうか
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// テキストを形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <returns>トークンの配列</returns>
        LinderaToken[] Tokenize(string text);

        /// <summary>
        /// テキストを非同期で形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>トークンの配列</returns>
        UniTask<LinderaToken[]> TokenizeAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// テキストから読み仮名を取得する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <returns>読み仮名（カタカナ）</returns>
        string GetReading(string text);

        /// <summary>
        /// テキストから読み仮名を非同期で取得する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>読み仮名（カタカナ）</returns>
        UniTask<string> GetReadingAsync(string text, CancellationToken cancellationToken = default);
    }
}
