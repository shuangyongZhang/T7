using System;

namespace HT.Framework
{
    /// <summary>
    /// 实体资源标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class EntityResourceAttribute : Attribute
    {
        /// <summary>YooAsset 地址。</summary>
        public string Location => AssetPath;

        /// <summary>使用单个 YooAsset 地址声明实体。</summary>
        public EntityResourceAttribute(string location, bool useObjectPool = false)
            : this(string.Empty, location, location, useObjectPool)
        {
        }

        public string AssetBundleName { get; private set; }
        public string AssetPath { get; private set; }
        public string ResourcePath { get; private set; }
        public bool IsUseObjectPool { get; private set; }

        public EntityResourceAttribute(string assetBundleName, string assetPath, string resourcePath, bool useObjectPool = false)
        {
            AssetBundleName = assetBundleName;
            AssetPath = assetPath;
            ResourcePath = resourcePath;
            IsUseObjectPool = useObjectPool;
        }
    }
}
