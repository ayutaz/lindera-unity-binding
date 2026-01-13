# Lindera for Unity

Japanese morphological analyzer for Unity using Lindera (Rust-based) via FFI bindings.

## Features

- Japanese text tokenization
- Reading (furigana) extraction from IPADIC dictionary
- Part-of-speech tagging
- Multi-platform support (Windows, macOS, Linux, iOS, Android)
- Async operations with UniTask

## Installation

### Via Unity Package Manager (Git URL)

1. Open Window > Package Manager
2. Click "+" button and select "Add package from git URL..."
3. Enter: `https://github.com/ayutaz/lindera-unity-binding.git?path=Assets/Lindera`

### Via OpenUPM (Coming Soon)

```bash
openupm add com.and.lindera
```

## Requirements

- Unity 2021.3 or later
- [UniTask](https://github.com/Cysharp/UniTask) 2.5.0 or later

### Sample Scene Requirements

The sample scene requires additional packages (automatically installed with Unity):

- TextMeshPro 3.0.9 or later
- Input System 1.11.2 or later
- uGUI 2.0.0 or later

## Usage

```csharp
using Lindera;

// Create tokenizer
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
using Lindera;
using Cysharp.Threading.Tasks;

async UniTaskVoid TokenizeAsync()
{
    using var tokenizer = new LinderaTokenizer();

    var tokens = await tokenizer.TokenizeAsync("日本語テキスト");

    foreach (var token in tokens)
    {
        Debug.Log(token.Surface);
    }
}
```

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

| Platform | Architecture | Status |
|----------|-------------|--------|
| Windows | x64 | Supported |
| macOS | Universal (x64 + ARM64) | Planned |
| Linux | x64 | Planned |
| iOS | ARM64 | Planned |
| Android | ARM64, ARMv7 | Planned |

## License

Apache-2.0
