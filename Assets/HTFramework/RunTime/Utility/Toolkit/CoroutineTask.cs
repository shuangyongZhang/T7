using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HT.Framework
{
    /// <summary>使用 Unity 的等待指令调度，同时将嵌套协程异常传递给 UniTask 调用方。</summary>
    internal static class CoroutineTask
    {
        internal static async UniTask Run(IEnumerator routine, MonoBehaviour owner)
        {
            if (owner == null) throw new OperationCanceledException("协程宿主已销毁。");
            var source = new UniTaskCompletionSource();
            var token = owner.GetCancellationTokenOnDestroy();
            var execution = Execute(routine, source);
            var registration = token.Register(() => source.TrySetCanceled(token));
            Coroutine coroutine = null;
            try
            {
                token.ThrowIfCancellationRequested();
                coroutine = owner.StartCoroutine(execution);
                await source.Task;
            }
            finally
            {
                registration.Dispose();
                if (coroutine != null && owner != null) owner.StopCoroutine(coroutine);
                (execution as IDisposable)?.Dispose();
            }
        }

        private static IEnumerator Execute(IEnumerator routine, UniTaskCompletionSource source)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(routine);
            Exception error = null;
            try
            {
                while (stack.Count > 0)
                {
                    object current = null;
                    bool moved = false;
                    try
                    {
                        moved = stack.Peek().MoveNext();
                        if (moved) current = stack.Peek().Current;
                        else (stack.Pop() as IDisposable)?.Dispose();
                    }
                    catch (Exception exception)
                    {
                        error = exception;
                    }
                    if (error != null)
                    {
                        break;
                    }
                    if (moved && current is IEnumerator nested)
                    {
                        stack.Push(nested);
                    }
                    else if (moved)
                    {
                        yield return current;
                    }
                }
            }
            finally
            {
                while (stack.Count > 0)
                {
                    try { (stack.Pop() as IDisposable)?.Dispose(); }
                    catch (Exception exception) { if (error == null) error = exception; }
                }
            }
            if (error != null) source.TrySetException(error);
            else source.TrySetResult();
        }
    }
}
