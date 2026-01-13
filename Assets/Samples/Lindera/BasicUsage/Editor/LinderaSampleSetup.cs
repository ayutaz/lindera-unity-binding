#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Lindera.Samples.Editor
{
    public static class LinderaSampleSetup
    {
        [MenuItem("Lindera/Setup Sample Scene")]
        public static void SetupSampleScene()
        {
            // 現在のシーンでLinderaSampleオブジェクトを探す
            var sampleGo = GameObject.Find("LinderaSample");

            if (sampleGo == null)
            {
                sampleGo = new GameObject("LinderaSample");
            }

            // LinderaSampleUIコンポーネントを追加
            if (sampleGo.GetComponent<LinderaSampleUI>() == null)
            {
                sampleGo.AddComponent<LinderaSampleUI>();
                EditorUtility.SetDirty(sampleGo);

                // シーンを保存
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                Debug.Log("LinderaSampleUI component added successfully!");
            }
            else
            {
                Debug.Log("LinderaSampleUI component already exists.");
            }
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
