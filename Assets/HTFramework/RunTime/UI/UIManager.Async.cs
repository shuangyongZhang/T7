using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HT.Framework
{
    public sealed partial class UIManager
    {
        private readonly Dictionary<Type, SemaphoreSlim> _uiGates = new Dictionary<Type, SemaphoreSlim>();
        private readonly Dictionary<Type, int> _uiRevisions = new Dictionary<Type, int>();
        private readonly SemaphoreSlim _temporaryUIGate = new SemaphoreSlim(1, 1);
        private bool _uiTerminated;

        private int GetUIRevision(Type type) => _uiRevisions.TryGetValue(type, out var revision) ? revision : 0;

        private async UniTask RunUIOperation(Type type, IEnumerator operation)
        {
            if (!_uiGates.TryGetValue(type, out var gate))
            {
                gate = type.IsSubclassOf(typeof(UILogicTemporary)) ? _temporaryUIGate : new SemaphoreSlim(1, 1);
                _uiGates.Add(type, gate);
            }
            int revision = GetUIRevision(type);
            await gate.WaitAsync(this.GetCancellationTokenOnDestroy());
            try
            {
                if (_uiTerminated || revision != GetUIRevision(type)) throw new OperationCanceledException();
                await CoroutineTask.Run(operation, this);
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary>等待 UI 淡出和 OnClose 完成，再执行依赖关闭结果的业务。</summary>
        public UniTask CloseUIAsync<T>() where T : UILogicBase => CloseUIAsync(typeof(T));

        /// <summary>等待指定 UI 完成关闭。</summary>
        public UniTask CloseUIAsync(Type type)
        {
            if (type.IsAbstract || !type.IsSubclassOf(typeof(UILogicBase))) return UniTask.CompletedTask;
            return RunUIOperation(type, _helper.CloseUI(type));
        }

        /// <summary>阻止退出过程中继续创建 UI。</summary>
        public override void OnTerminate()
        {
            _uiTerminated = true;
            base.OnTerminate();
        }
    }
}
