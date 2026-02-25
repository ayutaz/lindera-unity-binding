# Lindera Unity Binding

[日本語](README.md)

Japanese morphological analyzer for Unity using Lindera (Rust-based) via FFI bindings.

**[WebGL Demo](https://ayutaz.github.io/lindera-unity-binding/)**

## Demo Video

[![Lindera Unity Binding Demo](https://img.youtube.com/vi/9yte6Px-Qc4/hqdefault.jpg)](https://youtu.be/9yte6Px-Qc4)

## Features

- Japanese text tokenization
- **N-Best tokenization** — retrieve multiple analysis candidates ranked by cost
- Reading (furigana) extraction from IPADIC dictionary
- Part-of-speech tagging
- Multi-platform support (Windows, macOS, Linux, iOS, Android, **WebGL**)
- Async operations with UniTask

## Installation

### Via Unity Package Manager (Git URL)

1. Open Window > Package Manager
2. Click "+" button and select "Add package from git URL..."
3. Enter: `https://github.com/ayutaz/lindera-unity-binding.git?path=Assets/Lindera`

## Requirements

- Unity 2021.3 or later
- [UniTask](https://github.com/Cysharp/UniTask) 2.5.0 or later

### Sample Scene Requirements

The sample scene requires additional packages (automatically installed with Unity):

- TextMeshPro 3.0.9 or later
- Input System 1.11.2 or later
- uGUI 2.0.0 or later

## Usage

### Recommended: Factory Pattern (All Platforms)

```csharp
using LinderaUnityBinding;
using Cysharp.Threading.Tasks;

async UniTaskVoid Start()
{
    // Create platform-appropriate tokenizer asynchronously
    // WASM initialization is automatic on WebGL
    using var tokenizer = await LinderaTokenizerFactory.CreateAsync();

    var tokens = tokenizer.Tokenize("東京都に住んでいます");

    foreach (var token in tokens)
    {
        Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
    }
}
```

### Native Platforms Only

```csharp
using LinderaUnityBinding;

// Create tokenizer (not available on WebGL)
using var tokenizer = new LinderaTokenizer();

// Tokenize text
var tokens = tokenizer.Tokenize("東京都に住んでいます");

foreach (var token in tokens)
{
    Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
}
```

### Token Properties

| Property | Description |
|----------|-------------|
| `Surface` | Surface form of the token |
| `Reading` | Reading in katakana (from IPADIC) |
| `PartOfSpeech` | Part-of-speech tag |
| `ByteStart` | Start byte position in original text |
| `ByteEnd` | End byte position in original text |

### Async Tokenization

```csharp
using LinderaUnityBinding;
using Cysharp.Threading.Tasks;

async UniTaskVoid TokenizeAsync()
{
    using var tokenizer = await LinderaTokenizerFactory.CreateAsync();
    var tokens = await tokenizer.TokenizeAsync("日本語テキスト");

    foreach (var token in tokens)
    {
        Debug.Log(token.Surface);
    }
}
```

### N-Best Tokenization

Retrieve multiple analysis candidates ranked by Viterbi cost. Useful for disambiguation or presenting alternative segmentations.

```csharp
using LinderaUnityBinding;

using var tokenizer = new LinderaTokenizer();

// Get top 5 analysis candidates
var result = tokenizer.TokenizeNBest("東京都", n: 5);

foreach (var path in result.Paths)
{
    Debug.Log($"Cost: {path.Cost}");
    foreach (var token in path.Tokens)
    {
        Debug.Log($"  {token.Surface} ({token.PartOfSpeech})");
    }
}
```

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `text` | `string` | — | Text to analyze |
| `n` | `int` | — | Number of analysis paths to retrieve |
| `deduplicate` | `bool` | `true` | Remove duplicate segmentations |
| `costThreshold` | `long?` | `null` | Max cost difference from best path |

#### Async Version

```csharp
var result = await tokenizer.TokenizeNBestAsync("東京都", n: 5);
```

> **Note:** N-Best API is not yet supported on WebGL (`NotSupportedException` will be thrown).

## WebGL Support

On WebGL, lindera-wasm (official WASM build) is used for morphological analysis.

- **Always use `LinderaTokenizerFactory`** — Direct `LinderaTokenizer` won't work on WebGL
- **Async initialization required** — WASM module loading takes time
- N-Best API is not yet supported on WebGL

```csharp
if (LinderaTokenizerFactory.IsWebGL)
{
    Debug.Log("Running on WebGL with WASM");
}
```

## Important Notes

### Thread Safety

`LinderaTokenizer` is **NOT thread-safe**. Do not use the same instance from multiple threads simultaneously. For multi-threaded usage:

- Create a separate `LinderaTokenizer` instance for each thread, or
- Use proper synchronization mechanisms

When using `TokenizeAsync`, do not call it multiple times concurrently on the same instance.

### Resource Management

Always dispose the tokenizer after use. The recommended pattern is using `using` statements:

```csharp
using (var tokenizer = new LinderaTokenizer())
{
    var tokens = tokenizer.Tokenize("テキスト");
    // Process tokens
} // Tokenizer is automatically disposed here
```

### Error Handling

The tokenizer may throw the following exceptions:

| Exception | Description |
|-----------|-------------|
| `LinderaException` | Native library operation failed |
| `ObjectDisposedException` | Tokenizer was already disposed |
| `DllNotFoundException` | Native library not found (check Plugins directory) |

## Sample Scene

A sample scene is included in `Assets/Samples/Lindera/BasicUsage/`:

1. Open `LinderaSample.unity` via menu: **Lindera > Open Sample Scene**
2. Run menu: **Lindera > Setup Sample Scene** (creates UGUI with TextMeshPro and Japanese font)
3. Enter Play Mode
4. Enter Japanese text and click "Tokenize" to see results

The sample uses:
- UGUI with TextMeshPro for high-quality text rendering
- Noto Sans CJK JP font for Japanese character support
- Input System for modern input handling

## Supported Platforms

| Platform | Architecture | Library |
|----------|-------------|---------|
| Windows | x64 | `lindera_ffi.dll` |
| macOS | Universal (x64 + ARM64) | `liblindera_ffi.dylib` |
| Linux | x64 | `liblindera_ffi.so` |
| iOS | ARM64 | `liblindera_ffi.a` (static) |
| Android | ARM64 | `liblindera_ffi.so` |
| Android | ARMv7 | `liblindera_ffi.so` |
| WebGL | WASM | `lindera-wasm` (npm) |

## Building Native Libraries

Native libraries are automatically built by CI/CD when releasing. For local development, you can build them manually:

### Prerequisites

- Rust 1.70 or later (`rustup` recommended)

### Windows (x64)

```bash
cd native/lindera-ffi
cargo build --release --target x86_64-pc-windows-msvc
# Output: target/x86_64-pc-windows-msvc/release/lindera_ffi.dll
```

### macOS (Universal Binary)

```bash
cd native/lindera-ffi
rustup target add x86_64-apple-darwin aarch64-apple-darwin
cargo build --release --target x86_64-apple-darwin
cargo build --release --target aarch64-apple-darwin
lipo -create \
  target/x86_64-apple-darwin/release/liblindera_ffi.dylib \
  target/aarch64-apple-darwin/release/liblindera_ffi.dylib \
  -output liblindera_ffi.dylib
```

### Linux (x64)

```bash
cd native/lindera-ffi
cargo build --release --target x86_64-unknown-linux-gnu
# Output: target/x86_64-unknown-linux-gnu/release/liblindera_ffi.so
```

### iOS (ARM64)

```bash
cd native/lindera-ffi
rustup target add aarch64-apple-ios
cargo build --release --target aarch64-apple-ios
# Output: target/aarch64-apple-ios/release/liblindera_ffi.a
```

### Android (ARM64/ARMv7)

Requires Android NDK. See `.github/workflows/build-native.yml` for detailed setup.

```bash
cd native/lindera-ffi
rustup target add aarch64-linux-android armv7-linux-androideabi
cargo build --release --target aarch64-linux-android
cargo build --release --target armv7-linux-androideabi
```

### Copying Built Libraries

After building, copy the libraries to the appropriate Plugins directory:

| Platform | Source | Destination |
|----------|--------|-------------|
| Windows | `target/x86_64-pc-windows-msvc/release/lindera_ffi.dll` | `Plugins/x86_64/` |
| macOS | `liblindera_ffi.dylib` (universal) | `Plugins/macOS/` |
| Linux | `target/x86_64-unknown-linux-gnu/release/liblindera_ffi.so` | `Plugins/Linux/` |
| iOS | `target/aarch64-apple-ios/release/liblindera_ffi.a` | `Plugins/iOS/` |
| Android ARM64 | `target/aarch64-linux-android/release/liblindera_ffi.so` | `Plugins/Android/libs/arm64-v8a/` |
| Android ARMv7 | `target/armv7-linux-androideabi/release/liblindera_ffi.so` | `Plugins/Android/libs/armeabi-v7a/` |

## License

Apache-2.0
