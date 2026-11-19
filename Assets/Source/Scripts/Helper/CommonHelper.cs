using System;
using System.IO;
using UnityEngine;
namespace LB
{


    public class CommonHelper
    {
        public static byte GetAgeByBirthdate(DateTime birthdate)
        {
            DateTime now = DateTime.Now;
            byte age = (byte)(now.Year - birthdate.Year);
            if (now.Month < birthdate.Month || (now.Month == birthdate.Month && now.Day < birthdate.Day))
            {
                age--;
            }
            return age;
        }
        /// 将 Stream 转成 byte[]
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// 将 byte[] 转成 Stream

        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        public static byte[] ReadTexture(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[fileStream.Length]; //创建文件长度的buffer   
            fileStream.Read(buffer, 0, (int)fileStream.Length);
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            return buffer;
        }
        public static Vector2 GetImageSize(string filePath)
        {
            var fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(fileData);
            var size = new Vector2(texture.width, texture.height);
            UnityEngine.Object.Destroy(texture);
            return size;
        }


    }
}