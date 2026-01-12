# lindera-unity-binding

Lindera Japanese morphological analyzer bindings for Unity.

[Lindera](https://github.com/lindera/lindera)はRustで実装された日本語形態素解析エンジンです。このプロジェクトはcsbindgenを使用してRustからC#バインディングを生成し、Unity上でLinderaを利用可能にします。

## Features

- 日本語テキストの形態素解析（トークナイズ）
- UniTaskによる非同期処理対応
- Windows/macOS/Linux対応予定

## Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Unity (C#)     │────▶│  C# Bindings     │────▶│  Rust DLL       │
│  + UniTask      │     │  (csbindgen生成) │     │  (Lindera)      │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

## Requirements

- Unity 6000.1 or later
- Rust (for building native library)
- [csbindgen](https://github.com/Cysharp/csbindgen)
- [UniTask](https://github.com/Cysharp/UniTask)

## Installation

Coming soon.

## Usage

Coming soon.

## Building Native Library

```bash
# Rustプロジェクトディレクトリで実行
cargo build --release
```

## License

Apache License 2.0 - See [LICENSE](LICENSE) for details.
