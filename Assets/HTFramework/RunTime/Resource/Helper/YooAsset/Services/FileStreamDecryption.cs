using System.IO;
using UnityEngine;
using YooAsset;

namespace HT.Framework
{
    /// <summary>
    /// 资源文件流加载解密类
    /// </summary>
    public sealed class FileStreamDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = bundleStream;
            decryptResult.Result = AssetBundle.LoadFromStream(bundleStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return decryptResult;
        }


        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult
            {
                ManagedStream = bundleStream,
                CreateRequest = AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize())
            };
            return decryptResult;
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }
        private static uint GetManagedReadBufferSize()
        {
            return 1024;
        }
    }
}
