using UnityEditor;
using System.IO;
using UnityEngine;

namespace LB.BraveBird.Editor
{
    public class ClearAssetBundleName
    {
        [MenuItem("Tools/Clear AssetBundle Name")]
        public static void ClearSelectedFolderAssetBundleName()
        {
            // 获取选中的文件夹路径
            string[] selectedPaths = Selection.assetGUIDs;
            if (selectedPaths.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选中一个文件夹", "确定");
                return;
            }

            foreach (string guid in selectedPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path))
                {
                    // 递归处理文件夹
                    ProcessDirectory(path);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请确保选中的是文件夹", "确定");
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", "已清空选中文件夹及其子文件夹中所有资源的AssetBundle名称", "确定");
        }

        private static void ProcessDirectory(string directoryPath)
        {
            // 获取文件夹中的所有资源文件
            string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                // 转换为相对路径
                string relativePath = file.Replace(Application.dataPath, "Assets");

                // 检查是否为有效资源文件
                if (AssetDatabase.GetMainAssetTypeAtPath(relativePath) != null)
                {
                    // 清空AssetBundle名称
                    AssetImporter importer = AssetImporter.GetAtPath(relativePath);
                    if (importer != null)
                    {
                        importer.assetBundleName = string.Empty;
                        importer.assetBundleVariant = string.Empty;
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }
}