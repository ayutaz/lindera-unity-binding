#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace Lindera.Samples.Editor
{
    public static class LinderaSampleSetup
    {
        private const string FontPath = "Assets/Samples/Lindera/BasicUsage/Fonts/NotoSansJP-Regular.ttf";
        private const string FontAssetPath = "Assets/Samples/Lindera/BasicUsage/Fonts/NotoSansJP-Regular SDF.asset";

        [MenuItem("Lindera/Setup Sample Scene")]
        public static void SetupSampleScene()
        {
            // フォントアセットを取得または作成
            var fontAsset = GetOrCreateFontAsset();

            // Canvas作成
            var canvas = CreateCanvas();

            // UI要素を作成
            var inputField = CreateInputField(canvas.transform, fontAsset);
            var button = CreateButton(canvas.transform, fontAsset);
            var scrollView = CreateScrollView(canvas.transform, fontAsset);
            var resultText = scrollView.GetComponentInChildren<TMP_Text>();

            // LinderaSampleオブジェクトを作成またはアップデート
            var sampleGo = GameObject.Find("LinderaSample");
            if (sampleGo == null)
            {
                sampleGo = new GameObject("LinderaSample");
            }

            // LinderaSampleUIコンポーネントを設定
            var sampleUI = sampleGo.GetComponent<LinderaSampleUI>();
            if (sampleUI == null)
            {
                sampleUI = sampleGo.AddComponent<LinderaSampleUI>();
            }

            // SerializedObjectを使用してUI参照を設定
            var serializedObject = new SerializedObject(sampleUI);
            serializedObject.FindProperty("inputField").objectReferenceValue = inputField;
            serializedObject.FindProperty("resultText").objectReferenceValue = resultText;
            serializedObject.FindProperty("tokenizeButton").objectReferenceValue = button;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(sampleGo);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("Lindera Sample UI setup complete!");
        }

        private static TMP_FontAsset GetOrCreateFontAsset()
        {
            // 既存のフォントアセットを確認
            var existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existingAsset != null)
            {
                return existingAsset;
            }

            // フォントを読み込み
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogWarning($"Font not found at {FontPath}. Using default TMP font.");
                return TMP_Settings.defaultFontAsset;
            }

            // TMP Font Asset Creatorを使用してフォントアセットを作成
            // 動的フォントアセットを作成（ランタイムで文字を生成）
            var fontAsset = TMP_FontAsset.CreateFontAsset(font);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

            // アセットとして保存
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

            // Atlas Textureも保存
            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = "NotoSansJP-Regular Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }

            // Materialも保存
            if (fontAsset.material != null)
            {
                fontAsset.material.name = "NotoSansJP-Regular Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created TMP Font Asset at {FontAssetPath}");
            return fontAsset;
        }

        private static Canvas CreateCanvas()
        {
            var existingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas.gameObject);
            }

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem（既存のものがあれば削除して新規作成）
            var existingEventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existingEventSystem != null)
            {
                Object.DestroyImmediate(existingEventSystem.gameObject);
            }
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Input System Package対応
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            return canvas;
        }

        private static TMP_InputField CreateInputField(Transform parent, TMP_FontAsset fontAsset)
        {
            // Panel背景
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(parent, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = new Vector2(40, 40);
            panelRect.offsetMax = new Vector2(-40, -40);
            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // タイトル
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(0, 50);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "Lindera Sample - Japanese Tokenizer";
            titleText.fontSize = 32;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            if (fontAsset != null) titleText.font = fontAsset;

            // 入力ラベル
            var labelGo = new GameObject("InputLabel");
            labelGo.transform.SetParent(panelGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(0.5f, 1);
            labelRect.anchoredPosition = new Vector2(0, -80);
            labelRect.sizeDelta = new Vector2(-40, 30);
            var labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.text = "日本語テキストを入力:";
            labelText.fontSize = 20;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;
            if (fontAsset != null) labelText.font = fontAsset;

            // 入力フィールド
            var inputGo = new GameObject("InputField");
            inputGo.transform.SetParent(panelGo.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 1);
            inputRect.anchorMax = new Vector2(1, 1);
            inputRect.pivot = new Vector2(0.5f, 1);
            inputRect.anchoredPosition = new Vector2(0, -120);
            inputRect.sizeDelta = new Vector2(-40, 80);
            var inputImage = inputGo.AddComponent<Image>();
            inputImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // TextArea
            var textAreaGo = new GameObject("Text Area");
            textAreaGo.transform.SetParent(inputGo.transform, false);
            var textAreaRect = textAreaGo.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 10);
            textAreaRect.offsetMax = new Vector2(-10, -10);
            textAreaGo.AddComponent<RectMask2D>();

            // Placeholder
            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(textAreaGo.transform, false);
            var placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            var placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholder.text = "テキストを入力してください...";
            placeholder.fontSize = 18;
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholder.alignment = TextAlignmentOptions.TopLeft;
            if (fontAsset != null) placeholder.font = fontAsset;

            // Text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(textAreaGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            if (fontAsset != null) text.font = fontAsset;

            // TMP_InputField設定
            var inputField = inputGo.AddComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            if (fontAsset != null) inputField.fontAsset = fontAsset;

            return inputField;
        }

        private static Button CreateButton(Transform parent, TMP_FontAsset fontAsset)
        {
            var panel = parent.Find("Panel");

            var buttonGo = new GameObject("TokenizeButton");
            buttonGo.transform.SetParent(panel, false);
            var buttonRect = buttonGo.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 1);
            buttonRect.anchorMax = new Vector2(0.5f, 1);
            buttonRect.pivot = new Vector2(0.5f, 1);
            buttonRect.anchoredPosition = new Vector2(0, -220);
            buttonRect.sizeDelta = new Vector2(200, 50);

            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f);

            var button = buttonGo.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
            colors.pressedColor = new Color(0.15f, 0.45f, 0.75f, 1f);
            button.colors = colors;

            var buttonTextGo = new GameObject("Text");
            buttonTextGo.transform.SetParent(buttonGo.transform, false);
            var buttonTextRect = buttonTextGo.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            var buttonText = buttonTextGo.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Tokenize";
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            if (fontAsset != null) buttonText.font = fontAsset;

            return button;
        }

        private static ScrollRect CreateScrollView(Transform parent, TMP_FontAsset fontAsset)
        {
            var panel = parent.Find("Panel");

            // 結果ラベル
            var resultLabelGo = new GameObject("ResultLabel");
            resultLabelGo.transform.SetParent(panel, false);
            var resultLabelRect = resultLabelGo.AddComponent<RectTransform>();
            resultLabelRect.anchorMin = new Vector2(0, 1);
            resultLabelRect.anchorMax = new Vector2(1, 1);
            resultLabelRect.pivot = new Vector2(0.5f, 1);
            resultLabelRect.anchoredPosition = new Vector2(0, -290);
            resultLabelRect.sizeDelta = new Vector2(-40, 30);
            var resultLabel = resultLabelGo.AddComponent<TextMeshProUGUI>();
            resultLabel.text = "結果:";
            resultLabel.fontSize = 20;
            resultLabel.alignment = TextAlignmentOptions.Left;
            resultLabel.color = Color.white;
            if (fontAsset != null) resultLabel.font = fontAsset;

            // ScrollView
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(panel, false);
            var scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(20, 20);
            scrollRect.offsetMax = new Vector2(-20, -330);

            var scrollImage = scrollGo.AddComponent<Image>();
            scrollImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            var scrollView = scrollGo.AddComponent<ScrollRect>();
            scrollGo.AddComponent<RectMask2D>();

            // Viewport
            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(10, 10);
            viewportRect.offsetMax = new Vector2(-10, -10);
            viewportGo.AddComponent<RectMask2D>();

            // Content
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Result Text
            var resultTextGo = new GameObject("ResultText");
            resultTextGo.transform.SetParent(contentGo.transform, false);
            var resultTextRect = resultTextGo.AddComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0, 1);
            resultTextRect.anchorMax = new Vector2(1, 1);
            resultTextRect.pivot = new Vector2(0.5f, 1);
            resultTextRect.anchoredPosition = Vector2.zero;
            resultTextRect.sizeDelta = new Vector2(0, 0);

            var resultText = resultTextGo.AddComponent<TextMeshProUGUI>();
            resultText.fontSize = 16;
            resultText.color = Color.white;
            resultText.alignment = TextAlignmentOptions.TopLeft;
            resultText.enableWordWrapping = true;
            if (fontAsset != null) resultText.font = fontAsset;

            var textFitter = resultTextGo.AddComponent<ContentSizeFitter>();
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layoutElement = resultTextGo.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;

            scrollView.content = contentRect;
            scrollView.viewport = viewportRect;
            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.scrollSensitivity = 20;

            return scrollView;
        }

        [MenuItem("Lindera/Open Sample Scene")]
        public static void OpenSampleScene()
        {
            var scenePath = "Assets/Samples/Lindera/BasicUsage/LinderaSample.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                Debug.LogError($"Sample scene not found at: {scenePath}");
            }
        }
    }
}
#endif
