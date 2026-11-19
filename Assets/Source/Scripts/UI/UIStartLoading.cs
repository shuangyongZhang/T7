using HT.Framework;
using TMPro;

namespace LB
{
    /// <summary>
    /// UI-Loading
    /// </summary>
    [UIResource("UIStartLoading")]
    public class UIStartLoading : UILogicTemporary
    {
        [InjectPath("Text (TMP)")] private TextMeshProUGUI loadingText;

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
            loadingText.text = args[0].ToString();
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