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
        /// UTF-8バイトポインタからC#文字列に変換
        /// </summary>
        public static string PtrToStringUTF8(byte* ptr)
        {
            if (ptr == null) return null;

            int length = 0;
            while (ptr[length] != 0) length++;

            if (length == 0) return string.Empty;

            return Encoding.UTF8.GetString(ptr, length);
        }
    }
}
