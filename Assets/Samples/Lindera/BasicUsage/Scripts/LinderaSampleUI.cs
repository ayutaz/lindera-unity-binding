using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lindera;

namespace Lindera.Samples
{
    /// <summary>
    /// Linderaの基本的な使い方を示すサンプル（UGUI + TextMeshPro版）
    /// </summary>
    public class LinderaSampleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button tokenizeButton;

        [Header("Settings")]
        [SerializeField] private string defaultText = "東京都に住んでいます。今日はいい天気ですね。";

        private LinderaTokenizer _tokenizer;

        private void Awake()
        {
            _tokenizer = new LinderaTokenizer();
        }

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

            Tokenize(defaultText);
        }

        private void OnDestroy()
        {
            _tokenizer?.Dispose();

            if (tokenizeButton != null)
            {
                tokenizeButton.onClick.RemoveListener(OnTokenizeButtonClicked);
            }
        }

        private void OnTokenizeButtonClicked()
        {
            if (inputField != null)
            {
                Tokenize(inputField.text);
            }
        }

        private void Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                SetResultText("テキストを入力してください");
                return;
            }

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

        private void SetResultText(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
        }
    }
}
