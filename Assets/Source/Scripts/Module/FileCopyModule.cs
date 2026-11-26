using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HT.Framework;

namespace LB
{
    [CustomModule("FileCopyModule", true)]
    public class FileCopyModule : CustomModuleBase
    {
        private static FileCopyModule _instance;
        public static FileCopyModule Instance
        {
            get
            {
                _instance ??= Main.m_CustomModule["FileCopyModule"].Cast<FileCopyModule>();
                return _instance;
            }
        }

        private string _streamingAssetsPath;
        private string _persistentDataPath;
        private string[] _excludedFolders = { "Fonts", "yoo" };
        private string _markerFile;
        private const string VERSION_KEY = "FileCopyModule_Version";

        public override void OnInit()
        {
            base.OnInit();
            _streamingAssetsPath = Application.streamingAssetsPath;
            _persistentDataPath = Global.ConfigPath;
            _markerFile = Path.Combine(_persistentDataPath, ".copied");
        }

        public async UniTask CopyFilesOnStartup()
        {
            string currentVersion = Application.version;
            string savedVersion = PlayerPrefs.GetString(VERSION_KEY, "");

            if (savedVersion != currentVersion)
            {
                ClearConfigFiles();
                PlayerPrefs.SetString(VERSION_KEY, currentVersion);
                PlayerPrefs.Save();
            }

            if (File.Exists(_markerFile)) return;

            await CopyFilesRecursively(_streamingAssetsPath, _persistentDataPath);
            File.WriteAllText(_markerFile, "1");
        }

        public void ClearConfigFiles()
        {
            try
            {
                if (Directory.Exists(_persistentDataPath))
                {
                    Directory.Delete(_persistentDataPath, true);
                }
            }
            catch { }
        }

        private async UniTask CopyFilesRecursively(string sourceDir, string targetDir)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                await CopyFilesAndroid(sourceDir, targetDir);
            }
            else
            {
                await CopyFilesNormal(sourceDir, targetDir);
            }
        }

        private async UniTask CopyFilesNormal(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir)) return;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                if (fileName.EndsWith(".meta") || fileName.Equals("help.pdf", System.StringComparison.OrdinalIgnoreCase)) continue;

                string targetFilePath = Path.Combine(targetDir, fileName);

                if (!File.Exists(targetFilePath))
                {
                    byte[] data = await ReadFileBytesAsync(file);
                    if (data != null)
                    {
                        await WriteFileBytesAsync(targetFilePath, data);
                    }
                }
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                if (IsExcluded(dirName)) continue;

                string targetSubDir = Path.Combine(targetDir, dirName);
                await CopyFilesNormal(subDir, targetSubDir);
            }
        }

        private async UniTask CopyFilesAndroid(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            string[] rootFiles = { "evaluationSetting.json", "game.txt" };
            string[] rootFolders = { "BeachVolleyball", "BirdHitting", "BraveBird", "CanyonRacing",
                "CardShapeMatch", "ColorMatching", "FindTreasure", "HTML", "MazeGame",
                "PufferfishCatchBall", "SeaTreasureHunting", "SharkEatFish",
                "SpaceFighter", "VisualReaction" };

            foreach (string fileName in rootFiles)
            {
                string sourcePath = Path.Combine(sourceDir, fileName);
                string targetPath = Path.Combine(targetDir, fileName);

                if (!File.Exists(targetPath))
                {
                    byte[] data = await ReadFileBytesAsync(sourcePath);
                    if (data != null)
                    {
                        await WriteFileBytesAsync(targetPath, data);
                    }
                }
            }

            foreach (string folderName in rootFolders)
            {
                if (IsExcluded(folderName)) continue;

                string sourceFolder = Path.Combine(sourceDir, folderName);
                string targetFolder = Path.Combine(targetDir, folderName);
                await CopyGameFolder(sourceFolder, targetFolder);
            }
        }

        private async UniTask CopyGameFolder(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string gameConfigFolder = Path.Combine(sourceFolder, "GameConfig");
            string targetConfigFolder = Path.Combine(targetFolder, "GameConfig");

            if (!Directory.Exists(targetConfigFolder))
            {
                Directory.CreateDirectory(targetConfigFolder);
            }

            string[] configFiles = { "config.txt", "game.txt" };
            foreach (string fileName in configFiles)
            {
                string sourcePath = Path.Combine(gameConfigFolder, fileName);
                string targetPath = Path.Combine(targetConfigFolder, fileName);

                if (!File.Exists(targetPath))
                {
                    byte[] data = await ReadFileBytesAsync(sourcePath);
                    if (data != null)
                    {
                        await WriteFileBytesAsync(targetPath, data);
                    }
                }
            }
        }

        private bool IsExcluded(string folderName)
        {
            foreach (string excluded in _excludedFolders)
            {
                if (folderName.Equals(excluded, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public async UniTask<byte[]> ReadFileBytesAsync(string path)
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android && path.StartsWith(Application.streamingAssetsPath))
                {
                    using (var www = new UnityEngine.Networking.UnityWebRequest(path))
                    {
                        www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                        await www.SendWebRequest();
                        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            return www.downloadHandler.data;
                        }
                        return null;
                    }
                }
                else
                {
                    return System.IO.File.ReadAllBytes(path);
                }
            }
            catch
            {
                return null;
            }
        }

        private UniTask WriteFileBytesAsync(string path, byte[] data)
        {
            try
            {
                System.IO.File.WriteAllBytes(path, data);
            }
            catch { }
            return UniTask.CompletedTask;
        }

        public string GetReadPath(string relativePath)
        {
            if (Main.m_Debug.IsEnableDebugger)
            {
                string configPath = Path.Combine(_persistentDataPath, relativePath);
                if (File.Exists(configPath))
                {
                    return configPath;
                }
            }
            return Path.Combine(_streamingAssetsPath, relativePath);
        }

        public async UniTask<byte[]> ReadFileAsync(string relativePath)
        {
            string path = GetReadPath(relativePath);
            return await ReadFileBytesAsync(path);
        }

        public override void OnReady()
        {
            base.OnReady();
            IsRunning = false;
        }
    }
}