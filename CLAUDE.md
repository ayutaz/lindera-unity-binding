# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

LinderaをUnityで動作させるためのプロジェクト。Linderaは日本語形態素解析エンジン（Rustで実装）であり、csbindgenを使用してRustからC#バインディングを生成し、Unity上で利用可能にする。

## アーキテクチャ

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Unity (C#)     │────▶│  C# Bindings     │────▶│  Rust DLL       │
│  + UniTask      │     │  (csbindgen生成) │     │  (Lindera)      │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

### 技術スタック
- **Unity**: メインプラットフォーム
- **Rust/Lindera**: 形態素解析エンジン
- **csbindgen**: RustからC#バインディングを自動生成
- **UniTask**: Unity向け非同期処理ライブラリ

### FFI設計方針
| 項目 | 方針 |
|------|------|
| メモリ管理 | Rust側でアロケート、C#側で明示的にfree呼び出し |
| 文字列エンコーディング | UTF-8で統一（Rust/C#両方対応） |

## ビルドコマンド

### Rust DLLのビルド
```bash
# Rustプロジェクトディレクトリで実行
cargo build --release
```

### csbindgenでC#バインディング生成
```bash
# Cargo.tomlにcsbindgenの設定後
cargo build --release
```

### Unityプロジェクト
UnityエディタからBuild Settings経由でビルド

## 関連リソース

- 事前検証用Rustプロジェクト: `C:\Users\yuta\Desktop\Private\uG2P\verification\lindera-test`

## 開発時の注意点

- 非同期処理は必ずUniTaskを使用すること
- ネイティブライブラリのメモリ解放を忘れないこと（IDisposableパターン推奨）
- DLLはプラットフォームごとに適切なディレクトリに配置（Assets/Plugins/[platform]/）
