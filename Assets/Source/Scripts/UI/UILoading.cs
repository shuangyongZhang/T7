using HT.Framework;
using TMPro;

namespace LB
{
    /// <summary>
    /// UI-Loading
    /// </summary>
    [UIResource("UILoadingPanel")]
    // [UIResource("ui", "UILoadingPanel", "Prefabs/UI/UILoadingPanel")]
    public class UILoading : UILogicTemporary
    {
        [InjectPath("Text (TMP)")] private TMP_Text tipsText;
        /// <summary>
        /// 初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
        }
        /// <summary>
        /// 打开UI
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            tipsText.text = "";
            if (args.Length > 0)
            {
                string text = (string)args[0];
                tipsText.text = text;

            }
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// UI逻辑刷新
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}