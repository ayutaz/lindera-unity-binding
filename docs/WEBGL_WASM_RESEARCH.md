# WebGL/WASM対応 調査レポート

## 調査日: 2026-01-13

## 概要

LinderaはRustで実装された日本語形態素解析エンジンであり、公式でWASM (WebAssembly) サポートを提供しています。

## 発見事項

### 1. lindera-wasm の存在

Lindera公式が `lindera-wasm` パッケージを提供しています。

| 項目 | 内容 |
|------|------|
| リポジトリ | https://github.com/lindera/lindera-wasm |
| 最新バージョン | 2.0.0 (2026-01-11リリース) |
| ライセンス | MIT |
| Rust Edition | 2024 |

### 2. 利用可能なnpmパッケージ

| パッケージ名 | 辞書 |
|-------------|------|
| `lindera-wasm` | CJK全辞書 (IPADIC, ko-dic, CC-CEDICT) |
| `lindera-wasm-ipadic` | 日本語 (IPADIC) |
| `lindera-wasm-unidic` | 日本語 (UniDic) |
| `lindera-wasm-ko-dic` | 韓国語 |
| `lindera-wasm-cc-cedict` | 中国語 |

### 3. 依存関係

```
lindera 2.0
wasm-bindgen 0.2.106
serde 1.0.228
serde_json 1.0.149
js-sys 0.3.83
serde-wasm-bindgen 0.6.5
once_cell 1.21.3
```

### 4. ビルド方法

```bash
# wasm-packが必要
wasm-pack build --release --features=ipadic --target=bundler
```

## Unity WebGL統合の選択肢

### Option A: 既存の lindera-wasm を使用 (推奨)

**メリット:**
- 公式サポート、メンテナンスされている
- すぐに使える
- 辞書が埋め込み済み

**デメリット:**
- JavaScript APIなので、Unity側でjslib経由で呼び出す必要あり
- 既存のC# APIとは異なるインターフェース

**実装手順:**
1. lindera-wasm-ipadicのnpmパッケージをダウンロード
2. `.wasm` と `.js` ファイルをUnityプロジェクトに配置
3. Unity用の `.jslib` プラグインを作成
4. C#からJavaScript経由でWASMを呼び出すラッパーを作成

### Option B: 独自のWASMビルドを作成

**メリット:**
- 既存のFFI APIと統一感のあるインターフェース
- 不要な機能を削除して軽量化可能

**デメリット:**
- wasm-bindgenの設定が必要
- メンテナンスコストが増加
- 辞書の埋め込み方法を検討する必要あり

**実装手順:**
1. `native/lindera-ffi/`にWASMターゲット用の設定を追加
2. `wasm-bindgen`依存関係を追加
3. WASM用のエクスポート関数を作成
4. wasm-packでビルド
5. Unity用の統合を作成

## 推奨アプローチ

**Option A (lindera-wasm使用) を推奨します。**

理由:
1. 公式がメンテナンスしているため信頼性が高い
2. 実装工数が少ない
3. 辞書データの管理が不要

## 実装状況

### 完了

1. [x] lindera-wasm-ipadicをダウンロードして構造確認
2. [x] Unity WebGL用のjslibプラグイン作成 (`LinderaWebGL.jslib`)
3. [x] C#側のWebGL用ラッパークラス作成 (`LinderaTokenizerWebGL.cs`)
4. [x] プラットフォーム別の切り替え機構実装 (`LinderaTokenizerFactory.cs`)
5. [x] インターフェース定義 (`ILinderaTokenizer.cs`)

### 残作業

1. [ ] jslibの完全実装（lindera-wasmとの連携）
2. [ ] StreamingAssetsへのWASMファイル配置
3. [ ] WebGLビルドでのテスト
4. [ ] ドキュメント更新

## 参考リンク

- [lindera-wasm GitHub](https://github.com/lindera/lindera-wasm)
- [lindera-wasm npm](https://www.npmjs.com/package/lindera-wasm)
- [lindera-wasm デモ](https://lindera.github.io/lindera-wasm/)
- [lib.rs - lindera-wasm](https://lib.rs/crates/lindera-wasm)

## アーキテクチャ図

### 現在 (ネイティブプラットフォーム)
```
Unity (C#) → P/Invoke → Native DLL (lindera-ffi)
```

### WebGL対応後
```
[ネイティブ]
Unity (C#) → P/Invoke → Native DLL (lindera-ffi)

[WebGL]
Unity (C#) → jslib → JavaScript → lindera-wasm (WASM)
```

### C#コード例 (プラットフォーム切り替え)
```csharp
public class LinderaTokenizer
{
    #if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL: JavaScript経由でWASMを呼び出し
    [DllImport("__Internal")]
    private static extern void LinderaWasm_Initialize();

    [DllImport("__Internal")]
    private static extern string LinderaWasm_Tokenize(string text);
    #else
    // ネイティブ: 既存のFFI
    [DllImport("lindera_ffi")]
    private static extern IntPtr lindera_tokenizer_create();
    #endif
}
```
