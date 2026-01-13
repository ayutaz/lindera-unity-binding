# Lindera Unity Binding

[English](README_EN.md)

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

### サンプルシーンの要件

サンプルシーンには追加パッケージが必要です（Unityで自動インストール）:

- TextMeshPro 3.0.9 以降
- Input System 1.11.2 以降
- uGUI 2.0.0 以降

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

### トークンのプロパティ

| プロパティ | 説明 |
|-----------|------|
| `Surface` | 表層形（トークンのテキスト） |
| `Reading` | 読み（カタカナ、IPADIC由来） |
| `PartOfSpeech` | 品詞 |
| `ByteStart` | 元テキストでのバイト開始位置 |
| `ByteEnd` | 元テキストでのバイト終了位置 |

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

`LinderaTokenizer` は**スレッドセーフではありません**。複数のスレッドから同時に同じインスタンスを使用しないでください。マルチスレッド環境では:

- スレッドごとに別の `LinderaTokenizer` インスタンスを作成する、または
- 適切な同期機構を使用する

`TokenizeAsync` を使用する場合、同じインスタンスで複数回同時に呼び出さないでください。

### リソース管理

使用後は必ずトークナイザーをDisposeしてください。`using` ステートメントの使用を推奨します:

```csharp
using (var tokenizer = new LinderaTokenizer())
{
    var tokens = tokenizer.Tokenize("テキスト");
    // トークンを処理
} // ここでトークナイザーは自動的にDisposeされます
```

### エラーハンドリング

トークナイザーは以下の例外をスローする可能性があります:

| 例外 | 説明 |
|-----|------|
| `LinderaException` | ネイティブライブラリ操作の失敗 |
| `ObjectDisposedException` | トークナイザーが既にDisposeされている |
| `DllNotFoundException` | ネイティブライブラリが見つからない（Pluginsディレクトリを確認） |

## サンプルシーン

`Assets/Samples/Lindera/BasicUsage/` にサンプルシーンが含まれています:

1. メニュー **Lindera > Open Sample Scene** でシーンを開く
2. メニュー **Lindera > Setup Sample Scene** を実行（UGUI + TextMeshPro + 日本語フォントを作成）
3. Play Modeに入る
4. 日本語テキストを入力し、「Tokenize」をクリックして結果を確認

サンプルで使用しているもの:
- 高品質テキストレンダリングのためのUGUI + TextMeshPro
- 日本語文字サポートのためのNoto Sans CJK JPフォント
- モダンな入力処理のためのInput System

## 対応プラットフォーム

| プラットフォーム | アーキテクチャ | ライブラリ |
|-----------------|---------------|-----------|
| Windows | x64 | `lindera_ffi.dll` |
| macOS | Universal (x64 + ARM64) | `liblindera_ffi.dylib` |
| Linux | x64 | `liblindera_ffi.so` |
| iOS | ARM64 | `liblindera_ffi.a` (静的) |
| Android | ARM64 | `liblindera_ffi.so` |
| Android | ARMv7 | `liblindera_ffi.so` |
| WebGL | WASM | `lindera-wasm` (npm) |

## ネイティブライブラリのビルド

ネイティブライブラリはリリース時にCI/CDで自動的にビルドされます。ローカル開発用に手動でビルドすることも可能です:

### 前提条件

- Rust 1.70 以降（`rustup` 推奨）

### Windows (x64)

```bash
cd native/lindera-ffi
cargo build --release --target x86_64-pc-windows-msvc
# 出力: target/x86_64-pc-windows-msvc/release/lindera_ffi.dll
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
# 出力: target/x86_64-unknown-linux-gnu/release/liblindera_ffi.so
```

### iOS (ARM64)

```bash
cd native/lindera-ffi
rustup target add aarch64-apple-ios
cargo build --release --target aarch64-apple-ios
# 出力: target/aarch64-apple-ios/release/liblindera_ffi.a
```

### Android (ARM64/ARMv7)

Android NDKが必要です。詳細なセットアップは `.github/workflows/build-native.yml` を参照してください。

```bash
cd native/lindera-ffi
rustup target add aarch64-linux-android armv7-linux-androideabi
cargo build --release --target aarch64-linux-android
cargo build --release --target armv7-linux-androideabi
```

### ビルド後のライブラリ配置

ビルド後、適切なPluginsディレクトリにライブラリをコピーしてください:

| プラットフォーム | ソース | 配置先 |
|-----------------|--------|--------|
| Windows | `target/x86_64-pc-windows-msvc/release/lindera_ffi.dll` | `Plugins/x86_64/` |
| macOS | `liblindera_ffi.dylib` (universal) | `Plugins/macOS/` |
| Linux | `target/x86_64-unknown-linux-gnu/release/liblindera_ffi.so` | `Plugins/Linux/` |
| iOS | `target/aarch64-apple-ios/release/liblindera_ffi.a` | `Plugins/iOS/` |
| Android ARM64 | `target/aarch64-linux-android/release/liblindera_ffi.so` | `Plugins/Android/libs/arm64-v8a/` |
| Android ARMv7 | `target/armv7-linux-androideabi/release/liblindera_ffi.so` | `Plugins/Android/libs/armeabi-v7a/` |

## ライセンス

Apache-2.0
