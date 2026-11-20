using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HT.Framework
{
    internal sealed partial class YooAssetResourceHelper
    {
        public ResourceLoadMode LoadMode => ResourceLoadMode.YooAsset;
        public bool IsEditorMode => _activePlayMode == ResourcePlayMode.EditorSimulate;
        public bool IsLogDetail { get; private set; }
        public string AssetBundleRootPath => Application.streamingAssetsPath;
        public string AssetBundleManifestName => PackageName;
        public Dictionary<string, AssetBundle> AssetBundles { get; } = new Dictionary<string, AssetBundle>();
        public AssetBundleManifest Manifest => null;
        public Dictionary<string, Hash128> AssetBundleHashs { get; } = new Dictionary<string, Hash128>();

        public void SetLoader(ResourceLoadMode loadMode, bool isEditorMode, string manifestName, bool isLogDetail)
        {
            if (loadMode != ResourceLoadMode.YooAsset)
                throw new InvalidOperationException("YooAssetResourceHelper requires ResourceLoadMode.YooAsset.");
            IsLogDetail = isLogDetail;
        }

        public IEnumerator LoadAssetAsync<T>(ResourceInfoBase info, HTFAction<float> onLoading,
            HTFAction<T> onLoadDone, bool isPrefab, Transform parent, bool isUI) where T : UnityEngine.Object =>
            LoadAssetAsync(info.AssetPath, typeof(T), onLoading, onLoadDone, isPrefab, parent, isUI);

        public IEnumerator LoadAssetAsync<T>(string location, Type type, HTFAction<float> onLoading,
            HTFAction<T> onLoadDone, bool isPrefab, Transform parent, bool isUI) where T : UnityEngine.Object =>
            LoadAssetAsync<T>(location, isPrefab, parent, isUI).ToCoroutine(value =>
            {
                onLoading?.Invoke(1);
                onLoadDone?.Invoke(value);
            });

        public IEnumerator LoadSceneAsync(SceneInfo info, HTFAction<float> onLoading, HTFAction onLoadDone) =>
            LoadSceneAsync(info.AssetPath, onLoading, onLoadDone);

        public IEnumerator LoadSceneAsync(string location, HTFAction<float> onLoading, HTFAction onLoadDone) =>
            LoadSceneAsync(location, onLoading, LoadSceneMode.Additive, LocalPhysicsMode.None, false)
                .ToCoroutine(scene => onLoadDone?.Invoke());

        public void UnLoadAsset(string location)
        {
            var handle = TryGetLoadHandleByLocation(location, false);
            if (handle == null) return;
            if (handle.InstanceIDList.Count > 0)
                throw new InvalidOperationException("预制体实例必须使用 UnLoadAsset(instance) 归还引用。");
            UnLoadAsset(handle.Object);
        }

        public IEnumerator UnLoadSceneAsync(SceneInfo info) => UnLoadSceneAsync(info.AssetPath);

        public IEnumerator UnLoadSceneAsync(string location) => UnLoadScene(location).ToCoroutine();

        public IEnumerator UnLoadAllSceneAsync() => UnloadAllScenes().ToCoroutine();

        private async UniTask UnloadAllScenes()
        {
            foreach (var name in s_SceneOperationHandleMap.Keys.ToArray())
            {
                while (TryGetSceneOperationHandle(name, out _))
                    await UnLoadScene(name);
            }
        }

        IEnumerator IResourceHelper.ClearMemory() => ClearMemory().ToCoroutine();

        public void SetAssetBundlePath(string path) => throw UnsupportedBundleOperation();
        public AssetBundle GetAssetBundle(string assetBundleName) => throw UnsupportedBundleOperation();
        public IEnumerator UnLoadAssetBundleAsync(string assetBundleName, bool unloadAllLoadedObjects) => throw UnsupportedBundleOperation();
        public IEnumerator UnLoadAllAssetBundleAsync(bool unloadAllLoadedObjects) => throw UnsupportedBundleOperation();

        private static NotSupportedException UnsupportedBundleOperation() =>
            new NotSupportedException("YooAsset 管理 Bundle 生命周期，请使用资源句柄释放和 ClearMemory。");
    }
}
