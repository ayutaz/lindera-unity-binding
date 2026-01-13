using System;
using System.Text;

namespace Lindera
{
    /// <summary>
    /// Linderaネイティブライブラリへのバインディング
    /// csbindgenで生成されたNativeMethodsGeneratedのラッパー
    /// </summary>
    public static unsafe class NativeMethods
    {
        /// <summary>
        /// トークナイザーを作成
        /// </summary>
        /// <returns>トークナイザーハンドル（失敗時はIntPtr.Zero）</returns>
        public static IntPtr lindera_tokenizer_create()
        {
            var ptr = NativeMethodsGenerated.lindera_tokenizer_create();
            return (IntPtr)ptr;
        }

        /// <summary>
        /// トークナイザーを破棄
        /// </summary>
        /// <param name="handle">トークナイザーハンドル</param>
        public static void lindera_tokenizer_destroy(IntPtr handle)
        {
            NativeMethodsGenerated.lindera_tokenizer_destroy((LinderaTokenizerHandle*)handle);
        }

        /// <summary>
        /// テキストをトークナイズ
        /// </summary>
        /// <param name="handle">トークナイザーハンドル</param>
        /// <param name="text">UTF-8エンコードされたテキスト</param>
        /// <param name="text_len">テキストのバイト長</param>
        /// <returns>トークン結果ハンドル（失敗時はIntPtr.Zero）</returns>
        public static IntPtr lindera_tokenize(IntPtr handle, byte* text, int text_len)
        {
            var ptr = NativeMethodsGenerated.lindera_tokenize(
                (LinderaTokenizerHandle*)handle,
                text,
                text_len
            );
            return (IntPtr)ptr;
        }

        /// <summary>
        /// トークン数を取得
        /// </summary>
        /// <param name="handle">トークン結果ハンドル</param>
        /// <returns>トークン数（エラー時は-1）</returns>
        public static int lindera_tokens_count(IntPtr handle)
        {
            return NativeMethodsGenerated.lindera_tokens_count((LinderaTokenResultHandle*)handle);
        }

        /// <summary>
        /// 指定インデックスのトークンを取得
        /// </summary>
        /// <param name="handle">トークン結果ハンドル</param>
        /// <param name="index">インデックス</param>
        /// <returns>トークンデータへのポインタ（失敗時はnull）</returns>
        internal static TokenData* lindera_tokens_get(IntPtr handle, int index)
        {
            return NativeMethodsGenerated.lindera_tokens_get((LinderaTokenResultHandle*)handle, index);
        }

        /// <summary>
        /// トークン結果を破棄
        /// </summary>
        /// <param name="handle">トークン結果ハンドル</param>
        public static void lindera_tokens_destroy(IntPtr handle)
        {
            NativeMethodsGenerated.lindera_tokens_destroy((LinderaTokenResultHandle*)handle);
        }

        /// <summary>
        /// Rust側で確保した文字列を解放
        /// </summary>
        /// <param name="str">文字列ポインタ</param>
        public static void lindera_string_free(byte* str)
        {
            NativeMethodsGenerated.lindera_string_free(str);
        }

        /// <summary>
        /// 文字列の最大長（バッファオーバーラン防止用）
        /// </summary>
        private const int MaxStringLength = 1024 * 1024; // 1MB

        /// <summary>
        /// UTF-8バイトポインタからC#文字列に変換
        /// </summary>
        /// <param name="ptr">null終端されたUTF-8文字列へのポインタ</param>
        /// <returns>変換された文字列。ptrがnullの場合はnull</returns>
        /// <remarks>
        /// 安全性のため、最大1MBまでの文字列のみ処理します。
        /// null終端が見つからない場合は例外をスローします。
        /// </remarks>
        public static string PtrToStringUTF8(byte* ptr)
        {
            return PtrToStringUTF8(ptr, MaxStringLength);
        }

        /// <summary>
        /// UTF-8バイトポインタからC#文字列に変換（最大長指定）
        /// </summary>
        /// <param name="ptr">null終端されたUTF-8文字列へのポインタ</param>
        /// <param name="maxLength">読み取る最大バイト数</param>
        /// <returns>変換された文字列。ptrがnullの場合はnull</returns>
        /// <exception cref="InvalidOperationException">maxLength以内にnull終端が見つからない場合</exception>
        public static string PtrToStringUTF8(byte* ptr, int maxLength)
        {
            if (ptr == null) return null;
            if (maxLength <= 0) return string.Empty;

            int length = 0;
            while (length < maxLength && ptr[length] != 0)
            {
                length++;
            }

            // null終端が見つからなかった場合
            if (length >= maxLength && ptr[length] != 0)
            {
                throw new InvalidOperationException(
                    $"UTF-8 string exceeds maximum length of {maxLength} bytes or is not null-terminated.");
            }

            if (length == 0) return string.Empty;

            return Encoding.UTF8.GetString(ptr, length);
        }
    }
}
