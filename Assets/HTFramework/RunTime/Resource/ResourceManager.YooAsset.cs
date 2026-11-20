using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HT.Framework
{
    /// <summary>YooAsset 的运行模式；Player 始终使用内置离线包。</summary>
    public enum ResourcePlayMode
    {
        /// <summary>使用 StreamingAssets 内置包。</summary>
        Offline = 0,
        /// <summary>在编辑器中模拟资源包。</summary>
        EditorSimulate = 1
    }

    public sealed partial class ResourceManager
    {
        [SerializeField] internal ResourcePlayMode PlayMode = ResourcePlayMode.EditorSimulate;
        [SerializeField] internal string PackageName = "DefaultPackage";
        [SerializeField] internal bool Manually;

        /// <summary>是否由业务显式初始化 YooAsset。</summary>
        public bool IsManually => Manually;
        /// <summary>资源包和清单是否已就绪。</summary>
        public bool IsInitialized => (_helper as IYooAssetResourceHelper)?.IsInitialized ?? false;
        /// <summary>YooAsset 初始化完成通知。</summary>
        public event HTFAction<bool> InitializationCompleted;

        private IYooAssetResourceHelper YooHelper => _helper as IYooAssetResourceHelper
            ?? throw new InvalidOperationException("当前资源助手不是 YooAssetResourceHelper。");

        private void OnYooAssetInitialized(bool success)
        {
            InitializationCompleted?.Invoke(success);
        }

        /// <summary>解除初始化通知，避免模块终结后调用业务订阅者。</summary>
        public override void OnTerminate()
        {
            if (_helper is IYooAssetResourceHelper yoo)
                yoo.InitializationCompleted -= OnYooAssetInitialized;
            InitializationCompleted = null;
            base.OnTerminate();
        }

        /// <summary>使用 Inspector 配置初始化资源。</summary>
        public UniTask<bool> Initialize() => Initialize(PackageName, PlayMode);
        /// <summary>初始化指定的 YooAsset 包。</summary>
        public UniTask<bool> Initialize(string packageName, ResourcePlayMode playMode) => YooHelper.Initialize(packageName, playMode);
        /// <summary>兼容旧的模拟模式标记。</summary>
        [Obsolete("Use ResourcePlayMode instead.")]
        public UniTask<bool> Initialize(string packageName, bool isEditorMode) =>
            Initialize(packageName, isEditorMode ? ResourcePlayMode.EditorSimulate : ResourcePlayMode.Offline);

        /// <summary>加载资源并持有一个引用；使用 UnLoadAsset 归还。</summary>
        public UniTask<T> LoadAsset<T>(string location) where T : Object =>
            YooHelper.LoadAssetAsync<T>(location, false, null, false);
        /// <summary>加载并实例化预制体；销毁前使用 UnLoadAsset 归还引用。</summary>
        public UniTask<GameObject> LoadPrefab(string location, Transform parent, bool isUI = false) =>
            YooHelper.LoadAssetAsync<GameObject>(location, true, parent, isUI);
        /// <summary>供新版框架内部调用的可等待预制体加载。</summary>
        public async UniTask<GameObject> LoadPrefabAsync(PrefabInfo info, Transform parent, bool isUI = false)
        {
            if (_helper is IYooAssetResourceHelper yoo)
                return await yoo.LoadAssetAsync<GameObject>(info.AssetPath, true, parent, isUI);

            GameObject result = null;
            await CoroutineTask.Run(_helper.LoadAssetAsync<GameObject>(info, null, value => result = value, true, parent, isUI), Main.Current);
            return result;
        }
        /// <summary>读取原生文件文本，读取完成即释放底层句柄。</summary>
        public UniTask<string> LoadRawFileText(string location) => YooHelper.LoadRawFileTextAsync(location);
        /// <summary>读取原生文件字节，读取完成即释放底层句柄。</summary>
        public UniTask<byte[]> LoadRawFileData(string location) => YooHelper.LoadRawFileDataAsync(location);
        /// <summary>按标签加载资源。</summary>
        public async UniTask<Dictionary<string, T>> LoadAssetByTag<T>(string tag, bool isAddress = true) where T : Object
        {
            var result = new Dictionary<string, T>();
            try
            {
                foreach (var location in YooHelper.GetAssetPath(tag, isAddress))
                    result.Add(location, await LoadAsset<T>(location));
                return result;
            }
            catch
            {
                foreach (var asset in result.Values) UnLoadAsset(asset);
                throw;
            }
        }
        /// <summary>按标签读取文本，保留旧泛型签名。</summary>
        public async UniTask<Dictionary<string, string>> LoadRawFileTextByTag<T>(string tag, bool isAddress = true)
        {
            var result = new Dictionary<string, string>();
            foreach (var location in YooHelper.GetAssetPath(tag, isAddress))
                result.Add(location, await LoadRawFileText(location));
            return result;
        }
        /// <summary>按标签读取原生文件。</summary>
        public async UniTask<Dictionary<string, byte[]>> LoadRawFileDataByTag(string tag, bool isAddress = true)
        {
            var result = new Dictionary<string, byte[]>();
            foreach (var location in YooHelper.GetAssetPath(tag, isAddress))
                result.Add(location, await LoadRawFileData(location));
            return result;
        }
        /// <summary>加载 YooAsset 场景。</summary>
        public UniTask<Scene> LoadSceneAsync(string location, HTFAction<float> onLoading = null,
            LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool suspendLoad = false) =>
            YooHelper.LoadSceneAsync(location, onLoading, sceneMode, physicsMode, suspendLoad);
        /// <summary>归还资源或预制体实例的引用；不销毁实例。</summary>
        public void UnLoadAsset(Object obj)
        {
            if (_helper is IYooAssetResourceHelper yoo) yoo.UnLoadAsset(obj);
        }
    }
}
