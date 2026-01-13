using System.Text;
using UnityEngine;
using Lindera;

namespace Lindera.Samples
{
    /// <summary>
    /// Linderaの基本的な使い方を示すサンプル（OnGUI版）
    /// </summary>
    public class LinderaSampleUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string defaultText = "東京都に住んでいます。今日はいい天気ですね。";

        private LinderaTokenizer _tokenizer;
        private string _inputText;
        private string _resultText;
        private Vector2 _scrollPosition;

        private void Awake()
        {
            _tokenizer = new LinderaTokenizer();
            _inputText = defaultText;
        }

        private void Start()
        {
            Tokenize(_inputText);
        }

        private void OnDestroy()
        {
            _tokenizer?.Dispose();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));

            // タイトル
            GUILayout.Label("Lindera Sample - Japanese Tokenizer", GUI.skin.GetStyle("Box"));
            GUILayout.Space(10);

            // 入力フィールド
            GUILayout.Label("日本語テキストを入力:");
            _inputText = GUILayout.TextArea(_inputText, GUILayout.Height(60));

            GUILayout.Space(10);

            // トークナイズボタン
            if (GUILayout.Button("Tokenize", GUILayout.Height(40)))
            {
                Tokenize(_inputText);
            }

            GUILayout.Space(10);

            // 結果表示
            GUILayout.Label("結果:");
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);
            GUILayout.Label(_resultText);
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _resultText = "テキストを入力してください";
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

            _resultText = sb.ToString();
            Debug.Log(_resultText);
        }
    }
}
