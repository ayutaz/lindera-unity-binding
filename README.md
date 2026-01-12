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

- Unity 6000.3.2f1 or later
- Rust 1.70+ (for building native library)
- [csbindgen](https://github.com/Cysharp/csbindgen) 1.9+
- [UniTask](https://github.com/Cysharp/UniTask)
- [Lindera](https://github.com/lindera/lindera) 2.0+

## Installation

Coming soon.

## Usage

Coming soon.

## Supported Platforms

| Platform | Architecture | Status |
|----------|--------------|--------|
| Windows | x86_64 | Planned |
| macOS | x86_64 / ARM64 | Planned |
| Linux | x86_64 | Planned |
| iOS | ARM64 | Planned |
| Android | ARM64 / ARMv7 | Planned |

## Building Native Library

```bash
# Rustプロジェクトディレクトリで実行
cargo build --release

# クロスコンパイル例
cargo build --release --target x86_64-pc-windows-msvc    # Windows
cargo build --release --target aarch64-apple-darwin      # macOS Apple Silicon
cargo build --release --target x86_64-apple-darwin       # macOS Intel
cargo build --release --target x86_64-unknown-linux-gnu  # Linux
```

## License

Apache License 2.0 - See [LICENSE](LICENSE) for details.
