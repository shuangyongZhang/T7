using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HT.Framework
{
    /// <summary>使用 Unity 原生异步实例化，并保留框架的本地坐标及 UI 布局约定。</summary>
    internal static class AsyncPrefabInstantiation
    {
        internal static async UniTask<GameObject> CreateAsync(GameObject prefab, Transform parent, bool isUI)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            bool hasParent = !ReferenceEquals(parent, null);
            if (hasParent && parent == null) throw new OperationCanceledException("预制体父节点已销毁。");
            CancellationToken token = hasParent ? parent.GetCancellationTokenOnDestroy() : CancellationToken.None;
            token.ThrowIfCancellationRequested();

            var operation = Object.InstantiateAsync(prefab, parent);
            try
            {
                GameObject[] instances = await operation.ToUniTask(cancellationToken: token);
                token.ThrowIfCancellationRequested();
                if (hasParent && parent == null) throw new OperationCanceledException("预制体父节点已销毁。");
                if (instances == null || instances.Length != 1 || instances[0] == null)
                    throw new InvalidOperationException($"异步实例化没有返回有效对象：{prefab.name}");

                GameObject clone = instances[0];
                if (isUI)
                {
                    var sourceRect = prefab.transform as RectTransform;
                    var cloneRect = clone.transform as RectTransform;
                    if (sourceRect == null || cloneRect == null)
                        throw new InvalidOperationException($"UI 预制体缺少 RectTransform：{prefab.name}");
                    cloneRect.anchorMin = sourceRect.anchorMin;
                    cloneRect.anchorMax = sourceRect.anchorMax;
                    cloneRect.sizeDelta = sourceRect.sizeDelta;
                    cloneRect.anchoredPosition3D = sourceRect.anchoredPosition3D;
                }
                else
                {
                    clone.transform.localPosition = prefab.transform.localPosition;
                }
                clone.transform.localRotation = Quaternion.identity;
                clone.transform.localScale = Vector3.one;
                return clone;
            }
            catch
            {
                // UniTask 的取消只结束等待；仍需取消原生任务，并在其结束后才允许调用方释放模板资源。
                if (!operation.isDone)
                {
                    operation.Cancel();
                    await ((AsyncOperation)operation).ToUniTask();
                }
                // 取消恰好发生在创建完成后，或布局设置失败时，回收尚未交给调用方的实例。
                var instances = operation.Result;
                if (instances != null)
                {
                    foreach (var instance in instances)
                    {
                        if (instance != null) Object.Destroy(instance);
                    }
                }
                throw;
            }
        }
    }
}
