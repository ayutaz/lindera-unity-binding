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

        /// <summary>
        /// テキストをN-Best形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="n">取得する解析パス数</param>
        /// <param name="deduplicate">同一境界の結果を重複排除するか</param>
        /// <param name="costThreshold">コスト閾値（ベストコストからの差分上限）</param>
        /// <returns>N-Best解析結果</returns>
        LinderaNBestResult TokenizeNBest(string text, int n, bool deduplicate = true, long? costThreshold = null);

        /// <summary>
        /// テキストを非同期でN-Best形態素解析する
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <param name="n">取得する解析パス数</param>
        /// <param name="deduplicate">同一境界の結果を重複排除するか</param>
        /// <param name="costThreshold">コスト閾値（ベストコストからの差分上限）</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>N-Best解析結果</returns>
        UniTask<LinderaNBestResult> TokenizeNBestAsync(string text, int n, bool deduplicate = true, long? costThreshold = null, CancellationToken cancellationToken = default);
    }
}
