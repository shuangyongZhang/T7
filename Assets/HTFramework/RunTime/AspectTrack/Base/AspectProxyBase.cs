using System;
using System.Reflection;

namespace HT.Framework
{
    /// <summary>
    /// 切面代理者基类
    /// </summary>
    /// <remarks>
    /// Unity 2021+ 默认使用 .NET Standard 2.1，已移除 System.Runtime.Remoting / RealProxy。
    /// 这里改为直接返回真实对象的轻量实现（与 IsEnableAspectTrack = false 时的原有行为一致），
    /// 并保留原有的抽象方法，确保现有派生类与调用方继续可用。
    /// </remarks>
    public abstract class AspectProxyBase<T> where T : IAspectTrackObject
    {
        private const string VOIDSIGN = "Void";

        protected T _realObject;

        /// <summary>
        /// 真实对象
        /// </summary>
        public T RealObject
        {
            get { return _realObject; }
        }

        public AspectProxyBase(T realObject)
        {
            _realObject = realObject;
        }

        /// <summary>
        /// 获取代理对象（.NET Standard 2.1 下 RealProxy 不可用，直接返回真实对象）
        /// </summary>
        /// <returns>代理对象</returns>
        public virtual object GetTransparentProxy()
        {
            return _realObject;
        }

        /// <summary>
        /// 透传执行方法（保留拦截钩子语义，但不再依赖 RealProxy 消息管道）
        /// </summary>
        protected object Invoke(MethodBase method, object[] args)
        {
            bool isVoid = false;
            MethodInfo info = method as MethodInfo;
            if (info != null)
            {
                isVoid = (info.ReturnType.Name == VOIDSIGN);
                info = null;
            }

            object returnValue = null;
            object[] finalArgs = OnBeforeInvoke(method, args);

            try
            {
                if (isVoid && IsIntercept(method, finalArgs))
                {
                    OnIntercept(method);
                    return null;
                }
                returnValue = method.Invoke(_realObject, finalArgs ?? args);
                return returnValue;
            }
            catch (Exception ex)
            {
                Log.Warning("切面追踪执行异常：" + ex.Message);
                return null;
            }
            finally
            {
                OnAfterInvoke(method, returnValue);
            }
        }

        /// <summary>
        /// 方法被拦截
        /// </summary>
        protected virtual void OnIntercept(MethodBase method)
        { }
        /// <summary>
        /// 方法调用前
        /// </summary>
        protected abstract object[] OnBeforeInvoke(MethodBase method, object[] args);
        /// <summary>
        /// 方法调用后
        /// </summary>
        protected abstract void OnAfterInvoke(MethodBase method, object returnValue);
        /// <summary>
        /// 是否拦截
        /// </summary>
        private bool IsIntercept(MethodBase methodBase, object[] args)
        {
            if (Main.m_AspectTrack != null && Main.m_AspectTrack.IsEnableIntercept)
            {
                foreach (var condition in Main.m_AspectTrack.InterceptConditions)
                {
                    if (condition.Value != null && condition.Value.Invoke(methodBase, args))
                        return true;
                }
            }
            return false;
        }
    }
}
