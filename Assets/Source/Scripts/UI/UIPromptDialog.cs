using System;
using Cysharp.Threading.Tasks;
using HT.Framework;
using TMPro;
using UnityEngine.UI;

namespace LB
{

    public struct DialogInfo
    {
        public string Title;
        public string Message;
        public Action YesCallback;
        public Action NoCallback;

        public string YesCaption;
        public string NoCaption;
    }
    [UIResource("UIPromptDialog")]
    // [UIResource("ui", "UIPromptDialog", "Prefabs/UI/UIPromptDialog")]
    public class UIPromptDialog : UILogicTemporary
    {
        [InjectPath("Pop/text/head")] private TMP_Text headText;
        [InjectPath("Pop/text/content")] private TMP_Text contentText;
        [InjectPath("Pop/BtnGroup/yesButton")] private Button yesButton;
        [InjectPath("Pop/BtnGroup/yesButton/Text")] private TMP_Text yesCaptain;

        [InjectPath("Pop/BtnGroup/noButton")] private Button noButton;
        [InjectPath("Pop/BtnGroup/noButton/Text")] private TMP_Text noCaptain;


        private DialogInfo dialogInfo;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

            yesButton.onClick.AddListener(OnYesBtnClicked);
            noButton.onClick.AddListener(OnNoBtnClicked);
        }
        /// <summary>
        /// 打开UI
        /// </summary>
        public override void OnOpen(params object[] args)
        {

            base.OnOpen(args);
            if (args.Length > 0 && args[0] != null)
            {
                dialogInfo = (DialogInfo)args[0];
            }

            headText.text = dialogInfo.Title;
            contentText.text = dialogInfo.Message;
            yesButton.gameObject.SetActive(!string.IsNullOrEmpty(dialogInfo.YesCaption));
            noButton.gameObject.SetActive(!string.IsNullOrEmpty(dialogInfo.NoCaption));
            noCaptain.text = dialogInfo.NoCaption;
            yesCaptain.text = dialogInfo.YesCaption;
        }

        private void OnNoBtnClicked()
        {
            dialogInfo.NoCallback?.Invoke();
            Close();
        }

        private void OnYesBtnClicked()
        {
            dialogInfo.YesCallback?.Invoke();
            Close();
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

            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// UI逻辑刷新
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public static void ShowPromptDialog(string title, string message, string yesCaption, string noCaption, Action yesCallback = null, Action noCallback = null)
        {
            DialogInfo dialogInfo = new DialogInfo()
            {
                Title = title,
                Message = message,
                YesCallback = yesCallback,
                NoCallback = noCallback,
                YesCaption = yesCaption,
                NoCaption = noCaption,

            };
            Main.m_UI.OpenUI<UIPromptDialog>(dialogInfo).Forget();
        }
    }
}