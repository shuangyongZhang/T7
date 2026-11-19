
using System;
using Cysharp.Threading.Tasks;
using HT.Framework;

namespace LB
{
    /// <summary>
    /// 登录流程
    /// </summary>
    public class LoginProcedure : ProcedureBase
    {
        /// <summary>
        /// 流程初始化
        /// </summary>
        public override void OnInit()
        {

        }
        /// <summary>
        /// 进入流程
        /// </summary>
        /// <param name="lastProcedure">上一个离开的流程</param>
        public override void OnEnter(ProcedureBase lastProcedure)
        {
            base.OnEnter(lastProcedure);
            AppBootstrap.Instance.InitializeOnLogin();
            // Main.m_UI.PreloadingUI<UIKeyboard>().Forget();
            Main.m_UI.OpenUI<UILogin>().Forget();
            if (Main.m_Debug.IsEnableDebugger)
                BetaLogoShow.Instance.Init();
        }

        /// <summary>
        /// 离开流程
        /// </summary>
        /// <param name="nextProcedure">下一个进入的流程</param>
        public override void OnLeave(ProcedureBase nextProcedure)
        {

            base.OnLeave(nextProcedure);
            Main.m_UI.CloseUI<UILogin>();
            Main.m_UI.DestroyUI<UILogin>();
            Main.m_UI.OpenUI<UIStartLoading>("数据载入中......").Forget();
        }


        /// <summary>
        /// 流程帧刷新
        /// </summary>
        public override void OnUpdate()
        {

        }

        /// <summary>
        /// 流程帧刷新（秒）
        /// </summary>
        public override void OnUpdateSecond()
        {

        }
    }

}
