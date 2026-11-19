using HT.Framework;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using System;

namespace LB
{
    /// <summary>
    /// UI-登录
    /// </summary>
    [UIResource("UILoginPanel")]
    // [UIResource("ui", "UILoginPanel", "Prefabs/UI/UILoginPanel")]
    public class UILogin : UILogicResident
    {
        [InjectPath("LoginPanel/loginView")] private GameObject loginView;
        [InjectPath("LoginPanel/loginView/UsernameInputField")] private TMP_InputField usernameInputField;
        [InjectPath("LoginPanel/loginView/PasswordInputField")] private TMP_InputField passwordInputField;
        [InjectPath("LoginPanel/loginView/RememberToggle")] private Toggle rememberToggle;
        [InjectPath("LoginPanel/loginView/LoginButton")] private Button loginButton;
        [InjectPath("LoginPanel/loginView/registerButton")] private Button loginRegisterButton;


        [InjectPath("LoginPanel/registerView")] private GameObject registerView;
        [InjectPath("LoginPanel/registerView/UsernameInputField")] private TMP_InputField registerUsernameInputField;
        [InjectPath("LoginPanel/registerView/PasswordInputField")] private TMP_InputField registerPasswordInputField;
        [InjectPath("LoginPanel/registerView/confirmInputField")] private TMP_InputField confirmInputField;
        [InjectPath("LoginPanel/registerView/RoleDropdown")] private TMP_Dropdown roleDropdown;
        [InjectPath("LoginPanel/registerView/DeptInputField")] private TMP_InputField deptInputField;
        [InjectPath("LoginPanel/registerView/TeleInputField")] private TMP_InputField teleInputField;
        [InjectPath("LoginPanel/registerView/registerButton")] private Button registerButton;
        [InjectPath("LoginPanel/registerView/jumpLogin")] private Button jumpLoginButton;

        [InjectPath("LoginPanel/LoginTipsText")] private TMP_Text loginTipsText;
        [InjectPath("LoginPanel/QuickLoginButton")] private Button quickLoginButton;
        [InjectPath("LoginPanel/WelcomeText")] private TMP_Text welcomeText;
        [InjectPath("LoginPanel/closeBtn")] private Button closeBtn;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

            loginButton.onClick.AddListener(OnLogin);
            //quickLoginButton.onClick.AddListener(OnQuickLogin);
            loginRegisterButton.onClick.AddListener(OnLoginRegisterClick);
            jumpLoginButton.onClick.AddListener(OnJumpLoginClick);
            registerButton.onClick.AddListener(OnRegisterClick);
            closeBtn.onClick.AddListener(OnQuit);

            // welcomeText.text = AppDefine.AppName + " V" + Application.version.Substring(0, 1);

        }

        private void OnQuickLogin()
        {

            Global.CurAdminInfo = new AdminInfo()
            {
                Account = "admin",
                Password = SaveConfigModule.UserData.AdminPassword,
                Role = Role.Administrator,
            };
            Global.IsQuickLogin = true;
            Main.m_Procedure.SwitchProcedure<MainProcedure>();
        }


        private void OnLogin()
        {
            string username = usernameInputField.text;
            string password = passwordInputField.text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                loginTipsText.text = "提示：账号名或密码不能为空";
                return;
            }

            AdminInfo adminInfo = null;
            if (username == "admin")
            {
                var adminPassword = SaveConfigModule.UserData.AdminPassword;
                if (adminPassword != password)
                {
                    loginTipsText.text = "提示：用户密码错误";
                    return;
                }
                adminInfo = new AdminInfo()
                {
                    Account = "admin",
                    Password = adminPassword,
                    Role = Role.Administrator,
                };
            }
            else
            {
                adminInfo = DBManagerModule.Instance.QueryAdminInfoTableByName(username);
                if (adminInfo == null)
                {
                    loginTipsText.text = "提示：用户不存在";
                    return;
                }
                if (adminInfo.IsForbidden)
                {
                    loginTipsText.text = "提示：用户被禁用, 请联系管理员解禁";
                    return;
                }
                if (adminInfo.Password != password)
                {
                    loginTipsText.text = "提示：用户密码错误";
                    return;
                }

            }
            SaveConfigModule.Instance.SetUserData("userName", rememberToggle.isOn ? username : "");
            SaveConfigModule.Instance.SetUserData("password", rememberToggle.isOn ? password : "");
            SaveConfigModule.Instance.SaveUserData();


            Global.CurAdminInfo = adminInfo;
            Global.CurHospitalInfo = DBManagerModule.Instance.QueryHospitalInfo();
            Global.IsQuickLogin = false;
            Main.m_Procedure.SwitchProcedure<MainProcedure>();
        }

        private void OnLoginRegisterClick()
        {
            loginView.SetActive(false);
            registerView.SetActive(true);
        }
        private void OnJumpLoginClick()
        {
            loginView.SetActive(true);
            registerView.SetActive(false);
            usernameInputField.text = SaveConfigModule.Instance.GetUserData<string>("userName");
            passwordInputField.text = SaveConfigModule.Instance.GetUserData<string>("password");
        }
        private void OnQuit()
        {
            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "确认关闭应用?", "确定", "取消", (index) =>
            {
                Main.m_UI.OpenUI<UIStartLoading>("应用关闭中......").Forget();
                Application.Quit();
                return true;
            });
        }

        private void OnRegisterClick()
        {
            string account = registerUsernameInputField.text;
            string password1 = registerPasswordInputField.text;
            string password2 = confirmInputField.text;
            if (string.IsNullOrEmpty(account))
            {
                loginTipsText.text = "提示：账号不能为空";
                return;
            }
            if (string.IsNullOrEmpty(password1) || string.IsNullOrEmpty(password2))
            {
                loginTipsText.text = "提示：密码不能为空";
                return;
            }
            if (account == "admin")
            {
                loginTipsText.text = "提示：不能使用admin作为账号名";
                return;
            }
            var adminInfo = DBManagerModule.Instance.QueryAdminInfoTableByName(account);
            if (adminInfo != null)
            {
                loginTipsText.text = "提示：账号名已存在,请输入其它账号名";
                return;
            }
            if (password1 != password2)
            {

                loginTipsText.text = "提示：两次输入密码不同，请重输";
                return;
            }

            AdminInfo adminInfo1 = new AdminInfo();
            adminInfo1.Account = account;
            adminInfo1.Password = password1;
            adminInfo1.Dept = deptInputField.text;
            adminInfo1.Telephone = teleInputField.text;
            adminInfo1.Role = (Role)roleDropdown.value;
            adminInfo1.Sex = Sex.Male;
            adminInfo1.Creator = account;
            adminInfo1.IsForbidden = false;
            adminInfo1.CreateTime = DateTime.Now;

            var result = DBManagerModule.Instance.InsertAdminInfo(adminInfo1);
            if (result)
            {
                loginTipsText.text = "提示：账号创建成功";
                SaveConfigModule.Instance.SetUserData("userName", account);
                SaveConfigModule.Instance.SetUserData("password", password1);
                SaveConfigModule.Instance.SaveUserData();
                OnJumpLoginClick();
            }
            else
            {
                loginTipsText.text = "提示：账号创建失败，请检查此账号名是否已经注册!";
            }
        }
        /// <summary>
        /// 打开UI
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);

            loginView.SetActive(true);
            registerView.SetActive(false);

            loginTipsText.text = "";

            usernameInputField.text = SaveConfigModule.Instance.GetUserData<string>("userName");
            passwordInputField.text = SaveConfigModule.Instance.GetUserData<string>("password");
        }

        /// <summary>
        /// 置顶UI
        /// </summary>
        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
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
            loginButton.onClick.RemoveAllListeners();
            //quickLoginButton.onClick.RemoveAllListeners();
            loginRegisterButton.onClick.RemoveAllListeners();
            jumpLoginButton.onClick.RemoveAllListeners();
            registerButton.onClick.RemoveAllListeners();
            closeBtn.onClick.RemoveAllListeners();

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