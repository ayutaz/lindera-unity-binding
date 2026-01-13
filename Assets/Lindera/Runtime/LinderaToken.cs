namespace Lindera
{
    /// <summary>
    /// 形態素解析結果のトークン
    /// </summary>
    public readonly struct LinderaToken
    {
        /// <summary>
        /// 表層形（トークンのテキスト）
        /// </summary>
        public string Surface { get; }

        /// <summary>
        /// バイト単位の開始位置
        /// </summary>
        public uint ByteStart { get; }

        /// <summary>
        /// バイト単位の終了位置
        /// </summary>
        public uint ByteEnd { get; }

        /// <summary>
        /// トークン位置
        /// </summary>
        public uint Position { get; }

        /// <summary>
        /// 品詞情報等の詳細（CSV形式を分割したもの）
        /// </summary>
        public string[] Details { get; }

        /// <summary>
        /// 品詞
        /// </summary>
        public string PartOfSpeech => Details?.Length > 0 ? Details[0] : null;

        /// <summary>
        /// 読み（カタカナ）- IPADICの場合はindex 7
        /// </summary>
        public string Reading => Details?.Length > 7 ? Details[7] : null;

        /// <summary>
        /// 原形 - IPADICの場合はindex 6
        /// </summary>
        public string BaseForm => Details?.Length > 6 ? Details[6] : null;

        /// <summary>
        /// 新しいトークンを作成
        /// </summary>
        /// <param name="surface">表層形</param>
        /// <param name="byteStart">バイト単位の開始位置</param>
        /// <param name="byteEnd">バイト単位の終了位置</param>
        /// <param name="position">トークン位置</param>
        /// <param name="details">品詞情報等の詳細</param>
        public LinderaToken(string surface, uint byteStart, uint byteEnd, uint position, string[] details)
        {
            Surface = surface;
            ByteStart = byteStart;
            ByteEnd = byteEnd;
            Position = position;
            Details = details;
        }

        /// <summary>
        /// トークンの文字列表現を取得
        /// </summary>
        /// <returns>表層形と品詞を含む文字列</returns>
        public override string ToString()
        {
            return $"{Surface} [{PartOfSpeech}]";
        }
    }
}
