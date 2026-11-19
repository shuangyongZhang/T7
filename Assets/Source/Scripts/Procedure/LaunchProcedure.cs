
using System;
using Cysharp.Threading.Tasks;
using HT.Framework;


namespace LB
{
    /// <summary>
    /// 加载流程
    /// </summary>
    public class LaunchProcedure : ProcedureBase
    {

        /// <summary>
        /// 流程初始化
        /// </summary>
        public override void OnInit()
        {
            Main.m_Resource.InitializationCompleted += OnInitializationCompleted;

        }

        private void OnInitializationCompleted(bool arg)
        {
            Log.Info("OnInitializationCompleted " + arg);


            Main.m_Procedure.SwitchProcedure<LoginProcedure>();
        }

        /// <summary>
        /// 进入流程
        /// </summary>
        /// <param name="lastProcedure">上一个离开的流程</param>
        public override void OnEnter(ProcedureBase lastProcedure)
        {
            base.OnEnter(lastProcedure);

        }



        /// <summary>
        /// 离开流程
        /// </summary>
        /// <param name="nextProcedure">下一个进入的流程</param>
        public override void OnLeave(ProcedureBase nextProcedure)
        {

            base.OnLeave(nextProcedure);

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
