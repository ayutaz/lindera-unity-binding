# Lindera Unity Binding

[English](#english) | 日本語

LinderaをUnityで使用するための日本語形態素解析ライブラリです。Rust製のLinderaをFFIバインディング経由で利用します。

## 機能

- 日本語テキストのトークナイズ（形態素解析）
- IPADIC辞書による読み仮名（ふりがな）取得
- 品詞タグ付け
- マルチプラットフォーム対応（Windows, macOS, Linux, iOS, Android）
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

```csharp
using LinderaUnityBinding;

// トークナイザーを作成
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
    using var tokenizer = new LinderaTokenizer();
    var tokens = await tokenizer.TokenizeAsync("日本語テキスト");
    foreach (var token in tokens)
    {
        Debug.Log(token.Surface);
    }
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

## Requirements

- Unity 2021.3 or later
- [UniTask](https://github.com/Cysharp/UniTask) 2.5.0 or later

## Usage

```csharp
using LinderaUnityBinding;

// Create tokenizer
using var tokenizer = new LinderaTokenizer();

// Tokenize text
var tokens = tokenizer.Tokenize("東京都に住んでいます");

foreach (var token in tokens)
{
    Debug.Log($"{token.Surface} - {token.Reading} ({token.PartOfSpeech})");
}
```

## Important Notes

### Thread Safety

`LinderaTokenizer` is **NOT thread-safe**. In multi-threaded environments, create a separate instance for each thread or use proper synchronization.

### Resource Management

Always dispose using `using` statements.

## License

Apache-2.0
