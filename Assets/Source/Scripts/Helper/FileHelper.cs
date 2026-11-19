using System.IO;
using Cysharp.Threading.Tasks;
using HT.Framework;
using UnityEngine;

namespace LB
{

    public static class FileHelper
    {
        public static bool IsFileExists(string path)
        {
            return File.Exists(path);
        }
        public static void DeleteFile(string fileName)
        {
            if (File.Exists(fileName)) File.Delete(fileName);
        }
        public static void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
        public static string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
        //文件是否在使用中
        public static bool IsFileInUse(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
        public static void ClearDirectory(string directoryPath)
        {
            // 确保目录路径是完整的
            directoryPath = Path.GetFullPath(directoryPath);

            // 检查目录是否存在
            if (Directory.Exists(directoryPath))
            {
                // 获取所有的文件，并删除它们
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    File.Delete(filePath);
                }

                // 获取所有的子目录，并删除它们
                foreach (string dirPath in Directory.GetDirectories(directoryPath))
                {
                    Directory.Delete(dirPath, true); // 设置为true以递归删除子目录
                }
            }
        }
        public static async UniTask<T> ReadSerialConfigData<T>(string path)
        {
            if (Main.m_Debug.IsEnableDebugger)
            {
                if (!IsFileExists(path))
                {
                    throw new System.Exception(path + " 不存在");
                }
                string data = ReadAllText(path);
                var retval = JsonUtility.FromJson<T>(data);
                if (retval == null)
                {
                    throw new System.Exception(path + " 格式错误");
                }
                return retval;
            }
            else
            {
                return await ReadSerialConfigDataAsync<T>(path);
            }
        }
        private static async UniTask<T> ReadSerialConfigDataAsync<T>(string path)
        {
            byte[] data = await FileCopyModule.Instance.ReadFileBytesAsync(path);
            if (data == null || data.Length == 0)
            {
                throw new System.Exception(path + " 不存在");
            }

            // 移除 BOM 字符（如果存在）
            string json = System.Text.Encoding.UTF8.GetString(data);
            if (json.Length > 0 && json[0] == '\uFEFF')
            {
                json = json.Substring(1);
            }

            var retval = JsonUtility.FromJson<T>(json);
            if (retval == null)
            {
                throw new System.Exception(path + " 格式错误");
            }
            return retval;
        }
        public static INIParser InitParser(string path)
        {
            INIParser m_iniParser = new INIParser();
            m_iniParser.Open(path);

            if (string.IsNullOrEmpty(m_iniParser.iniString))
            {
                return null;
            }
            return m_iniParser;
        }
    }
}