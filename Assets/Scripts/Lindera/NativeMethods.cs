// NOTE: このファイルはcsbindgenで自動生成される予定です
// 現在はプレースホルダーとして手動で作成しています

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lindera
{
    /// <summary>
    /// FFIトークンデータ構造体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TokenData
    {
        public byte* text;
        public uint byte_start;
        public uint byte_end;
        public uint position;
        public byte* details;
    }

    /// <summary>
    /// Linderaネイティブライブラリへのバインディング
    /// </summary>
    public static unsafe class NativeMethods
    {
#if UNITY_IOS && !UNITY_EDITOR
        private const string DllName = "__Internal";
#else
        private const string DllName = "lindera_ffi";
#endif

        /// <summary>
        /// トークナイザーを作成
        /// </summary>
        /// <returns>トークナイザーハンドル（失敗時はIntPtr.Zero）</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_tokenizer_create")]
        public static extern IntPtr lindera_tokenizer_create();

        /// <summary>
        /// トークナイザーを破棄
        /// </summary>
        /// <param name="handle">トークナイザーハンドル</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_tokenizer_destroy")]
        public static extern void lindera_tokenizer_destroy(IntPtr handle);

        /// <summary>
        /// テキストをトークナイズ
        /// </summary>
        /// <param name="handle">トークナイザーハンドル</param>
        /// <param name="text">UTF-8エンコードされたテキスト</param>
        /// <param name="text_len">テキストのバイト長</param>
        /// <returns>トークン結果ハンドル（失敗時はIntPtr.Zero）</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_tokenize")]
        public static extern IntPtr lindera_tokenize(IntPtr handle, byte* text, int text_len);

        /// <summary>
        /// トークン数を取得
        /// </summary>
        /// <param name="handle">トークン結果ハンドル</param>
        /// <returns>トークン数（エラー時は-1）</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_tokens_count")]
        public static extern int lindera_tokens_count(IntPtr handle);

        /// <summary>
        /// 指定インデックスのトークンを取得
        /// </summary>
        /// <param name="handle">トークン結果ハンドル</param>
        /// <param name="index">インデックス</param>
        /// <returns>トークンデータへのポインタ（失敗時はnull）</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_tokens_get")]
        public static extern TokenData* lindera_tokens_get(IntPtr handle, int index);

        /// <summary>
        /// トークン結果を破棄
        /// </summary>
        /// <param name="handle">トークン結果ハンドル</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_tokens_destroy")]
        public static extern void lindera_tokens_destroy(IntPtr handle);

        /// <summary>
        /// Rust側で確保した文字列を解放
        /// </summary>
        /// <param name="str">文字列ポインタ</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lindera_string_free")]
        public static extern void lindera_string_free(byte* str);

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
