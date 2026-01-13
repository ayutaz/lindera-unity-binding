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

## Usage

```csharp
using Lindera;
using Cysharp.Threading.Tasks;

// Create tokenizer
using var tokenizer = new LinderaTokenizer();

// Tokenize text
var tokens = tokenizer.Tokenize("東京都に住んでいます");

foreach (var token in tokens)
{
    Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
}
```

## Sample Scene

A sample scene is included in `Assets/Samples/Lindera/BasicUsage/`:

1. Open `LinderaSample.unity`
2. Run menu: **Lindera > Setup Sample Scene** (creates UGUI with TextMeshPro)
3. Enter Play Mode
4. Enter Japanese text and click "Tokenize" to see results

The sample uses UGUI with TextMeshPro for better text rendering.

## License

Apache-2.0
