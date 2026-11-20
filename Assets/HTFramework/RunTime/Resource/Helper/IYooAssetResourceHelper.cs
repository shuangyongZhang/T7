using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HT.Framework
{
    /// <summary>
    /// YooAsset 后端在框架协程接口之外提供的可等待资源契约。
    /// </summary>
    internal interface IYooAssetResourceHelper : IResourceHelper
    {
        bool IsInitialized { get; }
        event HTFAction<bool> InitializationCompleted;
        UniTask<bool> Initialize(string packageName, ResourcePlayMode playMode);
        UniTask<T> LoadAssetAsync<T>(string location, bool isPrefab, Transform parent, bool isUI) where T : Object;
        UniTask<string> LoadRawFileTextAsync(string location);
        UniTask<byte[]> LoadRawFileDataAsync(string location);
        UniTask<Scene> LoadSceneAsync(string location, HTFAction<float> onLoading, LoadSceneMode sceneMode, LocalPhysicsMode physicsMode, bool suspendLoad);
        string[] GetAssetPath(string tag, bool isAddress);
        void UnLoadAsset(Object obj);
        UniTask UnLoadScene(string sceneName);
        new UniTask ClearMemory();
    }
}
