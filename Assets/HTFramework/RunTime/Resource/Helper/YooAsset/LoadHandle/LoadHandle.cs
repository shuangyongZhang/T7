using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace HT.Framework
{
    internal sealed class LoadHandle : IReference
    {
        internal string Location { get; private set; }
        internal AssetHandle AssetHandle { get; private set; }
        internal int RefCount { get; private set; }
        internal Object Object { get; private set; }
        /// <summary>
        /// 是否正在加载中
        /// </summary>
        internal bool WaitAsync { get; private set; }

        internal LinkedList<int> InstanceIDList { get; } = new LinkedList<int>();

        internal void SetLocation(string location)
        {
            Location = location;
        }

        internal void ResetHandle(Object obj, AssetHandle assetHandle)
        {
            Object = obj;
            AssetHandle = assetHandle;
        }

        internal void AddRefCount()
        {
            RefCount++;
        }

        internal bool RemoveRefCount()
        {
            if (RefCount <= 0)
            {
                return false;
            }

            RefCount--;
            return true;
        }

        internal void AddInstanceID(int instanceID)
        {
            InstanceIDList.AddLast(instanceID);
        }

        internal void RemoveInstanceID(int instanceID)
        {
            InstanceIDList.Remove(instanceID);
        }

        internal void SetWaitAsync(bool value)
        {
            WaitAsync = value;
        }

        public void Reset()
        {
            Location = string.Empty;
            AssetHandle = null;
            RefCount = 0;
            Object = null;
            WaitAsync = false;
            InstanceIDList.Clear();
        }
    }
}
