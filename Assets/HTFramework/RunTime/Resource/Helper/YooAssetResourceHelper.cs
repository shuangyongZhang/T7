using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace HT.Framework
{
    /// <summary>
    ///YooAsset资源管理器助手
    /// </summary>
    internal sealed partial class YooAssetResourceHelper : IYooAssetResourceHelper
    {


        private ResourceManager _module;
        private readonly SemaphoreSlim _assetLoadSemaphore = new SemaphoreSlim(1, 1);
        private ResourcePackage _package;
        private UniTaskCompletionSource<bool> _initializationSource;
        private ResourcePlayMode _activePlayMode;
        /// <summary>
        /// 是否完成初始化完成
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// 默认包名
        /// </summary>
        public string PackageName => _module.PackageName;

        /// <summary>
        /// 模块
        /// </summary>
        public IModuleManager Module { get; set; }

        public event HTFAction<bool> InitializationCompleted;

        /// <summary>
        /// 初始化助手
        /// </summary>
        public void OnInit()
        {
            _module = Module as ResourceManager;
            // 全局目标预算；单个 Awake/OnEnable 的耗时仍需由组件自身控制。
            AsyncInstantiateOperation.SetIntegrationTimeMS(2f);
        }

        /// <summary>
        /// 助手准备工作
        /// </summary>
        public void OnReady()
        {
            if (_module.IsManually) return;
            Initialize(PackageName, _module.PlayMode).Forget();

        }
        /// <summary>
        /// 刷新助手
        /// </summary>
        public void OnUpdate()
        {

        }
        /// <summary>
        /// 终结助手
        /// </summary>
        public void OnTerminate()
        {

        }
        /// <summary>
        /// 暂停助手
        /// </summary>
        public void OnPause()
        {

        }
        /// <summary>
        /// 恢复助手
        /// </summary>
        public void OnResume()
        {

        }

        public UniTask<bool> Initialize(string packageName, ResourcePlayMode playMode)
        {
#if !UNITY_EDITOR
            playMode = ResourcePlayMode.Offline;
#endif
            if (IsInitialized)
            {
                return UniTask.FromResult(true);
            }

            if (_initializationSource != null)
            {
                return _initializationSource.Task;
            }

            var source = new UniTaskCompletionSource<bool>();
            _initializationSource = source;
            InitializeAndNotify(packageName, playMode, source).Forget();
            return source.Task;
        }

        private async UniTaskVoid InitializeAndNotify(string packageName, ResourcePlayMode playMode, UniTaskCompletionSource<bool> source)
        {
            var result = false;
            try
            {
                result = await InitializePackage(packageName, playMode);
            }
            catch (Exception exception)
            {
                Log.Error($"YooAsset初始化异常：{exception}");
            }

            IsInitialized = result;
            if (ReferenceEquals(_initializationSource, source))
            {
                _initializationSource = null;
            }
            source.TrySetResult(result);
            InitializationCompleted?.Invoke(result);
        }

        /// <summary>
        /// 初始化资源包
        /// </summary>
        private async UniTask<bool> InitializePackage(string packageName, ResourcePlayMode playMode)
        {
            // 初始化资源系统
            if (!YooAssets.Initialized)
            {
                YooAssets.Initialize();
            }
            // 创建默认的资源包
            _package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
            YooAssets.SetDefaultPackage(_package);
            _activePlayMode = playMode;

            // 初始化
            InitializeParameters initializeParameters = null;
            if (playMode == ResourcePlayMode.EditorSimulate)
            {
#if UNITY_EDITOR
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                initializeParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                };
#else
                Log.Error("EditorSimulate模式仅可在Unity Editor中使用。");
                return false;
#endif
            }
            else if (playMode == ResourcePlayMode.Offline)
            {
                initializeParameters = new OfflinePlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters()
                };
            }
            else
            {
                Log.Error($"不支持的YooAsset运行模式：{playMode}");
                return false;
            }

            if (_package.InitializeStatus != EOperationStatus.Succeed)
            {
                var initializeOp = _package.InitializeAsync(initializeParameters);
                await initializeOp;
                if (initializeOp.Status != EOperationStatus.Succeed)
                {
                    Log.Error(initializeOp.Error);
                    return false;
                }
            }



            // 请求资源版本
            var requetVersionOp = _package.RequestPackageVersionAsync();
            await requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
            {
                Log.Error(requetVersionOp.Error);
                return false;
            }


            // 更新资源清单
            var updateManifestOp = _package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            await updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
            {
                Log.Error(updateManifestOp.Error);
                return false;
            }


            return true;
        }


        #region 加载资源
        /// <summary>
        /// 加载资源（异步）
        /// </summary>
        /// <param name="location"></param>
        /// <param name="isPrefab">是否是加载预制体</param>
        /// <param name="parent">预制体加载完成后的父级</param>
        /// <param name="isUI">是否是加载UI</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(string location, bool isPrefab, Transform parent, bool isUI) where T : Object
        {
            EnsureInitialized();
            var beginTime = Time.realtimeSinceStartup;
            await _assetLoadSemaphore.WaitAsync();
            var waitTime = Time.realtimeSinceStartup;
            float instantiateTime = 0f;
            LoadHandle loadHandle = null;
            try
            {
                //从定位中获取到LoadHandle,如果没有则创建一个新的
                loadHandle = TryGetLoadHandleByLocation(location, true);
                loadHandle.SetWaitAsync(true);

                Object result;
                if (loadHandle.Object != null)
                {
                    result = loadHandle.Object;
                }
                else
                {
                    var assetOperationHandle = _package.LoadAssetAsync<T>(location);
                    try
                    {
                        await assetOperationHandle.ToUniTask();
                    }
                    catch
                    {
                        if (assetOperationHandle.IsValid)
                        {
                            assetOperationHandle.Release();
                        }
                        throw;
                    }

                    result = assetOperationHandle.AssetObject;
                    if (result != null && AddLoadHandle(result, loadHandle))
                    {
                        loadHandle.ResetHandle(result, assetOperationHandle);
                    }
                    else
                    {
                        if (assetOperationHandle.IsValid)
                        {
                            assetOperationHandle.Release();
                        }
                        if (result != null)
                        {
                            Log.Error($"资源对象已关联到其他定位，无法建立句柄：{location}");
                            result = null;
                        }
                    }
                }

                if (result != null)
                {
                    loadHandle.AddRefCount();
                    if (isPrefab)
                    {
                        var prefabTem = result.Cast<GameObject>();
                        GameObject clone = null;
                        try
                        {
                            float instantiateStartTime = Time.realtimeSinceStartup;
                            clone = await AsyncPrefabInstantiation.CreateAsync(prefabTem, parent, isUI);
                            instantiateTime = Time.realtimeSinceStartup - instantiateStartTime;
                            AddClone(clone, prefabTem, loadHandle);
                            result = clone;
                        }
                        catch
                        {
                            if (clone != null) Main.Kill(clone);
                            if (loadHandle.RemoveRefCount() && loadHandle.RefCount == 0)
                            {
                                loadHandle.SetWaitAsync(false);
                                ReleaseHandle(loadHandle);
                                loadHandle = null;
                            }
                            throw;
                        }
                    }
                }

                LogLoadedInfo(result != null, location, beginTime, waitTime, instantiateTime);
                return result.Cast<T>();
            }
            finally
            {
                if (loadHandle != null)
                {
                    loadHandle.SetWaitAsync(false);
                    DiscardEmptyLoadHandle(loadHandle);
                }
                _assetLoadSemaphore.Release();
            }
        }
        #endregion

        #region 原生文件
        /// <summary>
        /// 获取原生文件文本
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async UniTask<string> LoadRawFileTextAsync(string location)
        {
            var rawFileOperationHandle = await LoadRawFileAsync(location);
            try
            {
                return rawFileOperationHandle.GetRawFileText();
            }
            finally
            {
                if (rawFileOperationHandle.IsValid)
                {
                    rawFileOperationHandle.Release();
                }
            }
        }

        /// <summary>
        /// 获取原生文件二进制数据
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async UniTask<byte[]> LoadRawFileDataAsync(string location)
        {
            var rawFileOperationHandle = await LoadRawFileAsync(location);
            try
            {
                return rawFileOperationHandle.GetRawFileData();
            }
            finally
            {
                if (rawFileOperationHandle.IsValid)
                {
                    rawFileOperationHandle.Release();
                }
            }
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        private async UniTask<RawFileHandle> LoadRawFileAsync(string location)
        {
            EnsureInitialized();
            var beginTime = Time.realtimeSinceStartup;
            var waitTime = Time.realtimeSinceStartup;
            RawFileHandle handle = null;
            try
            {
                handle = _package.LoadRawFileAsync(location);
                await handle.ToUniTask();
                LogLoadedInfo(handle.IsValid, location, beginTime, waitTime);
                return handle;
            }
            catch
            {
                if (handle != null && handle.IsValid)
                {
                    handle.Release();
                }
                throw;
            }
        }
        #endregion

        #region 场景加载
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="onLoading">加载进度</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <returns></returns>
        public async UniTask<Scene> LoadSceneAsync(string location, HTFAction<float> onLoading = null, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool suspendLoad = false)
        {
            EnsureInitialized();
            var beginTime = Time.realtimeSinceStartup;
            var waitTime = Time.realtimeSinceStartup;
            SceneHandle handle = null;
            try
            {
                handle = _package.LoadSceneAsync(location, sceneMode, physicsMode, suspendLoad);
                var progress = Progress.Create<float>(p => onLoading?.Invoke(p));
                await handle.ToUniTask(progress);
                LogLoadedInfo(handle.IsValid, location, beginTime, waitTime);

                var scene = handle.SceneObject;
                AddSceneOperationHandle(scene.name, handle);
                _sceneNamesByLocation[location] = scene.name;
                return scene;
            }
            catch
            {
                if (handle != null && handle.IsValid)
                {
                    handle.Release();
                }
                throw;
            }
        }

        /// <summary>
        /// 获取资源信息的路径列表
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="isAddress">是否为可寻址路径</param>
        /// <returns></returns>
        public string[] GetAssetPath(string tag, bool isAddress)
        {
            EnsureInitialized();
            var assetInfos = _package.GetAssetInfos(tag);
            var pathAry = new string[assetInfos.Length];
            if (isAddress)
            {
                for (var i = 0; i < assetInfos.Length; i++)
                {
                    pathAry[i] = assetInfos[i].Address;
                }
            }
            else
            {
                for (var i = 0; i < assetInfos.Length; i++)
                {
                    pathAry[i] = assetInfos[i].AssetPath;
                }
            }
            return pathAry;
        }

        #endregion

        #region 释放
        /// <summary>
        ///  卸载资源
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isClone"></param>
        public void UnLoadAsset(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            //是否为克隆体
            var isClone = false;
            //作为clone的实例化编号
            var cloneInstanceID = 0;
            //如果为gameObject 判断是否为克隆体
            if (obj.GetType() == typeof(GameObject))
            {
                //判断是否关联源obj
                var sourceObj = TryGetObjectByClone(obj, true);

                if (sourceObj != null)
                {
                    cloneInstanceID = obj.GetInstanceID();
                    obj = sourceObj;
                    isClone = true;
                }
            }

            //获得obj handle
            if (!TryGetLoadHandleByObj(obj, out var handle))
            {
                return;
            }

            //如果为克隆先移除其InstanceID
            if (isClone)
            {
                handle.RemoveInstanceID(cloneInstanceID);
            }

            //减少引用
            if (!handle.RemoveRefCount())
            {
                "重复释放资源：引用计数已经为零。".Warning();
                return;
            }

            //如果引用为0 则释放
            if (handle.RefCount == 0)
            {
                ReleaseHandle(handle);
            }
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public async UniTask UnLoadScene(string sceneName)
        {
            if (_sceneNamesByLocation.TryGetValue(sceneName, out var resolvedName))
                sceneName = resolvedName;
            if (!TryGetSceneOperationHandle(sceneName, out var sceneOperationHandle))
            {
                return;
            }

            RemoveSceneOperationHandle(sceneName);
            try
            {
                await sceneOperationHandle.UnloadAsync().ToUniTask();
                if (!TryGetSceneOperationHandle(sceneName, out _))
                {
                    foreach (var location in new System.Collections.Generic.List<string>(_sceneNamesByLocation.Keys))
                    {
                        if (_sceneNamesByLocation[location] == sceneName) _sceneNamesByLocation.Remove(location);
                    }
                }
            }
            catch
            {
                AddSceneOperationHandle(sceneName, sceneOperationHandle);
                throw;
            }
        }

        /// <summary>
        ///  清理内存，释放空闲内存（异步）
        /// </summary>
        public async UniTask ClearMemory()
        {
            EnsureInitialized();
            await _assetLoadSemaphore.WaitAsync();
            try
            {
                ReleaseUnusedHandles();
                await _package.UnloadUnusedAssetsAsync();
                await Resources.UnloadUnusedAssets();
            }
            finally
            {
                _assetLoadSemaphore.Release();
            }
        }
        #endregion

        #region 工具
        private void EnsureInitialized()
        {
            if (!IsInitialized || _package == null)
            {
                throw new InvalidOperationException("YooAsset资源包尚未初始化成功。");
            }
        }

        /// <summary>
        /// 打印加载结束时的信息
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="location"></param>
        /// <param name="beginTime"></param>
        /// <param name="waitTime"></param>
        private void LogLoadedInfo(bool isValid, string location, float beginTime, float waitTime, float instantiateTime = 0f)
        {
            var endTime = Time.realtimeSinceStartup;
            // 实例化耗时包含跨帧等待，不等于主线程连续阻塞时长。
            string.Format("异步加载资源{0}[{1}模式]：{2}\n排队：{3:F1} ms  资源读取：{4:F1} ms  实例化等待：{5:F1} ms"
                , isValid ? "成功" : "失败"
                , _activePlayMode == ResourcePlayMode.EditorSimulate ? "编辑器模拟" : "本地离线"
                , location
                , (waitTime - beginTime) * 1000f
                , Mathf.Max(0f, endTime - waitTime - instantiateTime) * 1000f
                , instantiateTime * 1000f).Info();
        }
        #endregion
    }
}
