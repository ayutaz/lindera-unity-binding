#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LinderaUnityBinding.Editor
{
    /// <summary>
    /// Lindera Unity Binding パッケージのエクスポートユーティリティ
    /// </summary>
    public static class LinderaPackageExporter
    {
        private const string PackageName = "LinderaUnityBinding";
        private const string PackageRoot = "Assets/Lindera";

        /// <summary>
        /// Unity Packageをエクスポート（デスクトップに保存）
        /// </summary>
        [MenuItem("Lindera Unity Binding/Export Unity Package")]
        public static void ExportPackage()
        {
            ExportPackageToPath(null);
        }

        /// <summary>
        /// Unity Packageを指定パスにエクスポート
        /// </summary>
        /// <param name="outputPath">出力先パス（nullの場合はダイアログ表示）</param>
        /// <returns>エクスポートされたファイルパス（キャンセル時はnull）</returns>
        public static string ExportPackageToPath(string outputPath)
        {
            // パッケージに含めるアセットを収集
            var assetPaths = GetPackageAssets();

            if (assetPaths.Length == 0)
            {
                Debug.LogError("[LinderaPackageExporter] No assets found to export.");
                return null;
            }

            // 出力パスが指定されていない場合はダイアログを表示
            if (string.IsNullOrEmpty(outputPath))
            {
                var version = GetPackageVersion();
                var defaultFileName = $"{PackageName}-{version}.unitypackage";
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                outputPath = EditorUtility.SaveFilePanel(
                    "Export Unity Package",
                    desktopPath,
                    defaultFileName,
                    "unitypackage"
                );

                if (string.IsNullOrEmpty(outputPath))
                {
                    Debug.Log("[LinderaPackageExporter] Export cancelled.");
                    return null;
                }
            }

            try
            {
                // パッケージをエクスポート
                AssetDatabase.ExportPackage(
                    assetPaths,
                    outputPath,
                    ExportPackageOptions.Recurse
                );

                Debug.Log($"[LinderaPackageExporter] Package exported successfully: {outputPath}");
                Debug.Log($"[LinderaPackageExporter] Included {assetPaths.Length} assets.");

                return outputPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LinderaPackageExporter] Export failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// パッケージに含めるアセットパスを取得
        /// </summary>
        private static string[] GetPackageAssets()
        {
            // Assets/Lindera 以下のすべてのアセットを含める
            var guids = AssetDatabase.FindAssets("", new[] { PackageRoot });
            var paths = new string[guids.Length + 1];

            // ルートフォルダも含める
            paths[0] = PackageRoot;

            for (int i = 0; i < guids.Length; i++)
            {
                paths[i + 1] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }

            return paths;
        }

        /// <summary>
        /// package.jsonからバージョンを取得
        /// </summary>
        private static string GetPackageVersion()
        {
            var packageJsonPath = Path.Combine(PackageRoot, "package.json");

            if (!File.Exists(packageJsonPath))
            {
                return "0.0.0";
            }

            try
            {
                var json = File.ReadAllText(packageJsonPath);
                // 簡易的なJSONパース（"version": "x.x.x" を抽出）
                var versionKey = "\"version\"";
                var versionIndex = json.IndexOf(versionKey, StringComparison.Ordinal);

                if (versionIndex < 0) return "0.0.0";

                var colonIndex = json.IndexOf(':', versionIndex);
                var quoteStart = json.IndexOf('"', colonIndex + 1);
                var quoteEnd = json.IndexOf('"', quoteStart + 1);

                if (quoteStart < 0 || quoteEnd < 0) return "0.0.0";

                return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            }
            catch
            {
                return "0.0.0";
            }
        }

        /// <summary>
        /// コマンドラインからのエクスポート用エントリーポイント
        /// </summary>
        /// <remarks>
        /// 使用方法:
        /// Unity -batchmode -quit -projectPath [project] -executeMethod LinderaUnityBinding.Editor.LinderaPackageExporter.ExportFromCommandLine -outputPath [path]
        /// </remarks>
        public static void ExportFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            string outputPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-outputPath" && i + 1 < args.Length)
                {
                    outputPath = args[i + 1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                var version = GetPackageVersion();
                outputPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    $"{PackageName}-{version}.unitypackage"
                );
            }

            var result = ExportPackageToPath(outputPath);

            if (result == null)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }
    }
}
#endif
