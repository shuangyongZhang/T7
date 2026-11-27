
using Cysharp.Threading.Tasks;
using HT.Framework;
namespace LB
{
    /// <summary>
    ///  主流程
    /// </summary>
    public class MainProcedure : ProcedureBase
    {

        /// <summary>
        /// 流程初始化
        /// </summary>
        public override void OnInit()
        {


        }
        private async UniTask InitAsync()
        {
            await AppBootstrap.Instance.InitializeOnMain();
            await Main.m_UI.PreloadingUI<UIReport>();
            // await Main.m_UI.PreloadingUI<UIPatientInfo>();
            await Main.m_UI.OpenUI<UIMain>();
            Main.m_UI.CloseUI<UIStartLoading>();

        }
        /// <summary>
        /// 进入流程
        /// </summary>
        /// <param name="lastProcedure">上一个离开的流程</param>
        public override void OnEnter(ProcedureBase lastProcedure)
        {
            base.OnEnter(lastProcedure);


            InitAsync().Forget();

        }


        /// <summary>
        /// 离开流程
        /// </summary>
        /// <param name="nextProcedure">下一个进入的流程</param>
        public override void OnLeave(ProcedureBase nextProcedure)
        {

            base.OnLeave(nextProcedure);
            Main.m_UI.CloseUI<UISetting>();
            Main.m_UI.CloseUI<UIPatientInfo>();
            Main.m_UI.CloseUI<UIMain>();
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