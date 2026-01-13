using System;

namespace Lindera
{
    /// <summary>
    /// Lindera操作で発生する例外
    /// </summary>
    public class LinderaException : Exception
    {
        public LinderaException(string message) : base(message)
        {
        }

        public LinderaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
