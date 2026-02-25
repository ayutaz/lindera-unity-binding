namespace LinderaUnityBinding
{
    /// <summary>
    /// N-Best形態素解析の個別パス結果
    /// </summary>
    public readonly struct LinderaNBestPath
    {
        /// <summary>
        /// このパスのトークン配列
        /// </summary>
        public LinderaToken[] Tokens { get; }

        /// <summary>
        /// このパスのViterbiコスト（低いほど良い）
        /// </summary>
        public long Cost { get; }

        /// <summary>
        /// 新しいN-Bestパスを作成
        /// </summary>
        /// <param name="tokens">トークン配列</param>
        /// <param name="cost">Viterbiコスト</param>
        public LinderaNBestPath(LinderaToken[] tokens, long cost)
        {
            Tokens = tokens;
            Cost = cost;
        }
    }

    /// <summary>
    /// N-Best形態素解析の結果
    /// </summary>
    public readonly struct LinderaNBestResult
    {
        /// <summary>
        /// コスト順にソートされた解析パスの配列
        /// </summary>
        public LinderaNBestPath[] Paths { get; }

        /// <summary>
        /// 新しいN-Best結果を作成
        /// </summary>
        /// <param name="paths">解析パスの配列</param>
        public LinderaNBestResult(LinderaNBestPath[] paths)
        {
            Paths = paths;
        }
    }
}
