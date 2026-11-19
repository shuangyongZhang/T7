using System;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TheraBytes.BetterUi;
using System.Collections.Generic;
using UIWidgets;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.UI.ProceduralImage;


namespace LB
{
    public enum VersionType
    {
        Alpha,
        Beta,
        RC,
        Release
    }
    /// <summary>
    /// UI-设置
    /// </summary>
    [UIResource("UISettingPanel")]
    // [UIResource("ui", "UISettingPanel", "Prefabs/UI/UISettingPanel")]
    public class UISetting : UILogicResident
    {

        [InjectPath("ButtonPanel")] private BetterButtonToggleGroup buttonToggleGroup;
        [InjectPath("ButtonPanel/GeneralButton")] private BetterButtonToggle generalButton;
        [InjectPath("ButtonPanel/DoctorButton")] private BetterButtonToggle doctorButton;

        [InjectPath("ButtonPanel/HospitalButton")] private BetterButtonToggle hospitalButton;
        [InjectPath("ButtonPanel/HospitalButton/LockImage")] private Image hospitalLockImage;
        [InjectPath("ButtonPanel/DoctorButton/LockImage")] private Image doctorLockImage;
        [InjectPath("GeneralPanel")] private RectTransform generalPanel;
        [InjectPath("DoctorPanel")] private RectTransform doctorPanel;
        [InjectPath("DoctorPanel/ListView")] private AdminListView adminListView;
        [InjectPath("DoctorPanel/OpenAddPanelButton")] private Button openAddPanelButton;
        [InjectPath("DoctorPanel/InfoPanel/SaveButton")] private Button saveButton;
        [InjectPath("DoctorPanel/InfoPanel/EditButton")] private Button editButton;
        [InjectPath("DoctorPanel/InfoPanel/ForbidButton")] private Button forbidButton;
        [InjectPath("DoctorPanel/InfoPanel/PermitButton")] private Button permitButton;
        [InjectPath("DoctorPanel/InfoPanel/NameInputField")] private TMP_InputField infoPanel_nameInputField;
        [InjectPath("DoctorPanel/InfoPanel/AccountInputField")] private TMP_InputField infoPanel_accountInputField;
        [InjectPath("DoctorPanel/InfoPanel/DeptInputField")] private TMP_InputField infoPanel_deptInputField;
        [InjectPath("DoctorPanel/InfoPanel/TeleInputField")] private TMP_InputField infoPanel_teleInputField;
        [InjectPath("DoctorPanel/InfoPanel/RoleDropdown")] private TMP_Dropdown infoPanel_roleDropdown;
        [InjectPath("DoctorPanel/InfoPanel/Password1InputField")] private TMP_InputField infoPanel_password1InputField;
        [InjectPath("DoctorPanel/InfoPanel/Password2InputField")] private TMP_InputField infoPanel_password2InputField;
        [InjectPath("DoctorPanel/InfoPanel/SexDropdown")] private TMP_Dropdown infoPanel_sexDropdown;
        [InjectPath("DoctorPanel/InfoPanel/AvatarImage")] private Image infoPanel_avatarImage;
        [InjectPath("DoctorPanel/AddPanel")] private RectTransform doctorAddPanel;
        [InjectPath("DoctorPanel/AddPanel/Content")] private ProceduralImage doctorAddPanelContent;
        [InjectPath("DoctorPanel/AddPanel/Content/NameInputField")] private TMP_InputField addPanel_nameInputField;
        [InjectPath("DoctorPanel/AddPanel/Content/AvatarImage")] private Image addPanel_avatarImage;
        [InjectPath("DoctorPanel/AddPanel/Content/AccountInputField")] private TMP_InputField addPanel_accountInputField;
        [InjectPath("DoctorPanel/AddPanel/Content/DeptInputField")] private TMP_InputField addPanel_deptInputField;
        [InjectPath("DoctorPanel/AddPanel/Content/TeleInputField")] private TMP_InputField addPanel_teleInputField;
        [InjectPath("DoctorPanel/AddPanel/Content/RoleDropdown")] private TMP_Dropdown addPanel_roleDropdown;
        [InjectPath("DoctorPanel/AddPanel/Content/Password1InputField")] private TMP_InputField addPanel_password1InputField;
        [InjectPath("DoctorPanel/AddPanel/Content/Password2InputField")] private TMP_InputField addPanel_password2InputField;

        // [InjectPath("DoctorPanel/AddPanel/Content/SexToggleGroup/Male")] private Toggle addPanel_maleToggle;
        // [InjectPath("DoctorPanel/AddPanel/Content/SexToggleGroup/Female")] private Toggle addPanel_femaleToggle;
        [InjectPath("DoctorPanel/AddPanel/Content/SexDropdown")] private TMP_Dropdown addPanel_sexDropdown;
        [InjectPath("DoctorPanel/AddPanel/Content/AddButton")] private Button addButton;
        [InjectPath("DoctorPanel/AddPanel/Content/CancelButton")] private Button cancelButton;


        [InjectPath("HospitalPanel")] private RectTransform hospitalPanel;
        [InjectPath("HospitalPanel/NameInputField")] private TMP_InputField hospitalNameInputField;
        [InjectPath("HospitalPanel/CodeInputField")] private TMP_InputField hospitalCodeInputField;
        [InjectPath("HospitalPanel/DeptInputField")] private TMP_InputField hospitalDeptInputField;
        [InjectPath("HospitalPanel/TeleInputField")] private TMP_InputField hospitalTeleInputField;
        [InjectPath("HospitalPanel/AddressInputField")] private TMP_InputField hospitalAddressInputField;
        // [InjectPath("HospitalPanel/DescInputField")] private TMP_InputField hospitalDescInputField;

        [InjectPath("HospitalPanel/HospitalLogo/SelectButton")] private Button selectLogoButton;
        [InjectPath("HospitalPanel/HospitalLogo/DeleteButton")] private Button deleteLogoButton;
        [InjectPath("HospitalPanel/HospitalLogo/Image/Logo")] private RawImage logoImage;
        [InjectPath("HospitalPanel/HospitalLogo/Image/Spare")] private RawImage spareImage;

        [InjectPath("HospitalPanel/LastModifyTime")] private TMP_Text lastModifyTime;
        [InjectPath("HospitalPanel/SaveButton")] private Button saveHospitalButton;
        [InjectPath("GeneralPanel/MusicSlider")] private Slider musicSlider;
        [InjectPath("GeneralPanel/SoundSlider")] private Slider soundSlider;

        [InjectPath("GeneralPanel/MusicSlider/Volume")] private TMP_Text musicText;
        [InjectPath("GeneralPanel/SoundSlider/Volume")] private TMP_Text soundText;
        [InjectPath("GeneralPanel/VersionText")] private TMP_Text versionText;
        [InjectPath("GeneralPanel/ReleaseVersionText")] private TMP_Text releaseVersionText;
        [InjectPath("GeneralPanel/DeviceText")] private TMP_Text deviceText;
        [InjectPath("GeneralPanel/DateText")] private TMP_Text releaseDateText;
        [InjectPath("GeneralPanel/TelephoneText")] private TMP_Text telephoneText;
        [InjectPath("GeneralPanel/AddressText")] private TMP_Text addressText;
        [InjectPath("GeneralPanel/WebsiteText")] private TMP_Text websiteText;
        [InjectPath("GeneralPanel/HelpButton")] private Button helpButton;
        private int curIndex;

        private List<AdminInfo> adminInfos;

        private AdminInfo CurAdminInfo
        {
            get;
            set;
        }


        private string versionType = "";
        private UnityTimer.Timer versionTimer;
        private bool hasChecked = false;



        /// <summary>
        /// 初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            InitGeneralSettingPanel();

            adminListView.Init();

            var roleList = EnumHelper.GetDescriptions(typeof(Role));
            addPanel_roleDropdown.ClearOptions();
            addPanel_roleDropdown.AddOptions(roleList);

            infoPanel_roleDropdown.ClearOptions();
            infoPanel_roleDropdown.AddOptions(roleList);

            var sexList = EnumHelper.GetDescriptions(typeof(Sex));
            addPanel_sexDropdown.ClearOptions();
            addPanel_sexDropdown.AddOptions(sexList);

            infoPanel_sexDropdown.ClearOptions();
            infoPanel_sexDropdown.AddOptions(sexList);

            addPanel_sexDropdown.onValueChanged.AddListener((value) =>
            {
                addPanel_avatarImage.sprite = (Sex)value == Sex.Female ? Global.IconDic["femaleDoctor"] : Global.IconDic["maleDoctor"];
                doctorAddPanelContent.sprite = (Sex)value == Sex.Female ? Global.IconDic["womenBigK"] : Global.IconDic["manBigK"];
            });
            infoPanel_sexDropdown.onValueChanged.AddListener((value) =>
            {
                infoPanel_avatarImage.sprite = (Sex)value == Sex.Female ? Global.IconDic["femaleDoctor"] : Global.IconDic["maleDoctor"];
            });


            openAddPanelButton.onClick.AddListener(OnOpenAdminAddPanel);
            cancelButton.onClick.AddListener(OnCloseAdminAddPanel);
            addButton.onClick.AddListener(OnAddAdmin);

            saveButton.onClick.AddListener(OnSaveAdmin);
            forbidButton.onClick.AddListener(OnForbidAdmin);
            permitButton.onClick.AddListener(OnPermitAdmin);
            editButton.onClick.AddListener(OnEditAdmin);

            adminListView.OnSelect.AddListener(OnAdminItemSelected);
            generalButton.onClick.AddListener(OnOpenGeneralPanel);
            doctorButton.onClick.AddListener(OnOpenDoctorPanel);

            hospitalButton.onClick.AddListener(OnOpenHospitalPanel);

            saveHospitalButton.onClick.AddListener(OnSaveHospitalInfo);
            selectLogoButton.onClick.AddListener(OnSelectHospitalLogo);
            deleteLogoButton.onClick.AddListener(OnDeleteHospitalLogo);
        }



        private void InitGeneralSettingPanel()
        {
            // versionType = Main.Current.GetStringParameter("VersionType");
            // versionType = string.IsNullOrWhiteSpace(versionType) ? "Release" : versionType;
            // versionType = Main.m_Debug.IsEnableDebugger ? "Debug" : versionType;
            versionType = VersionManager.VersionType;

            if (versionType == "Release")
            {
                versionText.text = string.Format("软件完整版本：{0}", Application.version);
                releaseVersionText.text = string.Format("软件发布版本：{0}", Application.version.Split('.')[0]);
            }
            else
            {
                versionText.text = string.Format("软件完整版本：{0} - <color=red>{1}", Application.version, versionType);
                releaseVersionText.text = string.Format("软件发布版本：{0}", Application.version.Split('.')[0]);
            }
            // releaseDateText.text = string.Format("发布日期：{0}", Main.Current.GetStringParameter("releaseDateText"));
            releaseDateText.text = string.Format("软件发布日期：{0}", releaseDateText.text = VersionManager.BuildDate);

            telephoneText.text = "联系电话：177 1241 0781/025-52313235";
            addressText.text = "地    址：南京高淳经济开发区双高路86号医疗器械产业园24号";
            websiteText.text = "网    站：www.jslvbai.com";

            helpButton.onClick.AddListener(() =>
            {
                var helpName = ConfigDataManager.Instance.IsCompensation ? "help_c.pdf" : "help.pdf";
                Application.OpenURL(Application.streamingAssetsPath + "/" + helpName);
            });

            musicSlider.value = Main.m_Audio.BackgroundVolume * 100;
            soundSlider.value = Main.m_Audio.SingleVolume * 100;
            musicText.text = musicSlider.value.ToString();
            soundText.text = soundSlider.value.ToString();

            musicSlider.onValueChanged.AddListener(_ =>
            {
                musicText.text = musicSlider.value.ToString();
                Main.m_Audio.BackgroundVolume = musicSlider.value / 100.0f;

            });
            soundSlider.onValueChanged.AddListener(_ =>
            {
                soundText.text = soundSlider.value.ToString();
                Main.m_Audio.OneShootVolume = Main.m_Audio.SingleVolume = Main.m_Audio.MultipleVolume = soundSlider.value / 100.0f;


            });

            versionTimer = UnityTimer.Timer.Register(0.5f, ComparativeVersion, null, true);
            versionTimer.Pause();
        }
        private void CloseAllSettingPanel()
        {
            generalPanel.gameObject.SetActive(false);
            doctorPanel.gameObject.SetActive(false);
            doctorAddPanel.gameObject.SetActive(false);
            hospitalPanel.gameObject.SetActive(false);
        }

        private void OnOpenGeneralPanel()
        {
            CloseAllSettingPanel();
            generalPanel.gameObject.SetActive(true);
            versionTimer?.Resume();

        }
        private void OnOpenDoctorPanel()
        {
            CloseAllSettingPanel();
            doctorPanel.gameObject.SetActive(true);

            CurAdminInfo = null;
            adminInfos = DBManagerModule.Instance.QueryAllAdminInfos();
            if (adminInfos == null) return;
            // //剔除自己
            // int index = adminInfos.FindIndex(e => e.Account == Global.CurAdminInfo.Account);
            // if (index >= 0) adminInfos.RemoveAt(index);


            adminListView.DataSource.Clear();
            adminListView.DataSource.BeginUpdate();
            int index = 0;
            foreach (AdminInfo adminInfo in adminInfos)
            {
                AdminItem data = new AdminItem()
                {
                    Index = index,
                    Account = adminInfo.Account,
                    Name = adminInfo.Name,
                    Sex = adminInfo.Sex,
                    Prohibited = adminInfo.IsForbidden,
                };
                adminListView.DataSource.Add(data);
                index++;
            }

            adminListView.DataSource.EndUpdate();

            SetListViewDefaultSelect();
        }



        private void OnOpenHospitalPanel()
        {
            CloseAllSettingPanel();
            hospitalPanel.gameObject.SetActive(true);


            if (Global.CurHospitalInfo != null)
            {
                hospitalNameInputField.text = Global.CurHospitalInfo.Name;
                hospitalCodeInputField.text = Global.CurHospitalInfo.Code;
                hospitalDeptInputField.text = Global.CurHospitalInfo.Dept;
                hospitalTeleInputField.text = Global.CurHospitalInfo.Telephone;
                hospitalAddressInputField.text = Global.CurHospitalInfo.Address;
                // hospitalDescInputField.text = Global.CurHospitalInfo.Desc;
                lastModifyTime.text = "最后一次更新日期:" + Global.CurHospitalInfo.ModifyTime.ToString("yyyy-MM-dd");
            }
            else
            {
                hospitalNameInputField.text = "";
                hospitalCodeInputField.text = "";
                hospitalDeptInputField.text = "";
                hospitalTeleInputField.text = "";
                hospitalAddressInputField.text = "";
                // hospitalDescInputField.text = "";
                lastModifyTime.text = "";
            }
            LoadHospitalLogo();
        }

        private void LoadHospitalLogo()
        {

            if (FileHelper.IsFileExists(Global.HospitalLogoPath))
            {
                TextureLoader.Load(Global.HospitalLogoPath, result =>
                {
                    if (logoImage != null)
                    {
                        logoImage.texture = result.Texture;
                        if (logoImage.TryGetComponent<AspectRatioFitter>(out var aspectRatioFitter))
                        {
                            var imgSize = CommonHelper.GetImageSize(Global.HospitalLogoPath);
                            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                            aspectRatioFitter.aspectRatio = imgSize.x / imgSize.y;
                        }
                    }
                });
            }
            else
            {
                if (logoImage != null) logoImage.texture = spareImage.texture;
            }

        }
        private void OnSaveHospitalInfo()
        {

            bool result;
            HospitalInfo hospitalInfo;
            if (Global.CurHospitalInfo != null)
            {
                hospitalInfo = Global.CurHospitalInfo.Clone();
                hospitalInfo.Name = hospitalNameInputField.text;
                hospitalInfo.Code = hospitalCodeInputField.text;
                hospitalInfo.Dept = hospitalDeptInputField.text;
                hospitalInfo.Telephone = hospitalTeleInputField.text;
                hospitalInfo.Address = hospitalAddressInputField.text;
                // hospitalInfo.Desc = hospitalDescInputField.text;
                hospitalInfo.ModifyTime = DateTime.Now;
                result = DBManagerModule.Instance.UpdateHospitalInfo(hospitalInfo);
            }
            else
            {
                hospitalInfo = new HospitalInfo
                {
                    Name = hospitalNameInputField.text,
                    Code = hospitalCodeInputField.text,
                    Dept = hospitalDeptInputField.text,
                    Telephone = hospitalTeleInputField.text,
                    Address = hospitalAddressInputField.text,
                    // Desc = hospitalDescInputField.text,
                    ModifyTime = DateTime.Now
                };
                result = DBManagerModule.Instance.InsertHospitalInfo(hospitalInfo);

            }

            if (result)
            {
                Global.CurHospitalInfo = hospitalInfo.Clone();
                lastModifyTime.text = "最后一次更新日期:" + Global.CurHospitalInfo.ModifyTime.ToString("yyyy-MM-dd");
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("保存成功!");
            }
            else
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "写入数据库错误,保存失败!", "确定");
            }
        }

        private void OnSelectHospitalLogo()
        {

            NativeFilePicker.Permission permission = NativeFilePicker.PickFile((imgPath) =>
                {
                    if (!string.IsNullOrEmpty(imgPath))
                    {

                        Log.Info("选择Logo文件成功");

                        var imgSize = CommonHelper.GetImageSize(imgPath);
                        if (imgSize.x > 256 || imgSize.y > 256)
                        {
                            UINotifyDialogHelper.Current.ShowPromptDialog("错误", "图片尺寸在范围在256*256内!", "确定");
                            return;
                        }

                        File.Copy(imgPath, Global.HospitalLogoPath, true);
                        //TextureLoader里有缓存 必须删除缓存才能刷新
                        TextureLoader.RemoveCache(Global.HospitalLogoPath);
                        LoadHospitalLogo();
                        UINotifyDialogHelper.Current.ShowNotifyAutoHide("设置成功!");
                    }
                    else
                    {
                        Log.Info("选择Logo文件失败");
                    }

                }
            , new string[] { NativeFilePicker.ConvertExtensionToFileType("png") });
            Log.Info("Permission result: " + permission);
            if (permission != NativeFilePicker.Permission.Granted)
            {

                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "导出文件需要写入权限,请在Android系统设置里允许应用写入权限!", "确定", null, (index) =>
                {
                    NativeFilePicker.OpenSettings();
                    return true;
                });


            }



            // var list = FileDialogHelper.SelectFile("请选择医院Logo图片", "图片|.png");
            // if (list.Count > 0)
            // {
            //     var imgPath = list.First();
            //     var imgSize = CommonHelper.GetImageSize(imgPath);
            //     if (imgSize.x > 256 || imgSize.y > 256)
            //     {
            //         UINotifyDialogHelper.Current.ShowPromptDialog("错误", "图片尺寸在范围在256*256内!", "确定");
            //         return;
            //     }

            //     File.Copy(imgPath, Global.HospitalLogoPath, true);
            //     //TextureLoader里有缓存 必须删除缓存才能刷新
            //     TextureLoader.RemoveCache(Global.HospitalLogoPath);
            //     LoadHospitalLogo();
            //     UINotifyDialogHelper.Current.ShowNotifyAutoHide("设置成功!");
            // }

        }
        private void OnDeleteHospitalLogo()
        {
            FileHelper.DeleteFile(Global.HospitalLogoPath);
            //TextureLoader里有缓存 必须删除缓存才能刷新
            TextureLoader.RemoveCache(Global.HospitalLogoPath);
            LoadHospitalLogo();
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("清除成功!");
        }
        private void OnAddAdmin()
        {
            string account = addPanel_accountInputField.text;
            string password1 = addPanel_password1InputField.text;
            string password2 = addPanel_password2InputField.text;
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password1) || string.IsNullOrEmpty(password2))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "必填项不能为空!", "确定");
                return;
            }
            if (account == "admin")
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "不能使用admin作为账号名!", "确定");
                return;
            }
            var adminInfo = DBManagerModule.Instance.QueryAdminInfoTableByName(account);
            if (adminInfo != null)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "账号名已存在,请输入其它账号名!", "确定");
                return;
            }

            if (password1 != password2)
            {

                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "两次输入密码不同，请重输!", "确定");
                return;
            }

            Action yesCallback = () =>
            {

                AdminInfo adminInfo = new AdminInfo
                {
                    Account = account,
                    Password = password1,
                    Name = addPanel_nameInputField.text,
                    Dept = addPanel_deptInputField.text,
                    Telephone = addPanel_teleInputField.text,
                    Role = (Role)addPanel_roleDropdown.value,
                    Sex = (Sex)addPanel_sexDropdown.value,
                    Creator = Global.CurAdminInfo.Account,
                    IsForbidden = false,
                    CreateTime = DateTime.Now
                };
                var result = DBManagerModule.Instance.InsertAdminInfo(adminInfo);
                if (result)
                {
                    adminInfos.Add(adminInfo);
                    adminListView.DataSource.BeginUpdate();
                    adminListView.DataSource.Add(new AdminItem()
                    {
                        Index = adminListView.DataSource.Count,
                        Account = adminInfo.Account,
                        Name = adminInfo.Name,
                        Sex = adminInfo.Sex,
                        Prohibited = adminInfo.IsForbidden,
                    });
                    adminListView.DataSource.EndUpdate();
                    CurAdminInfo = adminInfo;

                    OnCloseAdminAddPanel();
                    SetListViewDefaultSelect();

                    UINotifyDialogHelper.Current.ShowNotifyAutoHide("账号创建成功!");

                }
                else
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "账号创建失败，请检查此账号名是否已经注册!", "确定");
                }

            };
            if ((Role)addPanel_roleDropdown.value == Role.Administrator)
            {
                Main.m_UI.OpenUI<UISuperAdminPasswordDialog>(yesCallback).Forget();
            }
            else
            {
                yesCallback.Invoke();
            }

        }
        private void OnOpenAdminAddPanel()
        {

            addPanel_sexDropdown.value = 0;
            addPanel_roleDropdown.value = 0;
            addPanel_nameInputField.text = "";
            addPanel_deptInputField.text = "";
            addPanel_password1InputField.text = "";
            addPanel_password2InputField.text = "";
            addPanel_accountInputField.text = "";
            addPanel_teleInputField.text = "";
            addPanel_avatarImage.sprite = Global.IconDic["femaleDoctor"];
            doctorAddPanelContent.sprite = Global.IconDic["womenBigK"];

            doctorAddPanel.gameObject.SetActive(true);

        }
        private void SetAdminInfoPanelEditMode(bool editMode)
        {
            infoPanel_accountInputField.interactable = false;

            infoPanel_roleDropdown.interactable = editMode;
            infoPanel_nameInputField.interactable = editMode;
            infoPanel_deptInputField.interactable = editMode;
            infoPanel_password1InputField.interactable = editMode;
            infoPanel_password2InputField.interactable = editMode;
            infoPanel_teleInputField.interactable = editMode;
            infoPanel_sexDropdown.interactable = editMode;

            saveButton.gameObject.SetActive(editMode);
            editButton.gameObject.SetActive(!editMode);
        }

        private void OnCloseAdminAddPanel()
        {
            doctorAddPanel.gameObject.SetActive(false);
        }
        public void OnEditAdmin()
        {

            if (CurAdminInfo == null) return;

            SetAdminInfoPanelEditMode(true);

        }


        public void OnPermitAdmin()
        {
            if (CurAdminInfo == null) return;


            if (CurAdminInfo.Account == Global.CurAdminInfo.Account)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "无法启用自己!", "确定");
                return;
            }
            // if (CurAdminInfo.Account == "admin")
            // {
            //     UINotifyDialogHelper.Current.ShowPromptDialog("错误", "不能禁用超级管理员admin!", "确定");
            //     return;
            // }
            // Action yesCallback = () =>
            // {
            var adminInfo = CurAdminInfo.Clone();
            adminInfo.IsForbidden = false;

            bool result = DBManagerModule.Instance.UpdateAdminInfo(adminInfo);
            if (result)
            {
                var i = adminInfos.FindIndex(e => e.Account == CurAdminInfo.Account);
                if (i >= 0) adminInfos[i] = adminInfo;
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("启用成功!");
                forbidButton.gameObject.SetActive(!adminInfo.IsForbidden);
                permitButton.gameObject.SetActive(adminInfo.IsForbidden);
                if (curIndex >= 0)
                {
                    AdminItemComponent itemComponent = adminListView.GetItemComponent(curIndex) as AdminItemComponent;
                    var adminItem = adminListView.DataSource[curIndex];
                    adminItem.Prohibited = adminInfo.IsForbidden;
                    itemComponent.SetData(adminItem);
                }
            }
            else
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "启用失败!", "确定");
            }
            // };
            // if (CurAdminInfo.Role == Role.Administrator)
            // {
            //     Main.m_UI.OpenUI<UISuperAdminPasswordDialog>(yesCallback).Forget();
            // }
            // else
            // {
            //     yesCallback.Invoke();
            // }

        }
        //禁用管理者
        public void OnForbidAdmin()
        {
            if (CurAdminInfo == null) return;

            if (CurAdminInfo.Account == Global.CurAdminInfo.Account)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "无法禁用自己!", "确定");
                return;
            }
            // if (CurAdminInfo.Account == "admin")
            // {
            //     UINotifyDialogHelper.Current.ShowPromptDialog("错误", "不能禁用超级管理员admin!", "确定");
            //     return;
            // }
            // Action yesCallback = () =>
            // {
            var adminInfo = CurAdminInfo.Clone();
            adminInfo.IsForbidden = true;

            bool result = DBManagerModule.Instance.UpdateAdminInfo(adminInfo);
            if (result)
            {

                var i = adminInfos.FindIndex(e => e.Account == CurAdminInfo.Account);
                if (i >= 0) adminInfos[i] = adminInfo;

                UINotifyDialogHelper.Current.ShowNotifyAutoHide("禁用成功!");
                forbidButton.gameObject.SetActive(!adminInfo.IsForbidden);
                permitButton.gameObject.SetActive(adminInfo.IsForbidden);
                if (curIndex >= 0)
                {
                    AdminItemComponent itemComponent = adminListView.GetItemComponent(curIndex) as AdminItemComponent;
                    var adminItem = adminListView.DataSource[curIndex];
                    adminItem.Prohibited = adminInfo.IsForbidden;
                    itemComponent.SetData(adminItem);
                }
            }
            else
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "禁用失败!", "确定");

            }
            // };

            //     if (CurAdminInfo.Role == Role.Administrator)
            //     {
            //         Main.m_UI.OpenUI<UISuperAdminPasswordDialog>(yesCallback).Forget();
            //     }
            //     else
            //     {
            //         yesCallback.Invoke();
            //     }

        }

        private void OnSaveAdmin()
        {
            if (CurAdminInfo == null) return;

            string account = infoPanel_accountInputField.text;
            string password1 = infoPanel_password1InputField.text;
            string password2 = infoPanel_password2InputField.text;
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password1) || string.IsNullOrEmpty(password2))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "必填项不能为空!", "确定");
                return;
            }

            if (account == "admin")
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "不能使用admin作为账号名!", "确定");
                return;
            }
            if (password1 != password2)
            {

                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "两次输入密码不同，请重输!", "确定");
                return;
            }

            Action yesCallback = () =>
            {
                var adminInfo = CurAdminInfo.Clone();
                adminInfo.Account = account;
                adminInfo.Name = infoPanel_nameInputField.text;
                adminInfo.Dept = infoPanel_deptInputField.text;
                adminInfo.Telephone = infoPanel_teleInputField.text;
                adminInfo.Role = (Role)infoPanel_roleDropdown.value;
                adminInfo.Sex = (Sex)infoPanel_sexDropdown.value;
                adminInfo.Password = password1;

                bool result = DBManagerModule.Instance.UpdateAdminInfo(adminInfo);
                if (result)
                {
                    int index = adminInfos.FindIndex(e => e.Account == CurAdminInfo.Account);
                    if (index >= 0) adminInfos[index] = adminInfo;

                    if (curIndex >= 0)
                    {
                        AdminItemComponent itemComponent = adminListView.GetItemComponent(curIndex) as AdminItemComponent;
                        var adminItem = adminListView.DataSource[curIndex];
                        adminItem.Sex = adminInfo.Sex;
                        adminItem.Account = adminInfo.Account;
                        adminItem.Name = adminInfo.Name;
                        itemComponent.SetData(adminItem);
                    }

                    // if (CurAdminInfo.Account != adminInfo.Account)
                    // {
                    //     DBManagerModule.Instance.UpdatePatientsDoctor(CurAdminInfo.Account, adminInfo.Account);
                    //     if (Global.CurPatientInfo != null) Global.CurPatientInfo.Doctor = adminInfo.Account;
                    // }
                    SetAdminInfoPanelEditMode(false);

                    UINotifyDialogHelper.Current.ShowNotifyAutoHide("保存信息成功!");

                }
                else
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "保存信息失败!", "确定");

                }
            };
            if (CurAdminInfo.Role == Role.Administrator)
            {
                Main.m_UI.OpenUI<UISuperAdminPasswordDialog>(yesCallback).Forget();
            }
            else
            {
                yesCallback.Invoke();
            }


        }


        private void SetListViewDefaultSelect(int selectedIndex = 0)
        {
            var items = adminListView.DataSource;
            // int selectedIndex = listView.SelectedIndex < 0 ? 0 : listView.SelectedIndex;
            if (CurAdminInfo != null)
            {
                int index = items.FindIndex(x => x.Account == CurAdminInfo.Account);
                if (index != -1)
                {
                    adminListView.Select(index);
                    // adminListView.ScrollTo(index);
                }
            }
            else
            {
                //默认选择
                if (items.Count > 0)
                {

                    if (selectedIndex >= items.Count)
                    {
                        selectedIndex = items.Count - 1;
                        adminListView.Select(selectedIndex);
                        // adminListView.ScrollTo(selectedIndex);
                    }
                    else
                    {
                        adminListView.Select(selectedIndex);
                        // adminListView.ScrollTo(selectedIndex);
                        return;
                    }

                }

            }
        }
        private void OnAdminItemSelected(int index, ListViewItem component)
        {
            curIndex = index;
            var item = adminListView.DataSource[index];
            if (item != null)
            {
                var adminInfo = adminInfos.Find(e => e.Account == item.Account);
                if (adminInfo != null)
                {
                    CurAdminInfo = adminInfo;
                    infoPanel_accountInputField.text = CurAdminInfo.Account;
                    infoPanel_nameInputField.text = CurAdminInfo.Name;
                    infoPanel_deptInputField.text = CurAdminInfo.Dept;
                    infoPanel_sexDropdown.value = (int)CurAdminInfo.Sex;
                    infoPanel_roleDropdown.value = (int)CurAdminInfo.Role;
                    infoPanel_teleInputField.text = CurAdminInfo.Telephone;
                    infoPanel_password1InputField.text = CurAdminInfo.Password;
                    infoPanel_password2InputField.text = CurAdminInfo.Password;
                    infoPanel_avatarImage.sprite = CurAdminInfo.Sex == Sex.Female ? Global.IconDic["femaleDoctor"] : Global.IconDic["maleDoctor"];
                    forbidButton.gameObject.SetActive(!CurAdminInfo.IsForbidden);
                    permitButton.gameObject.SetActive(CurAdminInfo.IsForbidden);
                    SetAdminInfoPanelEditMode(false);
                }
            }
        }


        /// <summary>
        /// 置顶UI
        /// </summary>
        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
            var mainWindow = Main.m_UI.GetOpenedUI<UIMain>();
            if (mainWindow != null)
            {
                UIEntity.transform.SetParent(mainWindow.Content);
                UIEntity.transform.localScale = Vector3.one;
                UIEntity.transform.localPosition = Vector3.zero;
            }
        }


        /// <summary>
        /// 打开UI
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            buttonToggleGroup.SetAllButtonsOff(false);
            generalButton.IsClick = true;
            buttonToggleGroup.gameObject.SetActive(false);
            hospitalButton.interactable = Global.CurAdminInfo.Role == Role.Administrator;
            doctorButton.interactable = Global.CurAdminInfo.Role == Role.Administrator;
            hospitalLockImage.gameObject.SetActive(Global.CurAdminInfo.Role != Role.Administrator);
            doctorLockImage.gameObject.SetActive(Global.CurAdminInfo.Role != Role.Administrator);
            buttonToggleGroup.gameObject.SetActive(true);
        }



        /// <summary>
        /// 关闭UI
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();
            versionTimer?.Pause();
            adminListView.SelectedIndex = -1;
            curIndex = -1;
            SaveConfigModule.Instance.SetUserData("musicVolume", musicSlider.value);
            SaveConfigModule.Instance.SetUserData("soundVolume", soundSlider.value);
            SaveConfigModule.Instance.SaveUserData();
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            addPanel_sexDropdown.onValueChanged.RemoveAllListeners();
            infoPanel_sexDropdown.onValueChanged.RemoveAllListeners();

            openAddPanelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            addButton.onClick.RemoveAllListeners();

            saveButton.onClick.RemoveAllListeners();
            forbidButton.onClick.RemoveAllListeners();
            permitButton.onClick.RemoveAllListeners();
            editButton.onClick.RemoveAllListeners();

            adminListView.OnSelect.RemoveAllListeners();
            generalButton.onClick.RemoveAllListeners();
            doctorButton.onClick.RemoveAllListeners();

            hospitalButton.onClick.RemoveAllListeners();

            saveHospitalButton.onClick.RemoveAllListeners();
            selectLogoButton.onClick.RemoveAllListeners();
            deleteLogoButton.onClick.RemoveAllListeners();


        }

        /// <summary>
        /// UI逻辑刷新
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

        }



        private const string Pattern = "(\\d+\\.\\d{3})\\.([A-Za-z]{2,3}\\d{2})?\\d{8}-([A-Za-z]+)";
        readonly static Regex regex = new Regex(Pattern);
        //比对Application.version SerialPortModule.Instance.DeviceVersion每一位数字
        private int CompareVersion(string version1, string version2)
        {
            char[] version1Array = version1.ToCharArray();
            char[] version2Array = version2.ToCharArray();
            int index = 0;
            //比对前两位
            while (index < 3)//(index<version1Array.Length && index < version2Array.Length)
            {
                // int version1Int = int.Parse(version1Array[index].ToString());
                // int version2Int = int.Parse(version2Array[index].ToString());
                if (version1Array[index] > version2Array[index])
                {
                    return 1;
                }
                else if (version1Array[index] < version2Array[index])
                {
                    return -1;
                }
                else
                {
                    index++;
                }
            }
            return 0;
            // if (version1Array.Length > version2Array.Length)
            // {
            //     return 1;
            // }
            // else if (version1Array.Length < version2Array.Length)
            // {
            //     return -1;
            // }
            // else
            // {
            //     return 0;
            // }
        }

        private void ComparativeVersion()
        {
            if (string.IsNullOrEmpty(SerialPortModule.Instance.DeviceVersion))
            {
                SerialPortModule.Instance.CheckDeviceVer();
            }
            else if (!hasChecked)
            {

                Match match = regex.Match(SerialPortModule.Instance.DeviceVersion);
                if (match.Success)
                {
#if UNITY_EDITOR
                    Log.Info(match.Groups[1].Value);//版本号
                    Log.Info(match.Groups[2].Value);//补丁/定制版本（可无）
                    Log.Info(match.Groups[3].Value);//版本类型
#endif
                    if (versionType != match.Groups[3].Value)//版本类型一致
                    {
                        deviceText.text = string.Format("硬件版本：V{0} \n<color=red>{1}版本类型不兼容</color>", match.Groups[1].Value, match.Groups[3].Value);
                        hasChecked = true;
                        return;
                    }

                    var checkID = CompareVersion(Application.version, match.Groups[1].Value);
                    if (checkID == 0)
                    {
                        deviceText.text = string.Format("硬件版本：V{0}", match.Groups[1].Value);
                    }
                    else
                    {
                        if (checkID > 0)
                        {
                            deviceText.text = string.Format("硬件版本：<color=red>V{0} \n(硬件版本低于软件版本)", match.Groups[1].Value);

                        }
                        else
                        {
                            deviceText.text = string.Format("硬件版本：<color=red>V{0} \n(硬件版本高于软件版本)", match.Groups[1].Value);
                        }
                    }
                    Log.Info("设备版本：" + match.Groups[1].Value);
                    hasChecked = true;
                }
            }
        }

    }
}
