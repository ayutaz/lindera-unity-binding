using System;

namespace LinderaUnityBinding
{
    /// <summary>
    /// Lindera操作で発生する例外
    /// </summary>
    /// <remarks>
    /// この例外は、ネイティブライブラリの呼び出しが失敗した場合や、
    /// トークナイザーの作成・解析処理中にエラーが発生した場合にスローされます。
    /// </remarks>
    public class LinderaException : Exception
    {
        /// <summary>
        /// 指定されたメッセージで例外を作成
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public LinderaException(string message) : base(message)
        {
        }

        /// <summary>
        /// 指定されたメッセージと内部例外で例外を作成
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="innerException">内部例外</param>
        public LinderaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
