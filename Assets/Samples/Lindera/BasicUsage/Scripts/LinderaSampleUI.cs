using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LinderaUnityBinding;

namespace LinderaUnityBinding.Samples
{
    /// <summary>
    /// Linderaの基本的な使い方を示すサンプルUI（UGUI + TextMeshPro版）
    /// </summary>
    /// <remarks>
    /// このクラスはLinderaTokenizerの基本的な使用方法を示します。
    /// 入力フィールドにテキストを入力し、ボタンをクリックすると形態素解析結果が表示されます。
    /// </remarks>
    public class LinderaSampleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button tokenizeButton;

        [Header("Settings")]
        [SerializeField] private string defaultText = "東京都に住んでいます。今日はいい天気ですね。";

        private LinderaTokenizer _tokenizer;
        private bool _isInitialized;

        /// <summary>
        /// コンポーネントの初期化時にトークナイザーを作成
        /// </summary>
        private void Awake()
        {
            try
            {
                _tokenizer = new LinderaTokenizer();
                _isInitialized = _tokenizer.IsValid;

                if (!_isInitialized)
                {
                    Debug.LogError("[LinderaSampleUI] Failed to initialize tokenizer: tokenizer is not valid");
                }
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogError($"[LinderaSampleUI] Native library not found: {ex.Message}");
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LinderaSampleUI] Failed to initialize tokenizer: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// UI要素の初期設定とイベントリスナーの登録
        /// </summary>
        private void Start()
        {
            if (inputField != null)
            {
                inputField.text = defaultText;
            }

            if (tokenizeButton != null)
            {
                tokenizeButton.onClick.AddListener(OnTokenizeButtonClicked);
            }

            if (_isInitialized)
            {
                Tokenize(defaultText);
            }
            else
            {
                SetResultText("エラー: トークナイザーの初期化に失敗しました。\nネイティブライブラリが正しく配置されているか確認してください。");
            }
        }

        /// <summary>
        /// コンポーネント破棄時のクリーンアップ
        /// </summary>
        private void OnDestroy()
        {
            _tokenizer?.Dispose();

            if (tokenizeButton != null)
            {
                tokenizeButton.onClick.RemoveListener(OnTokenizeButtonClicked);
            }
        }

        /// <summary>
        /// トークナイズボタンクリック時のコールバック
        /// </summary>
        private void OnTokenizeButtonClicked()
        {
            if (!_isInitialized)
            {
                SetResultText("エラー: トークナイザーが初期化されていません。");
                return;
            }

            if (inputField != null)
            {
                Tokenize(inputField.text);
            }
        }

        /// <summary>
        /// テキストをトークナイズして結果を表示
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        private void Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                SetResultText("テキストを入力してください");
                return;
            }

            try
            {
                var tokens = _tokenizer.Tokenize(text);
                var sb = new StringBuilder();

                sb.AppendLine($"入力: {text}");
                sb.AppendLine($"トークン数: {tokens.Length}");
                sb.AppendLine("---");

                foreach (var token in tokens)
                {
                    sb.AppendLine($"[{token.Surface}]");
                    sb.AppendLine($"  読み: {token.Reading ?? "N/A"}");
                    sb.AppendLine($"  品詞: {token.PartOfSpeech}");
                    sb.AppendLine($"  位置: {token.ByteStart}-{token.ByteEnd}");
                    sb.AppendLine();
                }

                var result = sb.ToString();
                SetResultText(result);
                Debug.Log(result);
            }
            catch (ObjectDisposedException)
            {
                SetResultText("エラー: トークナイザーは既に破棄されています。");
                Debug.LogError("[LinderaSampleUI] Tokenizer has been disposed");
            }
            catch (Exception ex)
            {
                SetResultText($"エラー: トークナイズ中にエラーが発生しました。\n{ex.Message}");
                Debug.LogError($"[LinderaSampleUI] Tokenize error: {ex}");
            }
        }

        /// <summary>
        /// 結果テキストを設定
        /// </summary>
        /// <param name="text">表示するテキスト</param>
        private void SetResultText(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
        }
    }
}
