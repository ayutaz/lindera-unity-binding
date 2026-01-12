# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

LinderaをUnityで動作させるためのプロジェクト。Linderaは日本語形態素解析エンジン（Rustで実装）であり、csbindgenを使用してRustからC#バインディングを生成し、Unity上で利用可能にする。

## 技術スタック

| 技術 | バージョン | 用途 |
|------|-----------|------|
| Unity | 6000.3.2f1 | メインプラットフォーム |
| Rust | 1.70+ | ネイティブライブラリ |
| Lindera | 2.0+ | 日本語形態素解析エンジン |
| csbindgen | 1.9+ | RustからC#バインディング自動生成 |
| UniTask | - | Unity向け非同期処理 |

## アーキテクチャ

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Unity (C#)     │────▶│  C# Bindings     │────▶│  Rust DLL       │
│  + UniTask      │     │  (csbindgen生成) │     │  (Lindera)      │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

### FFI設計方針
| 項目 | 方針 |
|------|------|
| メモリ管理 | Rust側でアロケート、C#側で明示的にfree呼び出し |
| 文字列エンコーディング | UTF-8で統一（Rust/C#両方対応） |
| エラーハンドリング | NULLポインタ返却でエラー表現 |

## ディレクトリ構成（UPMパッケージ）

```
Packages/
└── com.and.lindera/
    ├── package.json
    ├── README.md
    ├── LICENSE
    ├── CHANGELOG.md
    ├── Runtime/
    │   ├── Lindera.asmdef
    │   ├── LinderaTokenizer.cs
    │   ├── LinderaToken.cs
    │   ├── LinderaException.cs
    │   ├── NativeMethods.cs
    │   └── NativeMethodsGenerated.cs  # csbindgen自動生成
    ├── Tests/
    │   ├── Editor/
    │   │   └── Lindera.Editor.Tests.asmdef
    │   └── Runtime/
    │       └── Lindera.Runtime.Tests.asmdef
    └── Plugins/
        ├── x86_64/           # Windows 64-bit (.dll)
        ├── macOS/            # macOS Universal (.dylib)
        ├── Linux/            # Linux x64 (.so)
        ├── iOS/              # iOS (.a 静的ライブラリ)
        └── Android/
            └── libs/
                ├── arm64-v8a/    # Android ARM64 (.so)
                └── armeabi-v7a/  # Android ARMv7 (.so)

native/
└── lindera-ffi/          # Rust FFIライブラリ
    ├── Cargo.toml
    ├── build.rs          # csbindgen設定
    └── src/
        └── lib.rs
```

## ビルドコマンド

### Rust DLLのビルド
```bash
# Rustプロジェクトディレクトリで実行
cargo build --release
```

### クロスコンパイル
```bash
# Windows
cargo build --release --target x86_64-pc-windows-msvc

# macOS
cargo build --release --target aarch64-apple-darwin  # Apple Silicon
cargo build --release --target x86_64-apple-darwin   # Intel

# Linux
cargo build --release --target x86_64-unknown-linux-gnu

# iOS (静的ライブラリ)
cargo rustc --release --target aarch64-apple-ios --crate-type staticlib

# Android
cargo build --release --target aarch64-linux-android
cargo build --release --target armv7-linux-androideabi
```

### csbindgenでC#バインディング生成
```bash
# Cargo.tomlにcsbindgen設定後、ビルド時に自動生成
cargo build --release
```

## Rust FFI関数命名規則

| 関数名パターン | 用途 |
|---------------|------|
| `lindera_*_create` | リソース作成 |
| `lindera_*_destroy` | リソース破棄 |
| `lindera_tokenize` | トークナイズ実行 |
| `lindera_tokens_*` | トークン結果操作 |
| `lindera_string_free` | 文字列メモリ解放 |

## 開発時の注意点

- 非同期処理は必ずUniTaskを使用すること
- ネイティブライブラリのメモリ解放を忘れないこと（IDisposableパターン推奨）
- iOSでは`[DllImport("__Internal")]`を使用（静的リンク）
- csbindgenの`csharp_use_function_pointer(false)`でUnity互換性確保

## 関連リソース

- 事前検証用Rustプロジェクト: `C:\Users\yuta\Desktop\Private\uG2P\verification\lindera-test`
- Lindera公式: https://github.com/lindera/lindera
- csbindgen: https://github.com/Cysharp/csbindgen
