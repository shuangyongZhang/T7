using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;

namespace HT.Framework
{

    /// <summary>
    /// Yooasset关联
    /// </summary>
    internal sealed partial class YooAssetResourceHelper
    {
        //location LoadHandle
        private readonly Dictionary<string, LoadHandle> s_LoadHandleMap = new Dictionary<string, LoadHandle>();
        //obj LoadHandle
        private readonly Dictionary<Object, LoadHandle> o_LoadHandleMap = new Dictionary<Object, LoadHandle>();
        //gameObjectClone  gameObject
        private readonly Dictionary<int, Object> instanceID_ObjectMap = new Dictionary<int, Object>();
        //场景名称  场景加载句柄
        private readonly Dictionary<string, List<SceneHandle>> s_SceneOperationHandleMap = new Dictionary<string, List<SceneHandle>>();
        private readonly Dictionary<string, string> _sceneNamesByLocation = new Dictionary<string, string>();

        #region location 2 LoadHandle
        /// <summary>
        /// 从定位中获取到LoadHandle,如果没有则创建一个新的
        /// </summary>
        /// <param name="location">定位</param>
        /// <param name="isSpawn">如果不存在是否生成</param>
        /// <returns></returns>
        private LoadHandle TryGetLoadHandleByLocation(string location, bool isSpawn)
        {
            if (!s_LoadHandleMap.TryGetValue(location, out var handle) && isSpawn)
            {
                handle = Main.m_ReferencePool.Spawn<LoadHandle>();
                handle.SetLocation(location);
                s_LoadHandleMap.Add(location, handle);
            }
            return handle;
        }
        #endregion

        #region obj 2 LoadHandle
        /// <summary>
        /// 将obj 与 loadHandle 关联
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="loadHandle"></param>
        /// <returns></returns>
        private bool AddLoadHandle(Object obj, LoadHandle loadHandle)
        {
            // return o_LoadHandleMap.TryAdd(obj, loadHandle);
            if (!o_LoadHandleMap.ContainsKey(obj))
            {
                o_LoadHandleMap.Add(obj, loadHandle);
                return true;
            }
            return false;

        }

        /// <summary>
        /// 获得LoadHandle
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private bool TryGetLoadHandleByObj(Object obj, out LoadHandle handle)
        {
            return o_LoadHandleMap.TryGetValue(obj, out handle);
        }

        /// <summary>
        /// Discards a newly created load handle when its load failed before producing an object.
        /// </summary>
        private void DiscardEmptyLoadHandle(LoadHandle handle)
        {
            if (handle == null || handle.Object != null || handle.RefCount != 0 || handle.WaitAsync)
            {
                return;
            }

            if (s_LoadHandleMap.TryGetValue(handle.Location, out var current) && ReferenceEquals(current, handle))
            {
                s_LoadHandleMap.Remove(handle.Location);
                Main.m_ReferencePool.Despawn(handle);
            }
        }
        #endregion


        #region Clone 2 obj
        /// <summary>
        /// 将clone 与 prefab关联
        /// </summary>
        /// <param name="clone"></param>
        /// <param name="prefab"></param>
        private void AddClone(Object clone, Object prefab, LoadHandle handle)
        {
            var instanceID = clone.GetInstanceID();
            instanceID_ObjectMap.Add(clone.GetInstanceID(), prefab);
            handle.AddInstanceID(instanceID);
        }
        /// <summary>
        /// 获取object
        /// </summary>
        /// <param name="clone"></param>
        /// <returns></returns>
        private Object TryGetObjectByClone(Object clone, bool isRemove)
        {
            var instanceID = clone.GetInstanceID();

            if (!instanceID_ObjectMap.ContainsKey(instanceID))
            {
                return null;
            }
            var obj = instanceID_ObjectMap[instanceID];
            if (isRemove)
            {
                instanceID_ObjectMap.Remove(instanceID);
            }
            return obj;
        }
        #endregion


        #region 场景
        /// <summary>
        /// 将场景名称 和 SceneOperationHandle 关联
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="handle"></param>
        private void AddSceneOperationHandle(string sceneName, SceneHandle handle)
        {
            if (!s_SceneOperationHandleMap.TryGetValue(sceneName, out var handles))
            {
                handles = new List<SceneHandle>();
                s_SceneOperationHandleMap.Add(sceneName, handles);
            }
            handles.Add(handle);
        }

        /// <summary>
        /// 获取SceneOperationHandle
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private bool TryGetSceneOperationHandle(string sceneName, out SceneHandle handle)
        {
            if (s_SceneOperationHandleMap.TryGetValue(sceneName, out var handles) && handles.Count > 0)
            {
                handle = handles[handles.Count - 1];
                return true;
            }

            handle = null;
            return false;
        }

        /// <summary>
        /// 移除SceneOperationHandle
        /// </summary>
        /// <param name="sceneName"></param>
        private void RemoveSceneOperationHandle(string sceneName)
        {
            if (s_SceneOperationHandleMap.TryGetValue(sceneName, out var handles) && handles.Count > 0)
            {
                handles.RemoveAt(handles.Count - 1);
                if (handles.Count == 0)
                {
                    s_SceneOperationHandleMap.Remove(sceneName);
                }
            }
        }
        #endregion

        #region 释放
        /// <summary>
        /// 释放Handle
        /// </summary>
        /// <param name="handle"></param>
        private void ReleaseHandle(LoadHandle handle)
        {
            if (handle.WaitAsync)
            {
                $"释放加载句柄失败 {handle.Location} 正在加载中".Warning();
                return;
            }

            s_LoadHandleMap.Remove(handle.Location);
            o_LoadHandleMap.Remove(handle.Object);
            foreach (var instanceID in handle.InstanceIDList)
            {
                instanceID_ObjectMap.Remove(instanceID);
            }

            if (handle.AssetHandle != null && handle.AssetHandle.IsValid)
            {
                handle.AssetHandle.Release();
            }

            Main.m_ReferencePool.Despawn(handle);
        }

        /// <summary>
        /// 释放没有活动引用的资源句柄。
        /// </summary>
        private void ReleaseUnusedHandles()
        {
            var loadHandles = s_LoadHandleMap.Values
                .Where(handle => handle.RefCount == 0 && !handle.WaitAsync)
                .ToArray();
            foreach (var handle in loadHandles)
            {
                ReleaseHandle(handle);
            }
        }
        #endregion
    }
}
