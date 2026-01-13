# Lindera Unity Binding

[English](#english) | 日本語

LinderaをUnityで使用するための日本語形態素解析ライブラリです。Rust製のLinderaをFFIバインディング経由で利用します。

**[WebGLデモ](https://ayutaz.github.io/lindera-unity-binding/)**

## 機能

- 日本語テキストのトークナイズ（形態素解析）
- IPADIC辞書による読み仮名（ふりがな）取得
- 品詞タグ付け
- マルチプラットフォーム対応（Windows, macOS, Linux, iOS, Android, **WebGL**）
- UniTaskによる非同期処理

## インストール

### Unity Package Manager (Git URL) 経由

1. Window > Package Manager を開く
2. 「+」ボタンをクリックし、「Add package from git URL...」を選択
3. 以下を入力: `https://github.com/ayutaz/lindera-unity-binding.git?path=Assets/Lindera`

## 要件

- Unity 2021.3 以降
- [UniTask](https://github.com/Cysharp/UniTask) 2.5.0 以降

## 使い方

### 推奨: ファクトリパターン（全プラットフォーム対応）

```csharp
using LinderaUnityBinding;
using Cysharp.Threading.Tasks;

async UniTaskVoid Start()
{
    // プラットフォームに応じたトークナイザーを非同期で作成
    // WebGLの場合はWASMの初期化が自動で行われます
    using var tokenizer = await LinderaTokenizerFactory.CreateAsync();

    // テキストをトークナイズ
    var tokens = tokenizer.Tokenize("東京都に住んでいます");

    foreach (var token in tokens)
    {
        Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
    }
}
```

### ネイティブプラットフォーム専用（従来の方法）

```csharp
using LinderaUnityBinding;

// トークナイザーを作成（WebGLでは使用不可）
using var tokenizer = new LinderaTokenizer();

// テキストをトークナイズ
var tokens = tokenizer.Tokenize("東京都に住んでいます");

foreach (var token in tokens)
{
    Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
}
```

### 非同期トークナイズ

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

## WebGL対応

WebGLプラットフォームでは、lindera-wasm（公式WASMビルド）を使用して形態素解析を行います。

### WebGL使用時の注意点

1. **必ず`LinderaTokenizerFactory`を使用してください** - 直接`LinderaTokenizer`を使用するとWebGLでは動作しません
2. **非同期初期化が必要です** - WASMモジュールの読み込みに時間がかかります
3. **StreamingAssetsにWASMファイルが必要です** - パッケージに含まれています

### プラットフォーム判定

```csharp
// WebGLかどうかを確認
if (LinderaTokenizerFactory.IsWebGL)
{
    Debug.Log("Running on WebGL with WASM");
}
```

## 重要な注意事項

### スレッドセーフティ

`LinderaTokenizer` は**スレッドセーフではありません**。マルチスレッド環境では、スレッドごとに別のインスタンスを作成するか、適切な同期機構を使用してください。

### リソース管理

使用後は必ず `using` ステートメントでDisposeしてください。

## ライセンス

Apache-2.0

---

<a name="english"></a>
# English

Japanese morphological analyzer for Unity using Lindera (Rust-based) via FFI bindings.

**[WebGL Demo](https://ayutaz.github.io/lindera-unity-binding/)**

## Features

- Japanese text tokenization
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

    // Tokenize text
    var tokens = tokenizer.Tokenize("東京都に住んでいます");

    foreach (var token in tokens)
    {
        Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
    }
}
```

### Native Platforms Only (Legacy)

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

## WebGL Support

On WebGL platform, lindera-wasm (official WASM build) is used for morphological analysis.

### WebGL Usage Notes

1. **Always use `LinderaTokenizerFactory`** - Direct `LinderaTokenizer` won't work on WebGL
2. **Async initialization required** - WASM module loading takes time
3. **WASM files in StreamingAssets required** - Included in package

### Platform Detection

```csharp
// Check if running on WebGL
if (LinderaTokenizerFactory.IsWebGL)
{
    Debug.Log("Running on WebGL with WASM");
}
```

## Important Notes

### Thread Safety

`LinderaTokenizer` is **NOT thread-safe**. In multi-threaded environments, create a separate instance for each thread or use proper synchronization.

### Resource Management

Always dispose using `using` statements.

## License

Apache-2.0
