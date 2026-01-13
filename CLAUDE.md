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
| UniTask | 2.5.10 | Unity向け非同期処理 |
| TextMeshPro | 3.0.9 | テキストレンダリング（サンプル用） |
| Input System | 1.11.2 | 入力処理（サンプル用） |

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

## ディレクトリ構成

```
Assets/
├── Lindera/                  # UPMパッケージ本体（配布対象）
│   ├── package.json
│   ├── README.md
│   ├── LICENSE
│   ├── CHANGELOG.md
│   ├── Runtime/
│   │   ├── Lindera.asmdef
│   │   ├── LinderaTokenizer.cs
│   │   ├── LinderaToken.cs
│   │   ├── LinderaException.cs
│   │   ├── NativeMethods.cs
│   │   └── NativeMethodsGenerated.cs  # csbindgen自動生成
│   └── Plugins/
│       └── x86_64/           # Windows 64-bit (.dll)
│
├── Samples/
│   └── Lindera/
│       └── BasicUsage/       # サンプルシーン
│           ├── LinderaSample.unity
│           ├── Lindera.Samples.asmdef
│           ├── Scripts/
│           │   └── LinderaSampleUI.cs  # UGUI + TextMeshPro
│           ├── Editor/
│           │   ├── Lindera.Samples.Editor.asmdef
│           │   └── LinderaSampleSetup.cs  # UIセットアップ
│           └── Fonts/
│               ├── NotoSansJP-Regular.ttf
│               └── NotoSansJP-Regular SDF.asset
│
├── Tests/                    # 開発用テスト（配布には含まれない）
│   ├── Editor/
│   │   └── Lindera.Tests.Editor.asmdef
│   └── Runtime/
│       └── Lindera.Tests.Runtime.asmdef
│
└── TextMesh Pro/             # TMP Essential Resources

Packages/
└── manifest.json

native/
└── lindera-ffi/              # Rust FFIライブラリ
    ├── Cargo.toml
    ├── build.rs              # csbindgen設定
    └── src/
        └── lib.rs
```

## サンプルシーンの使用方法

1. メニュー: **Lindera > Open Sample Scene** でシーンを開く
2. メニュー: **Lindera > Setup Sample Scene** でUIを自動生成
   - Canvas、EventSystem、TextMeshPro UIが作成される
   - Noto Sans CJK JP フォントが自動適用される
3. Play Modeで動作確認

## UPMインストール方法

```
https://github.com/ayutaz/lindera-unity-binding.git?path=Assets/Lindera
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
- サンプルUIはUGUI + TextMeshProを使用（OnGUIは非推奨）
- Input System Packageを使用（旧InputManagerは非対応）

## 関連リソース

- 事前検証用Rustプロジェクト: `C:\Users\yuta\Desktop\Private\uG2P\verification\lindera-test`
- Lindera公式: https://github.com/lindera/lindera
- csbindgen: https://github.com/Cysharp/csbindgen
