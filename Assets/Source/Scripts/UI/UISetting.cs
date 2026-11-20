using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
    /// UI-设置（由 WebUI2UnityUI 转换的 UISettingPanel 驱动）
    /// </summary>
    [UIResource("UISettingPanel")]
    public class UISetting : UILogicResident
    {
        [InjectPath("MenuPanel/GeneralButton")] private Button generalButton;
        [InjectPath("MenuPanel/HospitalButton")] private Button hospitalButton;
        [InjectPath("MenuPanel/DoctorButton")] private Button doctorButton;
        [InjectPath("MenuPanel/HospitalButton/LockImage")] private Image hospitalLockImage;
        [InjectPath("MenuPanel/DoctorButton/LockImage")] private Image doctorLockImage;

        [InjectPath("WorkPanel/GeneralPanel")] private GameObject generalPanel;
        [InjectPath("WorkPanel/GeneralPanel/MusicSlider")] private Slider musicSlider;
        [InjectPath("WorkPanel/GeneralPanel/MusicVolumeText")] private TMP_Text musicVolumeText;
        [InjectPath("WorkPanel/GeneralPanel/SoundSlider")] private Slider soundSlider;
        [InjectPath("WorkPanel/GeneralPanel/SoundVolumeText")] private TMP_Text soundVolumeText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/VersionText")] private TMP_Text versionText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/ReleaseVersionText")] private TMP_Text releaseVersionText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/DeviceText")] private TMP_Text deviceText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/DateText")] private TMP_Text releaseDateText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/TelephoneText")] private TMP_Text telephoneText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/AddressText")] private TMP_Text addressText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/WebsiteText")] private TMP_Text websiteText;
        [InjectPath("WorkPanel/GeneralPanel/VersionCard/HelpButton")] private Button helpButton;

        [InjectPath("WorkPanel/HospitalPanel")] private GameObject hospitalPanel;
        [InjectPath("WorkPanel/HospitalPanel/HospitalNameInputField")] private TMP_InputField hospitalNameInput;
        [InjectPath("WorkPanel/HospitalPanel/HospitalCodeInputField")] private TMP_InputField hospitalCodeInput;
        [InjectPath("WorkPanel/HospitalPanel/HospitalDeptInputField")] private TMP_InputField hospitalDeptInput;
        [InjectPath("WorkPanel/HospitalPanel/HospitalTeleInputField")] private TMP_InputField hospitalTeleInput;
        [InjectPath("WorkPanel/HospitalPanel/HospitalAddressInputField")] private TMP_InputField hospitalAddressInput;
        [InjectPath("WorkPanel/HospitalPanel/SaveHospitalButton")] private Button saveHospitalButton;
        [InjectPath("WorkPanel/HospitalPanel/HospitalLogo/SelectLogoButton")] private Button selectLogoButton;
        [InjectPath("WorkPanel/HospitalPanel/HospitalLogo/DeleteLogoButton")] private Button deleteLogoButton;
        [InjectPath("WorkPanel/HospitalPanel/HospitalLogo/Logo")] private RawImage logoImage;
        [InjectPath("WorkPanel/HospitalPanel/HospitalLogo/Logo/Spare")] private RawImage spareImage;
        [InjectPath("WorkPanel/HospitalPanel/LastModifyTime")] private TMP_Text lastModifyTime;

        [InjectPath("WorkPanel/DoctorPanel")] private GameObject doctorPanel;
        [InjectPath("WorkPanel/DoctorPanel/OpenAddPanelButton")] private Button openAddPanelButton;
        [InjectPath("WorkPanel/DoctorPanel/DoctorRowsContent")] private RectTransform doctorRowsContent;
        [InjectPath("WorkPanel/DoctorPanel/DoctorRowsContent/DoctorRowTemplate")] private GameObject doctorRowTemplate;

        [InjectPath("WorkPanel/DoctorPanel/DoctorPager")] private RectTransform doctorPager;
        [InjectPath("WorkPanel/DoctorPanel/DoctorPager/PrevPageButton")] private Button prevPageButton;
        [InjectPath("WorkPanel/DoctorPanel/DoctorPager/NextPageButton")] private Button nextPageButton;
        [InjectPath("WorkPanel/DoctorPanel/DoctorPager/PageNumberTemplate")] private GameObject pageNumberTemplate;
        [InjectPath("WorkPanel/DoctorPanel/DoctorPager/PageEllipsisTemplate")] private GameObject pageEllipsisTemplate;

        [InjectPath("DoctorEditPanel")] private GameObject doctorEditPanel;
        [InjectPath("DoctorEditPanel/EditCloseButton")] private Button editCloseButton;
        [InjectPath("DoctorEditPanel/CancelEditDoctorButton")] private Button cancelEditDoctorButton;
        [InjectPath("DoctorEditPanel/InfoAccountInputField")] private TMP_InputField infoAccountInput;
        [InjectPath("DoctorEditPanel/InfoNameInputField")] private TMP_InputField infoNameInput;
        [InjectPath("DoctorEditPanel/InfoDeptInputField")] private TMP_InputField infoDeptInput;
        [InjectPath("DoctorEditPanel/InfoTeleInputField")] private TMP_InputField infoTeleInput;
        [InjectPath("DoctorEditPanel/InfoRoleDropdown")] private TMP_Dropdown infoRoleDropdown;
        [InjectPath("DoctorEditPanel/InfoSexDropdown")] private TMP_Dropdown infoSexDropdown;
        [InjectPath("DoctorEditPanel/InfoPassword1InputField")] private TMP_InputField infoPassword1Input;
        [InjectPath("DoctorEditPanel/InfoPassword2InputField")] private TMP_InputField infoPassword2Input;
        [InjectPath("DoctorEditPanel/InfoAvatarImage")] private Image infoAvatarImage;
        [InjectPath("DoctorEditPanel/SaveDoctorButton")] private Button saveDoctorButton;
        [InjectPath("DoctorEditPanel/ForbidButton")] private Button forbidButton;
        [InjectPath("DoctorEditPanel/PermitButton")] private Button permitButton;

        [InjectPath("DoctorAddPanel")] private GameObject doctorAddPanel;
        [InjectPath("DoctorAddPanel/AddCloseButton")] private Button addCloseButton;
        [InjectPath("DoctorAddPanel/AddNameInputField")] private TMP_InputField addNameInput;
        [InjectPath("DoctorAddPanel/AddAccountInputField")] private TMP_InputField addAccountInput;
        [InjectPath("DoctorAddPanel/AddDeptInputField")] private TMP_InputField addDeptInput;
        [InjectPath("DoctorAddPanel/AddTeleInputField")] private TMP_InputField addTeleInput;
        [InjectPath("DoctorAddPanel/AddRoleDropdown")] private TMP_Dropdown addRoleDropdown;
        [InjectPath("DoctorAddPanel/AddSexDropdown")] private TMP_Dropdown addSexDropdown;
        [InjectPath("DoctorAddPanel/AddPassword1InputField")] private TMP_InputField addPassword1Input;
        [InjectPath("DoctorAddPanel/AddPassword2InputField")] private TMP_InputField addPassword2Input;
        [InjectPath("DoctorAddPanel/AddAvatarImage")] private Image addAvatarImage;
        [InjectPath("DoctorAddPanel/CancelAddDoctorButton")] private Button cancelAddDoctorButton;
        [InjectPath("DoctorAddPanel/ConfirmAddDoctorButton")] private Button confirmAddDoctorButton;

        private List<AdminInfo> adminInfos;
        private AdminInfo curAdmin;
        private GameObject selectedDoctorRow;
        private int curPage;
        private const int PageSize = 7;
        private string versionType = "";
        private UnityTimer.Timer versionTimer;
        private bool hasChecked;

        private const string Pattern = "(\\d+\\.\\d{3})\\.([A-Za-z]{2,3}\\d{2})?\\d{8}-([A-Za-z]+)";
        private static readonly Regex VersionRegex = new Regex(Pattern);

        public override void OnInit()
        {
            base.OnInit();

            var roleOptions = EnumHelper.GetDescriptions(typeof(Role));
            var sexOptions = EnumHelper.GetDescriptions(typeof(Sex));
            addRoleDropdown.ClearOptions();
            addRoleDropdown.AddOptions(roleOptions);
            infoRoleDropdown.ClearOptions();
            infoRoleDropdown.AddOptions(roleOptions);
            addSexDropdown.ClearOptions();
            addSexDropdown.AddOptions(sexOptions);
            infoSexDropdown.ClearOptions();
            infoSexDropdown.AddOptions(sexOptions);

            generalButton.onClick.AddListener(OnOpenGeneralPanel);
            hospitalButton.onClick.AddListener(OnOpenHospitalPanel);
            doctorButton.onClick.AddListener(OnOpenDoctorPanel);

            helpButton.onClick.AddListener(OnOpenHelp);
            saveHospitalButton.onClick.AddListener(OnSaveHospitalInfo);
            selectLogoButton.onClick.AddListener(OnSelectHospitalLogo);
            deleteLogoButton.onClick.AddListener(OnDeleteHospitalLogo);

            openAddPanelButton.onClick.AddListener(OnOpenAdminAddPanel);
            prevPageButton.onClick.AddListener(() => TurnPage(-1));
            nextPageButton.onClick.AddListener(() => TurnPage(1));
            addCloseButton.onClick.AddListener(OnCloseAdminAddPanel);
            cancelAddDoctorButton.onClick.AddListener(OnCloseAdminAddPanel);
            confirmAddDoctorButton.onClick.AddListener(OnAddAdmin);
            editCloseButton.onClick.AddListener(OnCloseAdminEditPanel);
            cancelEditDoctorButton.onClick.AddListener(OnCloseAdminEditPanel);
            saveDoctorButton.onClick.AddListener(OnSaveAdmin);
            forbidButton.onClick.AddListener(OnForbidAdmin);
            permitButton.onClick.AddListener(OnPermitAdmin);

            InitGeneralSettingPanel();
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);

            bool isAdmin = Global.CurAdminInfo != null && Global.CurAdminInfo.Role == Role.Administrator;
            hospitalButton.interactable = isAdmin;
            doctorButton.interactable = isAdmin;
            hospitalLockImage.gameObject.SetActive(!isAdmin);
            doctorLockImage.gameObject.SetActive(!isAdmin);

            OnOpenGeneralPanel();
        }

        private void InitGeneralSettingPanel()
        {
            versionType = VersionManager.VersionType;
            versionText.text = "软件完整版本：" + Application.version +
                (versionType == "Release" ? "" : " - " + versionType);
            releaseVersionText.text = "软件发布版本：" + (Application.version.Contains(".") ? Application.version.Split('.')[0] : Application.version);
            releaseDateText.text = "软件发布日期：" + VersionManager.BuildDate;
            telephoneText.text = "联系电话：177 1241 0781 / 025-52313235";
            addressText.text = "地址：南京高淳经济开发区双高路86号医疗器械产业园24号";
            websiteText.text = "网站：www.jslvbai.com";

            musicSlider.minValue = 0f;
            musicSlider.maxValue = 100f;
            soundSlider.minValue = 0f;
            soundSlider.maxValue = 100f;
            musicSlider.value = Main.m_Audio.BackgroundVolume * 100f;
            soundSlider.value = Main.m_Audio.SingleVolume * 100f;
            musicVolumeText.text = Mathf.RoundToInt(musicSlider.value).ToString();
            soundVolumeText.text = Mathf.RoundToInt(soundSlider.value).ToString();

            musicSlider.onValueChanged.AddListener(value =>
            {
                musicVolumeText.text = Mathf.RoundToInt(value).ToString();
                Main.m_Audio.BackgroundVolume = value / 100f;
            });
            soundSlider.onValueChanged.AddListener(value =>
            {
                soundVolumeText.text = Mathf.RoundToInt(value).ToString();
                Main.m_Audio.OneShootVolume = Main.m_Audio.SingleVolume = Main.m_Audio.MultipleVolume = value / 100f;
            });

            versionTimer = UnityTimer.Timer.Register(0.5f, ComparativeVersion, null, true);
            versionTimer.Pause();
        }

        private void SetTabButtonSelected(Button button, bool selected)
        {
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = selected ? new Color(0.937f, 0.965f, 1f, 1f) : new Color(0f, 0f, 0f, 0f);
            Color textColor = selected ? new Color(0.078f, 0.337f, 0.941f, 1f) : new Color(0.271f, 0.318f, 0.369f, 1f);
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>())
            {
                text.color = textColor;
            }
        }

        private void OnOpenGeneralPanel()
        {
            generalPanel.SetActive(true);
            hospitalPanel.SetActive(false);
            doctorPanel.SetActive(false);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            SetTabButtonSelected(generalButton, true);
            SetTabButtonSelected(hospitalButton, false);
            SetTabButtonSelected(doctorButton, false);
            versionTimer?.Resume();
        }

        private void OnOpenHospitalPanel()
        {
            if (!hospitalButton.interactable) return;
            generalPanel.SetActive(false);
            hospitalPanel.SetActive(true);
            doctorPanel.SetActive(false);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            SetTabButtonSelected(generalButton, false);
            SetTabButtonSelected(hospitalButton, true);
            SetTabButtonSelected(doctorButton, false);
            versionTimer?.Pause();

            if (Global.CurHospitalInfo != null)
            {
                hospitalNameInput.text = Global.CurHospitalInfo.Name ?? "";
                hospitalCodeInput.text = Global.CurHospitalInfo.Code ?? "";
                hospitalDeptInput.text = Global.CurHospitalInfo.Dept ?? "";
                hospitalTeleInput.text = Global.CurHospitalInfo.Telephone ?? "";
                hospitalAddressInput.text = Global.CurHospitalInfo.Address ?? "";
                lastModifyTime.text = "最后一次更新日期：" + Global.CurHospitalInfo.ModifyTime.ToString("yyyy-MM-dd");
            }
            else
            {
                hospitalNameInput.text = "";
                hospitalCodeInput.text = "";
                hospitalDeptInput.text = "";
                hospitalTeleInput.text = "";
                hospitalAddressInput.text = "";
                lastModifyTime.text = "";
            }
            LoadHospitalLogo();
        }

        private void OnOpenDoctorPanel()
        {
            if (!doctorButton.interactable) return;
            generalPanel.SetActive(false);
            hospitalPanel.SetActive(false);
            doctorPanel.SetActive(true);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            SetTabButtonSelected(generalButton, false);
            SetTabButtonSelected(hospitalButton, false);
            SetTabButtonSelected(doctorButton, true);
            versionTimer?.Pause();

            adminInfos = DBManagerModule.Instance.QueryAllAdminInfos() ?? new List<AdminInfo>();
            if (curAdmin != null && adminInfos.FindIndex(a => a.Account == curAdmin.Account) < 0) curAdmin = null;
            RefreshDoctorRows(true);
        }

        private void OnOpenHelp()
        {
            string helpName = ConfigDataManager.Instance.IsCompensation ? "help_c.pdf" : "help.pdf";
            Application.OpenURL(Application.streamingAssetsPath + "/" + helpName);
        }

        private void RefreshDoctorRows(bool followSelection = false)
        {
            for (int i = doctorRowsContent.childCount - 1; i >= 0; i--)
            {
                Transform child = doctorRowsContent.GetChild(i);
                if (child == doctorRowTemplate.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            selectedDoctorRow = null;

            int pageCount = Mathf.Max(1, Mathf.CeilToInt(adminInfos.Count / (float)PageSize));
            int keepIndex = curAdmin != null ? adminInfos.FindIndex(a => a.Account == curAdmin.Account) : -1;
            if (keepIndex >= 0 && followSelection) curPage = keepIndex / PageSize;
            curPage = Mathf.Clamp(curPage, 0, pageCount - 1);

            int start = curPage * PageSize;
            int sliceCount = Mathf.Min(PageSize, adminInfos.Count - start);
            for (int i = 0; i < sliceCount; i++)
            {
                AdminInfo admin = adminInfos[start + i];
                GameObject row = UnityEngine.Object.Instantiate(doctorRowTemplate, doctorRowsContent);
                row.name = "DoctorRow_" + i;
                row.SetActive(true);

                RectTransform templateRect = doctorRowTemplate.GetComponent<RectTransform>();
                RectTransform rowRect = row.GetComponent<RectTransform>();
                if (templateRect != null && rowRect != null)
                {
                    Vector2 basePosition = templateRect.anchoredPosition;
                    rowRect.anchoredPosition = new Vector2(basePosition.x, basePosition.y - i * 48f);
                }

                SetChildText(row, "RowAccountText", admin.Account);
                SetChildText(row, "RowRoleText", admin.Role == Role.Administrator ? "管理员" : "操作员");
                SetChildText(row, "RowNameText", admin.Name);
                SetChildText(row, "RowDeptText", admin.Dept);
                SetChildText(row, "RowSexText", admin.Sex == Sex.Female ? "女" : "男");
                SetChildText(row, "RowCreateTimeText", admin.CreateTime == default(DateTime) ? "-" : admin.CreateTime.ToString("yyyy-MM-dd"));
                SetChildText(row, "RowPasswordText", "******** (已加密)");

                AdminInfo captured = admin;
                Button rowButton = row.GetComponent<Button>();
                if (rowButton == null) rowButton = row.AddComponent<Button>();
                rowButton.transition = Selectable.Transition.None;
                rowButton.onClick.RemoveAllListeners();
                rowButton.onClick.AddListener(() => SelectDoctor(captured, row));

                Transform buttonNode = row.transform.Find("SelectDoctorButton");
                if (buttonNode != null)
                {
                    Button button = buttonNode.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() =>
                    {
                        SelectDoctor(captured, row);
                        OnOpenAdminEditPanel();
                    });
                }
            }

            if (sliceCount > 0)
            {
                int keepInSlice = keepIndex - start;
                if (keepIndex >= 0 && keepInSlice >= 0 && keepInSlice < sliceCount)
                    SelectDoctor(adminInfos[keepIndex], FindRowByIndex(keepInSlice));
                else if (keepIndex < 0)
                    SelectDoctor(adminInfos[start], FindRowByIndex(0));
            }
            else curAdmin = null;

            UIPagerBar.Refresh(doctorPager, prevPageButton, nextPageButton, pageNumberTemplate, pageEllipsisTemplate,
                pageCount, curPage, OnPageSelected);
        }

        private void TurnPage(int delta)
        {
            curPage += delta;
            RefreshDoctorRows();
        }

        private void OnPageSelected(int page)
        {
            curPage = page;
            RefreshDoctorRows();
        }

        private GameObject FindRowByIndex(int index)
        {
            return doctorRowsContent.Find("DoctorRow_" + index) != null
                ? doctorRowsContent.Find("DoctorRow_" + index).gameObject
                : null;
        }

        private static void SetChildText(GameObject parent, string childName, string value)
        {
            Transform node = parent.transform.Find(childName);
            if (node == null) return;
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text != null) text.text = value ?? "";
        }

        private void HighlightDoctorRow(GameObject row)
        {
            if (selectedDoctorRow != null)
            {
                Image oldImage = selectedDoctorRow.GetComponent<Image>();
                if (oldImage != null) oldImage.color = Color.white;
            }
            selectedDoctorRow = row;
            Image image = row != null ? row.GetComponent<Image>() : null;
            if (image != null) image.color = new Color(0.937f, 0.965f, 1f, 1f);
        }

        private void SelectDoctor(AdminInfo admin, GameObject row)
        {
            if (admin == null) return;
            curAdmin = admin;
            HighlightDoctorRow(row);

            infoAccountInput.text = admin.Account ?? "";
            infoNameInput.text = admin.Name ?? "";
            infoDeptInput.text = admin.Dept ?? "";
            infoTeleInput.text = admin.Telephone ?? "";
            infoRoleDropdown.value = (int)admin.Role;
            infoSexDropdown.value = (int)admin.Sex;
            infoPassword1Input.text = admin.Password ?? "";
            infoPassword2Input.text = admin.Password ?? "";
            infoAvatarImage.sprite = admin.Sex == Sex.Female ? Global.IconDic["femaleDoctor"] : Global.IconDic["maleDoctor"];

            forbidButton.gameObject.SetActive(!admin.IsForbidden);
            permitButton.gameObject.SetActive(admin.IsForbidden);
            SetAdminEditMode(doctorEditPanel.activeSelf);
        }

        private void SetAdminEditMode(bool editMode)
        {
            infoAccountInput.interactable = false;
            infoNameInput.interactable = editMode;
            infoDeptInput.interactable = editMode;
            infoTeleInput.interactable = editMode;
            infoRoleDropdown.interactable = editMode;
            infoSexDropdown.interactable = editMode;
            infoPassword1Input.interactable = editMode;
            infoPassword2Input.interactable = editMode;
        }

        private void OnOpenAdminEditPanel()
        {
            if (curAdmin == null) return;
            SetAdminEditMode(true);
            ShowModalOnMain(doctorEditPanel);
        }

        private void OnCloseAdminEditPanel()
        {
            HideModalOnMain(doctorEditPanel);
        }

        private void ShowModalOnMain(GameObject modal)
        {
            var main = Main.m_UI.GetOpenedUI<UIMain>();
            if (main != null) main.ShowModal(modal);
            else if (modal != null) modal.SetActive(true);
        }

        private void HideModalOnMain(GameObject modal)
        {
            var main = Main.m_UI.GetOpenedUI<UIMain>();
            if (main != null) main.HideModal(modal);
            else if (modal != null) modal.SetActive(false);
        }

        private void OnOpenAdminAddPanel()
        {
            addNameInput.text = "";
            addAccountInput.text = "";
            addDeptInput.text = "";
            addTeleInput.text = "";
            addPassword1Input.text = "";
            addPassword2Input.text = "";
            addRoleDropdown.value = 0;
            addSexDropdown.value = 0;
            addAvatarImage.sprite = Global.IconDic["femaleDoctor"];
            ShowModalOnMain(doctorAddPanel);
        }

        private void OnCloseAdminAddPanel()
        {
            HideModalOnMain(doctorAddPanel);
        }

        private void OnAddAdmin()
        {
            string account = addAccountInput.text;
            string password1 = addPassword1Input.text;
            string password2 = addPassword2Input.text;

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
            if (DBManagerModule.Instance.QueryAdminInfoTableByName(account) != null)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "账号名已存在，请输入其它账号名!", "确定");
                return;
            }
            if (password1 != password2)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "两次输入密码不同，请重输!", "确定");
                return;
            }

            Action yesCallback = () =>
            {
                var admin = new AdminInfo
                {
                    Account = account,
                    Password = password1,
                    Name = addNameInput.text,
                    Dept = addDeptInput.text,
                    Telephone = addTeleInput.text,
                    Role = (Role)addRoleDropdown.value,
                    Sex = (Sex)addSexDropdown.value,
                    Creator = Global.CurAdminInfo.Account,
                    IsForbidden = false,
                    CreateTime = DateTime.Now
                };

                if (!DBManagerModule.Instance.InsertAdminInfo(admin))
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "账号创建失败，请检查此账号名是否已经注册!", "确定");
                    return;
                }

                adminInfos.Add(admin);
                curAdmin = admin;
                OnCloseAdminAddPanel();
                RefreshDoctorRows(true);
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("账号创建成功!");
            };

            if ((Role)addRoleDropdown.value == Role.Administrator)
                Main.m_UI.OpenUI<UISuperAdminPasswordDialog>(yesCallback).Forget();
            else
                yesCallback.Invoke();
        }

        private void OnSaveAdmin()
        {
            if (curAdmin == null) return;
            string account = infoAccountInput.text;
            string password1 = infoPassword1Input.text;
            string password2 = infoPassword2Input.text;

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password1) || string.IsNullOrEmpty(password2))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "必填项不能为空!", "确定");
                return;
            }
            if (password1 != password2)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "两次输入密码不同，请重输!", "确定");
                return;
            }

            Action yesCallback = () =>
            {
                string oldAccount = curAdmin.Account;
                AdminInfo updated = curAdmin.Clone();
                updated.Account = account;
                updated.Name = infoNameInput.text;
                updated.Dept = infoDeptInput.text;
                updated.Telephone = infoTeleInput.text;
                updated.Role = (Role)infoRoleDropdown.value;
                updated.Sex = (Sex)infoSexDropdown.value;
                updated.Password = password1;

                if (!DBManagerModule.Instance.UpdateAdminInfo(updated, oldAccount))
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "保存信息失败!", "确定");
                    return;
                }

                int index = adminInfos.FindIndex(a => a.Account == oldAccount);
                if (index >= 0) adminInfos[index] = updated;
                curAdmin = updated;
                RefreshDoctorRows(true);
                OnCloseAdminEditPanel();
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("保存信息成功!");
            };

            if (curAdmin.Role == Role.Administrator)
                Main.m_UI.OpenUI<UISuperAdminPasswordDialog>(yesCallback).Forget();
            else
                yesCallback.Invoke();
        }

        private void OnForbidAdmin()
        {
            SetAdminForbidden(true);
        }

        private void OnPermitAdmin()
        {
            SetAdminForbidden(false);
        }

        private void SetAdminForbidden(bool forbidden)
        {
            if (curAdmin == null) return;
            if (curAdmin.Account == Global.CurAdminInfo.Account)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", forbidden ? "无法禁用自己!" : "无法启用自己!", "确定");
                return;
            }

            AdminInfo updated = curAdmin.Clone();
            updated.IsForbidden = forbidden;
            if (!DBManagerModule.Instance.UpdateAdminInfo(updated))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", forbidden ? "禁用失败!" : "启用失败!", "确定");
                return;
            }

            int index = adminInfos.FindIndex(a => a.Account == updated.Account);
            if (index >= 0) adminInfos[index] = updated;
            curAdmin = updated;
            RefreshDoctorRows(true);
            UINotifyDialogHelper.Current.ShowNotifyAutoHide(forbidden ? "禁用成功!" : "启用成功!");
        }

        private void OnSaveHospitalInfo()
        {
            bool result;
            HospitalInfo info;
            if (Global.CurHospitalInfo != null)
            {
                info = Global.CurHospitalInfo.Clone();
                info.Name = hospitalNameInput.text;
                info.Code = hospitalCodeInput.text;
                info.Dept = hospitalDeptInput.text;
                info.Telephone = hospitalTeleInput.text;
                info.Address = hospitalAddressInput.text;
                info.ModifyTime = DateTime.Now;
                result = DBManagerModule.Instance.UpdateHospitalInfo(info);
            }
            else
            {
                info = new HospitalInfo
                {
                    Name = hospitalNameInput.text,
                    Code = hospitalCodeInput.text,
                    Dept = hospitalDeptInput.text,
                    Telephone = hospitalTeleInput.text,
                    Address = hospitalAddressInput.text,
                    ModifyTime = DateTime.Now,
                    CreateTime = DateTime.Now
                };
                result = DBManagerModule.Instance.InsertHospitalInfo(info);
            }

            if (!result)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "写入数据库错误，保存失败!", "确定");
                return;
            }

            Global.CurHospitalInfo = info.Clone();
            lastModifyTime.text = "最后一次更新日期：" + info.ModifyTime.ToString("yyyy-MM-dd");
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("保存成功!");
        }

        private void LoadHospitalLogo()
        {
            if (FileHelper.IsFileExists(Global.HospitalLogoPath))
            {
                TextureLoader.Load(Global.HospitalLogoPath, result =>
                {
                    if (logoImage != null) logoImage.texture = result.Texture;
                    // Spare 是 Logo 的子节点，会盖在 Logo 上；自定义 Logo 加载出来后要藏掉占位图
                    if (spareImage != null) spareImage.gameObject.SetActive(false);
                });
            }
            else
            {
                if (logoImage != null) logoImage.texture = spareImage != null ? spareImage.texture : null;
                // 没有自定义 Logo 时再显示占位图
                if (spareImage != null) spareImage.gameObject.SetActive(true);
            }
        }

        private void OnSelectHospitalLogo()
        {
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile(path =>
            {
                if (string.IsNullOrEmpty(path)) return;
                var size = CommonHelper.GetImageSize(path);
                if (size.x > 256 || size.y > 256)
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "图片尺寸在范围在256*256内!", "确定");
                    return;
                }

                File.Copy(path, Global.HospitalLogoPath, true);
                TextureLoader.RemoveCache(Global.HospitalLogoPath);
                LoadHospitalLogo();
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("设置成功!");
            }, new[] { NativeFilePicker.ConvertExtensionToFileType("png") });

            if (permission != NativeFilePicker.Permission.Granted)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "导出文件需要写入权限，请在Android系统设置里允许应用写入权限!", "确定", null, index =>
                {
                    NativeFilePicker.OpenSettings();
                    return true;
                });
            }
        }

        private void OnDeleteHospitalLogo()
        {
            FileHelper.DeleteFile(Global.HospitalLogoPath);
            TextureLoader.RemoveCache(Global.HospitalLogoPath);
            LoadHospitalLogo();
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("清除成功!");
        }

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

        public override void OnClose()
        {
            base.OnClose();
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            versionTimer?.Pause();
            SaveConfigModule.Instance.SetUserData("musicVolume", musicSlider.value);
            SaveConfigModule.Instance.SetUserData("soundVolume", soundSlider.value);
            SaveConfigModule.Instance.SaveUserData();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            generalButton.onClick.RemoveAllListeners();
            hospitalButton.onClick.RemoveAllListeners();
            doctorButton.onClick.RemoveAllListeners();
            helpButton.onClick.RemoveAllListeners();
            saveHospitalButton.onClick.RemoveAllListeners();
            selectLogoButton.onClick.RemoveAllListeners();
            deleteLogoButton.onClick.RemoveAllListeners();
            openAddPanelButton.onClick.RemoveAllListeners();
            prevPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.RemoveAllListeners();
            addCloseButton.onClick.RemoveAllListeners();
            cancelAddDoctorButton.onClick.RemoveAllListeners();
            confirmAddDoctorButton.onClick.RemoveAllListeners();
            editCloseButton.onClick.RemoveAllListeners();
            cancelEditDoctorButton.onClick.RemoveAllListeners();
            saveDoctorButton.onClick.RemoveAllListeners();
            forbidButton.onClick.RemoveAllListeners();
            permitButton.onClick.RemoveAllListeners();
            musicSlider.onValueChanged.RemoveAllListeners();
            soundSlider.onValueChanged.RemoveAllListeners();
        }

        private int CompareVersion(string version1, string version2)
        {
            char[] a = version1.ToCharArray();
            char[] b = version2.ToCharArray();
            int index = 0;
            while (index < 3)
            {
                if (a[index] > b[index]) return 1;
                if (a[index] < b[index]) return -1;
                index++;
            }
            return 0;
        }

        private void ComparativeVersion()
        {
            if (string.IsNullOrEmpty(SerialPortModule.Instance.DeviceVersion))
            {
                SerialPortModule.Instance.CheckDeviceVer();
                return;
            }
            if (hasChecked) return;

            Match match = VersionRegex.Match(SerialPortModule.Instance.DeviceVersion);
            if (!match.Success) return;

            if (versionType != match.Groups[3].Value)
            {
                deviceText.text = string.Format("硬件版本：V{0} \n<color=red>{1}版本类型不兼容</color>", match.Groups[1].Value, match.Groups[3].Value);
                hasChecked = true;
                return;
            }

            int result = CompareVersion(Application.version, match.Groups[1].Value);
            if (result == 0)
                deviceText.text = "硬件版本：V" + match.Groups[1].Value;
            else if (result > 0)
                deviceText.text = string.Format("硬件版本：<color=red>V{0} \n(硬件版本低于软件版本)</color>", match.Groups[1].Value);
            else
                deviceText.text = string.Format("硬件版本：<color=red>V{0} \n(硬件版本高于软件版本)</color>", match.Groups[1].Value);

            hasChecked = true;
        }
    }
}
