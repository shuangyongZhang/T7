using System;
using HT.Framework;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LB
{
    /// <summary>
    /// UI-管理密码输入框
    /// </summary>
    [UIResource("UISuperAdminPasswordDialog")]
    // [UIResource("ui", "UISuperAdminPasswordDialog", "Prefabs/UI/UISuperAdminPasswordDialog")]
    public class UISuperAdminPasswordDialog : UILogicTemporary
    {
        [InjectPath("ConfirmButton")] private Button confirmButton;
        [InjectPath("CancelButton")] private Button cancelButton;

        [InjectPath("Password")] private TMP_InputField passwordInputField;


        private UnityAction yesCallBack;
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

            yesCallBack = null;
            passwordInputField.text = "";

            Action callback = (Action)args[0];
            if (callback != null) yesCallBack = new UnityAction(callback);

            confirmButton.onClick.AddListener(OnConfirm);
            cancelButton.onClick.AddListener(Close);
        }

        private void OnCancel()
        {

        }

        private void OnConfirm()
        {
            var password = passwordInputField.text;
            var adminPassword = SaveConfigModule.UserData.AdminPassword;
            if (adminPassword == password)
            {
                yesCallBack?.Invoke();
                yesCallBack = null;
                Close();
            }
            else
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "密码错误,请重新输入!", null, "确定");

            }
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();

            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
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