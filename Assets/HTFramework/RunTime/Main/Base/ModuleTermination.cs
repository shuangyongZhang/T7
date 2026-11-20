using System.Collections.Generic;

namespace HT.Framework
{
    /// <summary>集中管理退出依赖，不改变模块初始化及更新的优先级。</summary>
    internal static class ModuleTermination
    {
        internal static void Run(IList<IModuleManager> modules, IModuleManager ui,
            IModuleManager resource, IModuleManager events, IModuleManager referencePool)
        {
            // OnClose/OnDestroy 仍需要事件、业务服务和资源引用，先关闭 UI。
            ui?.OnTerminate();
            for (int i = 0; i < modules.Count; i++)
            {
                IModuleManager module = modules[i];
                if (ReferenceEquals(module, ui) || ReferenceEquals(module, resource)
                    || ReferenceEquals(module, events) || ReferenceEquals(module, referencePool))
                    continue;
                module.OnTerminate();
            }

            // Entity 等模块归还资源后，再销毁基础服务；事件对象最后归还引用池。
            resource?.OnTerminate();
            events?.OnTerminate();
            referencePool?.OnTerminate();
        }
    }
}
