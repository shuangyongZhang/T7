using System;
using System.IO;
using System.Collections.Generic;
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
        [InjectPath("MenuPanel/TreatmentPlanButton")] private Button treatmentPlanButton;
        [InjectPath("MenuPanel/TreatmentPlanButton/Icon")] private Image treatmentPlanIcon;
        [InjectPath("MenuPanel/GeneralButton")] private Button generalButton;
        [InjectPath("MenuPanel/HospitalButton")] private Button hospitalButton;
        [InjectPath("MenuPanel/DoctorButton")] private Button doctorButton;
        [InjectPath("MenuPanel/HospitalButton/LockImage")] private Image hospitalLockImage;
        [InjectPath("MenuPanel/DoctorButton/LockImage")] private Image doctorLockImage;

        [InjectPath("WorkPanel/TreatmentPlanPanel")] private GameObject treatmentPlanPanel;
        [InjectPath("WorkPanel/TreatmentPlanPanel/TreatmentPlanRowsContent")] private RectTransform treatmentPlanRowsContent;
        [InjectPath("WorkPanel/TreatmentPlanPanel/TreatmentPlanRowsContent/TreatmentPlanRowTemplate")] private GameObject treatmentPlanRowTemplate;
        [InjectPath("WorkPanel/TreatmentPlanPanel/TreatmentPlanEmptyState")] private GameObject treatmentPlanEmptyState;
        [InjectPath("WorkPanel/TreatmentPlanPanel/OpenAddPlanButton")] private Button openAddPlanButton;
        [InjectPath("WorkPanel/TreatmentPlanPanel/OpenAddPlanButton/OpenAddPlanIcon")] private Image openAddPlanIcon;

        [InjectPath("TreatmentPlanEditorPanel")] private GameObject treatmentPlanEditorPanel;
        [InjectPath("TreatmentPlanEditorPanel/ClosePlanEditorButton")] private Button closePlanEditorButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanNameInputField")] private TMP_InputField planNameInput;
        [InjectPath("TreatmentPlanEditorPanel/PlanModeCpmButton")] private Button planModeCpmButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanModeAssistButton")] private Button planModeAssistButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanModeActiveButton")] private Button planModeActiveButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanModeResistButton")] private Button planModeResistButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionLimitDropdown")] private TMP_Dropdown planDirectionLimitDropdown;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration3Button")] private Button planDuration3Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration5Button")] private Button planDuration5Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration10Button")] private Button planDuration10Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration15Button")] private Button planDuration15Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration20Button")] private Button planDuration20Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration25Button")] private Button planDuration25Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDuration30Button")] private Button planDuration30Button;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionAbductionButton")] private Button planDirectionAbductionButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionFlexionButton")] private Button planDirectionFlexionButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionHorizontalButton")] private Button planDirectionHorizontalButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionPronationButton")] private Button planDirectionPronationButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionRotationButton")] private Button planDirectionRotationButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanDirectionExtensionButton")] private Button planDirectionExtensionButton;
        [InjectPath("TreatmentPlanEditorPanel/PlanGameCard/PlanGameDropdown")] private TMP_Dropdown planGameDropdown;
        [InjectPath("TreatmentPlanEditorPanel/PlanGameCard/PlanGameScroll")] private ScrollRect planGameScroll;
        [InjectPath("TreatmentPlanEditorPanel/PlanGameCard/PlanGameScroll/Viewport/Content")] private RectTransform planGameRowsContent;
        [InjectPath("TreatmentPlanEditorPanel/PlanGameCard/PlanGameScroll/Viewport/Content/PlanGameRowTemplate")] private GameObject planGameRowTemplate;
        [InjectPath("TreatmentPlanEditorPanel/PlanGameCard/PlanGameScroll/Viewport/Content/PlanGameEmptyText")] private GameObject planGameEmptyText;
        [InjectPath("TreatmentPlanEditorPanel/PlanGameCard/PlanAddGameButton")] private Button planAddGameButton;
        [InjectPath("TreatmentPlanEditorPanel/CancelPlanEditorButton")] private Button cancelPlanEditorButton;
        [InjectPath("TreatmentPlanEditorPanel/SavePlanButton")] private Button savePlanButton;

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
        private List<TreatmentPlan> treatmentPlans;
        private TreatmentPlan editingPlan;
        private HashSet<string> editingDirections = new HashSet<string>();
        private TrainingMode pendingMode = TrainingMode.Assist;
        private int pendingDuration = 20;
        private List<TreatmentPlanItem> editingGames = new List<TreatmentPlanItem>();
        private List<GameDefinition> availableGames = new List<GameDefinition>();
        private GameObject selectedPlanRow;
        private const int PageSize = 16;
        private string versionType = "";

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

            addSexDropdown.onValueChanged.AddListener(index =>
            {
                addAvatarImage.sprite = (Sex)index == Sex.Female ? Global.IconDic["femaleDoctor"] : Global.IconDic["maleDoctor"];
            });
            infoSexDropdown.onValueChanged.AddListener(index =>
            {
                infoAvatarImage.sprite = (Sex)index == Sex.Female ? Global.IconDic["femaleDoctor"] : Global.IconDic["maleDoctor"];
            });

            treatmentPlanButton.onClick.AddListener(OnOpenTreatmentPlanPanel);
            SetIcon(treatmentPlanIcon, "treatmentPlan");
            SetIcon(openAddPlanIcon, "add");
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

            openAddPlanButton.onClick.AddListener(() => OpenPlanEditor(null));
            closePlanEditorButton.onClick.AddListener(ClosePlanEditor);
            cancelPlanEditorButton.onClick.AddListener(ClosePlanEditor);
            savePlanButton.onClick.AddListener(SaveTreatmentPlan);
            planDuration3Button.onClick.AddListener(() => SelectPlanDuration(3));
            planDuration5Button.onClick.AddListener(() => SelectPlanDuration(5));
            planDuration10Button.onClick.AddListener(() => SelectPlanDuration(10));
            planDuration15Button.onClick.AddListener(() => SelectPlanDuration(15));
            planDuration20Button.onClick.AddListener(() => SelectPlanDuration(20));
            planDuration25Button.onClick.AddListener(() => SelectPlanDuration(25));
            planDuration30Button.onClick.AddListener(() => SelectPlanDuration(30));
            planDirectionAbductionButton.onClick.AddListener(() => TogglePlanDirection("abduction"));
            planDirectionFlexionButton.onClick.AddListener(() => TogglePlanDirection("flexion"));
            planDirectionHorizontalButton.onClick.AddListener(() => TogglePlanDirection("horizontal"));
            planDirectionPronationButton.onClick.AddListener(() => TogglePlanDirection("pronation"));
            planDirectionRotationButton.onClick.AddListener(() => TogglePlanDirection("rotation"));
            planDirectionExtensionButton.onClick.AddListener(() => TogglePlanDirection("extension"));
            planModeCpmButton.onClick.AddListener(() => SelectPlanMode(TrainingMode.Cpm));
            planModeAssistButton.onClick.AddListener(() => SelectPlanMode(TrainingMode.Assist));
            planModeActiveButton.onClick.AddListener(() => SelectPlanMode(TrainingMode.Active));
            planModeResistButton.onClick.AddListener(() => SelectPlanMode(TrainingMode.Resist));
            planAddGameButton.onClick.AddListener(AddPlanGame);
            planDirectionLimitDropdown.ClearOptions();
            planDirectionLimitDropdown.AddOptions(new List<string> { "非限制方向", "限制方向" });

            planDirectionLimitDropdown.onValueChanged.AddListener(_ =>
            {
                if (planDirectionLimitDropdown.value == 1 && editingDirections.Count > 1)
                {
                    string first = null;
                    foreach (string direction in TrainingCatalog.DirectionIds)
                    {
                        if (!editingDirections.Contains(direction)) continue;
                        first = direction;
                        break;
                    }
                    editingDirections.Clear();
                    if (first != null) editingDirections.Add(first);
                }
                RefreshPlanEditorSelection();
                RefreshPlanGameOptions();
            });

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

            if (args != null && args.Length > 0 && args[0] as string == "addTreatmentPlan")
            {
                OnOpenTreatmentPlanPanel();
                OpenPlanEditor(null);
            }
            else
            {
                OnOpenGeneralPanel();
            }
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
            deviceText.text = "硬件版本：通讯未接入";

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
            treatmentPlanPanel.SetActive(false);
            hospitalPanel.SetActive(false);
            doctorPanel.SetActive(false);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            HideModalOnMain(treatmentPlanEditorPanel);
            SetTabButtonSelected(treatmentPlanButton, false);
            SetTabButtonSelected(generalButton, true);
            SetTabButtonSelected(hospitalButton, false);
            SetTabButtonSelected(doctorButton, false);
        }

        private void OnOpenTreatmentPlanPanel()
        {
            generalPanel.SetActive(false);
            treatmentPlanPanel.SetActive(true);
            hospitalPanel.SetActive(false);
            doctorPanel.SetActive(false);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            HideModalOnMain(treatmentPlanEditorPanel);
            SetTabButtonSelected(treatmentPlanButton, true);
            SetTabButtonSelected(generalButton, false);
            SetTabButtonSelected(hospitalButton, false);
            SetTabButtonSelected(doctorButton, false);
            treatmentPlans = DBManagerModule.Instance.QueryAllTreatmentPlans() ?? new List<TreatmentPlan>();
            RefreshTreatmentPlanRows();
        }

        private void OnOpenHospitalPanel()
        {
            if (!hospitalButton.interactable) return;
            generalPanel.SetActive(false);
            treatmentPlanPanel.SetActive(false);
            hospitalPanel.SetActive(true);
            doctorPanel.SetActive(false);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            SetTabButtonSelected(treatmentPlanButton, false);
            SetTabButtonSelected(generalButton, false);
            SetTabButtonSelected(hospitalButton, true);
            SetTabButtonSelected(doctorButton, false);

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
            treatmentPlanPanel.SetActive(false);
            hospitalPanel.SetActive(false);
            doctorPanel.SetActive(true);
            HideModalOnMain(doctorAddPanel);
            HideModalOnMain(doctorEditPanel);
            SetTabButtonSelected(treatmentPlanButton, false);
            SetTabButtonSelected(generalButton, false);
            SetTabButtonSelected(hospitalButton, false);
            SetTabButtonSelected(doctorButton, true);

            adminInfos = DBManagerModule.Instance.QueryAllAdminInfos() ?? new List<AdminInfo>();
            if (curAdmin != null && adminInfos.FindIndex(a => a.Account == curAdmin.Account) < 0) curAdmin = null;
            RefreshDoctorRows(true);
        }

        private void RefreshTreatmentPlanRows()
        {
            for (int i = treatmentPlanRowsContent.childCount - 1; i >= 0; i--)
            {
                Transform child = treatmentPlanRowsContent.GetChild(i);
                if (child == treatmentPlanRowTemplate.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            selectedPlanRow = null;
            treatmentPlanEmptyState.SetActive(treatmentPlans == null || treatmentPlans.Count == 0);

            if (treatmentPlans == null) return;
            RectTransform templateRect = treatmentPlanRowTemplate.GetComponent<RectTransform>();
            for (int i = 0; i < treatmentPlans.Count; i++)
            {
                TreatmentPlan plan = treatmentPlans[i];
                GameObject row = UnityEngine.Object.Instantiate(treatmentPlanRowTemplate, treatmentPlanRowsContent);
                row.name = "TreatmentPlanRow_" + i;
                row.SetActive(true);
                RectTransform rect = row.GetComponent<RectTransform>();
                if (rect != null && templateRect != null)
                    rect.anchoredPosition = new Vector2(templateRect.anchoredPosition.x, templateRect.anchoredPosition.y - i * 58f);

                List<TrainingMode> modes = plan.GetModes();
                List<string> modeNames = new List<string>();
                foreach (TrainingMode planMode in modes) modeNames.Add(TrainingCatalog.GetModeName(planMode));
                List<TreatmentPlanItem> games = plan.GetGameItems();
                int totalMinutes = 0;
                foreach (TreatmentPlanItem item in games) totalMinutes += item.DurationMinutes;
                SetChildText(row, "RowPlanNameText", plan.Name);
                SetChildText(row, "RowPlanModeText", modeNames.Count > 0 ? string.Join("、", modeNames) : "--");
                SetChildText(row, "RowPlanGamesText", games.Count + " 个游戏");
                SetChildText(row, "RowPlanDurationText", totalMinutes + " 分钟");
                SetChildText(row, "RowPlanUpdateText", plan.UpdateTime == default(DateTime) ? "--" : plan.UpdateTime.ToString("yyyy-MM-dd HH:mm"));

                TreatmentPlan captured = plan;
                Button rowButton = row.GetComponent<Button>();
                if (rowButton == null) rowButton = row.AddComponent<Button>();
                rowButton.transition = Selectable.Transition.None;
                rowButton.targetGraphic = row.GetComponent<Image>();
                rowButton.onClick.RemoveAllListeners();
                rowButton.onClick.AddListener(() =>
                {
                    selectedPlanRow = row;
                    HighlightTreatmentPlanRow();
                });

                Transform editNode = row.transform.Find("EditPlanButton");
                if (editNode != null)
                {
                    Button editButton = editNode.GetComponent<Button>();
                    editButton.onClick.RemoveAllListeners();
                    editButton.onClick.AddListener(() => OpenPlanEditor(captured));
                }
            }
        }

        private void HighlightTreatmentPlanRow()
        {
            for (int i = 0; i < treatmentPlanRowsContent.childCount; i++)
            {
                Transform child = treatmentPlanRowsContent.GetChild(i);
                if (child == treatmentPlanRowTemplate.transform) continue;
                Image image = child.GetComponent<Image>();
                if (image != null) image.color = child.gameObject == selectedPlanRow ? new Color(0.937f, 0.965f, 1f, 1f) : Color.white;
            }
        }

        private void OpenPlanEditor(TreatmentPlan plan)
        {
            editingPlan = plan != null ? plan : new TreatmentPlan
            {
                Name = "",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            editingDirections = new HashSet<string> { "abduction" };
            pendingMode = TrainingMode.Assist;
            pendingDuration = 20;
            editingGames = editingPlan.GetGameItems();
            planNameInput.text = editingPlan.Name ?? "";
            planDirectionLimitDropdown.SetValueWithoutNotify(0);
            RefreshPlanGameOptions();
            RefreshPlanGameRows();
            RefreshPlanEditorSelection();
            ShowModalOnMain(treatmentPlanEditorPanel);
        }

        private void ClosePlanEditor()
        {
            HideModalOnMain(treatmentPlanEditorPanel);
            editingPlan = null;
            editingDirections.Clear();
            editingGames.Clear();
        }

        private void SelectPlanMode(TrainingMode mode)
        {
            pendingMode = mode;
            RefreshPlanEditorSelection();
            RefreshPlanGameOptions();
        }

        private void SelectPlanDuration(int minutes)
        {
            pendingDuration = minutes;
            RefreshPlanEditorSelection();
        }

        private void TogglePlanDirection(string direction)
        {
            if (planDirectionLimitDropdown.value == 1)
            {
                editingDirections.Clear();
                editingDirections.Add(direction);
            }
            else if (editingDirections.Contains(direction))
            {
                if (editingDirections.Count > 1) editingDirections.Remove(direction);
            }
            else editingDirections.Add(direction);
            RefreshPlanEditorSelection();
            RefreshPlanGameOptions();
        }

        private void RefreshPlanEditorSelection()
        {
            SetSelected(planDuration3Button, pendingDuration == 3);
            SetSelected(planDuration5Button, pendingDuration == 5);
            SetSelected(planDuration10Button, pendingDuration == 10);
            SetSelected(planDuration15Button, pendingDuration == 15);
            SetSelected(planDuration20Button, pendingDuration == 20);
            SetSelected(planDuration25Button, pendingDuration == 25);
            SetSelected(planDuration30Button, pendingDuration == 30);
            SetSelected(planDirectionAbductionButton, editingDirections.Contains("abduction"));
            SetSelected(planDirectionFlexionButton, editingDirections.Contains("flexion"));
            SetSelected(planDirectionHorizontalButton, editingDirections.Contains("horizontal"));
            SetSelected(planDirectionPronationButton, editingDirections.Contains("pronation"));
            SetSelected(planDirectionRotationButton, editingDirections.Contains("rotation"));
            SetSelected(planDirectionExtensionButton, editingDirections.Contains("extension"));
            SetSelected(planModeCpmButton, pendingMode == TrainingMode.Cpm);
            SetSelected(planModeAssistButton, pendingMode == TrainingMode.Assist);
            SetSelected(planModeActiveButton, pendingMode == TrainingMode.Active);
            SetSelected(planModeResistButton, pendingMode == TrainingMode.Resist);
        }

        private void RefreshPlanGameOptions()
        {
            string selectedId = availableGames.Count > 0 && planGameDropdown.value >= 0 && planGameDropdown.value < availableGames.Count
                ? availableGames[planGameDropdown.value].Id
                : null;
            availableGames = new List<GameDefinition>();
            foreach (GameDefinition game in GameServices.Games)
            {
                if (!MatchesPlanGame(game, pendingMode, planDirectionLimitDropdown.value == 1, editingDirections)) continue;
                if (editingGames.Exists(item => item.GameId == game.Id)) continue;
                availableGames.Add(game);
            }
            List<string> names = new List<string>();
            foreach (GameDefinition game in availableGames)
                names.Add(string.IsNullOrEmpty(game.DisplayName) ? game.Id : game.DisplayName);
            if (names.Count == 0)
                names.Add(GameServices.Catalog == null ? "等待游戏目录接口接入" : "当前筛选无匹配游戏");
            planGameDropdown.ClearOptions();
            planGameDropdown.AddOptions(names);
            int selectedIndex = availableGames.FindIndex(game => game.Id == selectedId);
            planGameDropdown.SetValueWithoutNotify(Mathf.Max(0, selectedIndex));
        }

        private static bool MatchesPlanGame(GameDefinition game, TrainingMode mode, bool directionRestricted, HashSet<string> directions)
        {
            if (game == null || string.IsNullOrEmpty(game.Id) || game.Modes == null || Array.IndexOf(game.Modes, mode) < 0) return false;
            return !directionRestricted || directions.Contains(game.Direction);
        }

        private void AddPlanGame()
        {
            if (editingPlan == null) return;
            if (availableGames.Count == 0)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide(GameServices.Catalog == null
                    ? "游戏目录接口尚未接入，暂无可添加游戏"
                    : "当前训练模式与方向没有匹配游戏");
                return;
            }
            GameDefinition game = availableGames[Mathf.Clamp(planGameDropdown.value, 0, availableGames.Count - 1)];
            if (editingGames.Exists(item => item.GameId == game.Id))
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("该游戏已在方案中");
                return;
            }
            bool restricted = planDirectionLimitDropdown.value == 1;
            string direction = game.Direction;
            if (restricted && !editingDirections.Contains(direction))
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("游戏运动方向与当前限定方向不一致");
                return;
            }
            editingGames.Add(new TreatmentPlanItem
            {
                GameId = game.Id,
                DisplayName = game.DisplayName,
                Mode = (int)pendingMode,
                Direction = direction,
                IsDirectionRestricted = restricted,
                DurationMinutes = pendingDuration,
                ResistanceLevel = 3
            });
            RefreshPlanGameOptions();
            RefreshPlanGameRows();
        }

        private void RefreshPlanGameRows()
        {
            if (planGameRowsContent == null) return;
            for (int i = planGameRowsContent.childCount - 1; i >= 0; i--)
            {
                Transform child = planGameRowsContent.GetChild(i);
                if (child == planGameRowTemplate.transform || child == planGameEmptyText.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            planGameEmptyText.SetActive(editingGames.Count == 0);
            RectTransform templateRect = planGameRowTemplate.GetComponent<RectTransform>();
            for (int i = 0; i < editingGames.Count; i++)
            {
                TreatmentPlanItem item = editingGames[i];
                GameObject row = UnityEngine.Object.Instantiate(planGameRowTemplate, planGameRowsContent);
                row.name = "PlanGameRow_" + i;
                row.SetActive(true);
                RectTransform rect = row.GetComponent<RectTransform>();
                if (rect != null && templateRect != null)
                    rect.anchoredPosition = new Vector2(templateRect.anchoredPosition.x, templateRect.anchoredPosition.y - i * 30f);
                SetChildText(row, "RowGameNameText", string.IsNullOrEmpty(item.DisplayName) ? item.GameId : item.DisplayName);
                string directionText = item.IsDirectionRestricted
                    ? "限制 · " + TrainingCatalog.GetDirectionName(item.Direction)
                    : "非限制方向";
                SetChildText(row, "RowGameConfigText", TrainingCatalog.GetModeName((TrainingMode)item.Mode) + " · " + directionText + " · " + item.DurationMinutes + " 分钟");
                TreatmentPlanItem captured = item;
                Transform removeNode = row.transform.Find("RemovePlanGameButton");
                if (removeNode != null)
                {
                    Button removeButton = removeNode.GetComponent<Button>();
                    removeButton.onClick.RemoveAllListeners();
                    removeButton.onClick.AddListener(() =>
                    {
                        editingGames.Remove(captured);
                        RefreshPlanGameOptions();
                        RefreshPlanGameRows();
                    });
                }
            }
            planGameRowsContent.sizeDelta = new Vector2(planGameRowsContent.sizeDelta.x, Mathf.Max(102f, editingGames.Count * 30f + 8f));
            if (planGameScroll != null) planGameScroll.verticalNormalizedPosition = 1f;
        }

        private void SaveTreatmentPlan()
        {
            if (editingPlan == null) return;
            if (string.IsNullOrWhiteSpace(planNameInput.text))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("提示", "请输入治疗方案名称", "确定");
                return;
            }
            if (editingGames.Count == 0)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("提示", "请至少添加一个游戏", "确定");
                return;
            }

            editingPlan.Name = planNameInput.text.Trim();
            editingPlan.UpdateTime = DateTime.Now;
            editingPlan.SetGameItems(editingGames);

            bool success = editingPlan.PlanID > 0
                ? DBManagerModule.Instance.UpdateTreatmentPlan(editingPlan)
                : DBManagerModule.Instance.InsertTreatmentPlan(editingPlan);
            if (!success)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "治疗方案保存失败", "确定");
                return;
            }
            ClosePlanEditor();
            treatmentPlans = DBManagerModule.Instance.QueryAllTreatmentPlans() ?? new List<TreatmentPlan>();
            RefreshTreatmentPlanRows();
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("治疗方案已保存");
        }

        private static void SetSelected(Button button, bool selected)
        {
            if (button == null) return;
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = selected ? new Color(0.937f, 0.965f, 1f, 1f) : Color.white;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = selected ? new Color(0.078f, 0.337f, 0.941f, 1f) : new Color(0.271f, 0.318f, 0.369f, 1f);
        }

        private static void SetIcon(Image image, string key)
        {
            if (image == null) return;
            Sprite sprite;
            if (Global.IconDic.TryGetValue(key, out sprite))
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
        }

        private void OnOpenHelp()
        {
            Application.OpenURL(Application.streamingAssetsPath + "/help.pdf");
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
            Log.Info("OnSelectHospitalLogo: 打开选择 Logo 文件对话框");
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile(path =>
            {
                Log.Info("OnSelectHospitalLogo: 回调 path=" + (string.IsNullOrEmpty(path) ? "<null>" : path));
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
            HideModalOnMain(treatmentPlanEditorPanel);
            SaveConfigModule.Instance.SetUserData("musicVolume", Mathf.RoundToInt(musicSlider.value));
            SaveConfigModule.Instance.SetUserData("soundVolume", Mathf.RoundToInt(soundSlider.value));
            SaveConfigModule.Instance.SaveUserData();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            addSexDropdown.onValueChanged.RemoveAllListeners();
            infoSexDropdown.onValueChanged.RemoveAllListeners();
            treatmentPlanButton.onClick.RemoveAllListeners();
            generalButton.onClick.RemoveAllListeners();
            hospitalButton.onClick.RemoveAllListeners();
            doctorButton.onClick.RemoveAllListeners();
            helpButton.onClick.RemoveAllListeners();
            saveHospitalButton.onClick.RemoveAllListeners();
            selectLogoButton.onClick.RemoveAllListeners();
            deleteLogoButton.onClick.RemoveAllListeners();
            openAddPlanButton.onClick.RemoveAllListeners();
            closePlanEditorButton.onClick.RemoveAllListeners();
            cancelPlanEditorButton.onClick.RemoveAllListeners();
            savePlanButton.onClick.RemoveAllListeners();
            planModeCpmButton.onClick.RemoveAllListeners();
            planModeAssistButton.onClick.RemoveAllListeners();
            planModeActiveButton.onClick.RemoveAllListeners();
            planModeResistButton.onClick.RemoveAllListeners();
            planAddGameButton.onClick.RemoveAllListeners();
            planGameDropdown.onValueChanged.RemoveAllListeners();
            planDuration3Button.onClick.RemoveAllListeners();
            planDuration5Button.onClick.RemoveAllListeners();
            planDuration10Button.onClick.RemoveAllListeners();
            planDuration15Button.onClick.RemoveAllListeners();
            planDuration20Button.onClick.RemoveAllListeners();
            planDuration25Button.onClick.RemoveAllListeners();
            planDuration30Button.onClick.RemoveAllListeners();
            planDirectionAbductionButton.onClick.RemoveAllListeners();
            planDirectionFlexionButton.onClick.RemoveAllListeners();
            planDirectionHorizontalButton.onClick.RemoveAllListeners();
            planDirectionPronationButton.onClick.RemoveAllListeners();
            planDirectionRotationButton.onClick.RemoveAllListeners();
            planDirectionExtensionButton.onClick.RemoveAllListeners();
            planDirectionLimitDropdown.onValueChanged.RemoveAllListeners();
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

    }
}
