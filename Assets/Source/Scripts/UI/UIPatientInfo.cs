using System;
using System.IO;
using System.Collections.Generic;
using HT.Framework;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.UI.ProceduralImage;

namespace LB
{
    /// <summary>
    /// 患者信息UI
    /// </summary>
    [UIResource("UIPatientInfoPanel")]
    // [UIResource("ui", "UIPatientInfoPanel", "Prefabs/UI/UIPatientInfoPanel")]
    public class UIPatientInfo : UILogicResident
    {

        [InjectPath("SearchPanel/SearchInputField")] private TMP_InputField searchInputField;
        [InjectPath("PatientListPanel")] private RectTransform patientListPanel;
        [InjectPath("PatientListPanel/ListView")] private PatientListView patientListView;
        [InjectPath("PatientListPanel/AddPatientButton")] private Button openAddPanelButton;
        [InjectPath("PatientInfoPanel")] private RectTransform patientInfoPanel;
        [InjectPath("PatientInfoPanel/Content")] private ProceduralImage patientInfoPanelContent;
        [InjectPath("PatientInfoPanel/Content/DeleteButton")] private Button deleteButton;
        [InjectPath("PatientInfoPanel/Content/SaveButton")] private Button saveButton;
        [InjectPath("PatientInfoPanel/Content/EditButton")] private Button editButton;
        [InjectPath("PatientInfoPanel/Content/CloseButton")] private Button closeButton;
        [InjectPath("PatientInfoPanel/Content/AvatarImage")] private Image infoPanel_avatarImage;
        [InjectPath("PatientInfoPanel/Content/NameInputField")] private TMP_InputField infoPanel_nameInputField;
        [InjectPath("PatientInfoPanel/Content/AgeInputField")] private TMP_InputField infoPanel_ageInputField;
        [InjectPath("PatientInfoPanel/Content/StatureInputField")] private TMP_InputField infoPanel_statureInputField;
        [InjectPath("PatientInfoPanel/Content/WeightInputField")] private TMP_InputField infoPanel_weightInputField;

        [InjectPath("PatientInfoPanel/Content/CardNumInputField")] private TMP_InputField infoPanel_cardNumInputField;
        [InjectPath("PatientInfoPanel/Content/CreateTimeInputField")] private TMP_InputField infoPanel_createTimeInputField;

        [InjectPath("PatientInfoPanel/Content/SexDropdown")] private TMP_Dropdown infoPanel_sexDropdown;
        [InjectPath("PatientInfoPanel/Content/AffectedSideDropdown")] private TMP_Dropdown infoPanel_affectedSideDropdown;


        [InjectPath("AddPatientPanel")] private RectTransform addPatientPanel;
        [InjectPath("AddPatientPanel/Content")] private ProceduralImage addPatientPanelContent;
        [InjectPath("AddPatientPanel/Content/AvatarImage")] private Image addPanel_avatarImage;
        [InjectPath("AddPatientPanel/Content/NameInputField")] private TMP_InputField addPanel_nameInputField;
        [InjectPath("AddPatientPanel/Content/CardNumInputField")] private TMP_InputField addPanel_cardNumInputField;

        [InjectPath("AddPatientPanel/Content/AgeInputField")] private TMP_InputField addPanel_ageInputField;
        [InjectPath("AddPatientPanel/Content/StatureInputField")] private TMP_InputField addPanel_statureInputField;
        [InjectPath("AddPatientPanel/Content/WeightInputField")] private TMP_InputField addPanel_weightInputField;
        // [InjectPath("AddPatientPanel/Content/AdmissionDateInputField")] private TMP_InputField addPanel_admissionDateInputField;
        // [InjectPath("AddPatientPanel/Content/DiagnoseInputField")] private TMP_InputField addPanel_diagnoseInputField;
        [InjectPath("AddPatientPanel/Content/SexDropdown")] private TMP_Dropdown addPanel_sexDropdown;
        [InjectPath("AddPatientPanel/Content/AffectedSideDropdown")] private TMP_Dropdown addPanel_affectedSideDropdown;


        [InjectPath("AddPatientPanel/Content/CancelButton")] private Button cancelButton;
        [InjectPath("AddPatientPanel/Content/AddButton")] private Button addButton;
        [InjectPath("AddPatientPanel/Content/CloseButton")] private Button addCloseBtn;


        private List<PatientInfo> patientInfos;

        private List<PatientInfo> searchPatientInfos;

        private bool isSearching = false;


        private PatientInfo CurPatientInfo
        {
            get;
            set;
        }
        private string curDoctor;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            patientListView.Init();


            var affectedSideList = EnumHelper.GetDescriptions(typeof(AffectedSide));
            addPanel_affectedSideDropdown.ClearOptions();
            addPanel_affectedSideDropdown.AddOptions(affectedSideList);

            infoPanel_affectedSideDropdown.ClearOptions();
            infoPanel_affectedSideDropdown.AddOptions(affectedSideList);

            var sexList = EnumHelper.GetDescriptions(typeof(Sex));
            addPanel_sexDropdown.ClearOptions();
            addPanel_sexDropdown.AddOptions(sexList);

            infoPanel_sexDropdown.ClearOptions();
            infoPanel_sexDropdown.AddOptions(sexList);

            addButton.onClick.AddListener(OnAddPatient);
            saveButton.onClick.AddListener(OnSavePatient);
            deleteButton.onClick.AddListener(OnRemovePatient);
            editButton.onClick.AddListener(OnEditPatient);
            closeButton.onClick.AddListener(OnCloseAddPatientPanel);
            cancelButton.onClick.AddListener(OnCloseAddPatientPanel);
            addCloseBtn.onClick.AddListener(OnCloseAddPatientPanel);
            openAddPanelButton.onClick.AddListener(OnOpenAddPatientPanel);

            addPanel_sexDropdown.onValueChanged.AddListener((value) =>
            {
                addPanel_avatarImage.sprite = (Sex)value == Sex.Female ? Global.IconDic["women"] : Global.IconDic["man"];
                addPatientPanelContent.sprite = (Sex)value == Sex.Female ? Global.IconDic["womenBigK"] : Global.IconDic["manBigK"];
            });
            infoPanel_sexDropdown.onValueChanged.AddListener((value) =>
            {
                infoPanel_avatarImage.sprite = (Sex)value == Sex.Female ? Global.IconDic["women"] : Global.IconDic["man"];
                patientInfoPanelContent.sprite = (Sex)value == Sex.Female ? Global.IconDic["womenBigK"] : Global.IconDic["manBigK"];
            });

            searchInputField.onValueChanged.AddListener(OnSearchTextChanged);
            patientListView.OnSelect.AddListener(OnPatientItemSelected);
            patientListView.DataSource.OnCollectionChange += OnListViewDataSourceChanged;
        }

        private void SetPatientInfoPanelEditMode(bool editMode)
        {
            infoPanel_nameInputField.interactable = editMode;
            infoPanel_ageInputField.interactable = editMode;
            infoPanel_weightInputField.interactable = editMode;
            infoPanel_statureInputField.interactable = editMode;
            infoPanel_cardNumInputField.interactable = editMode;
            infoPanel_createTimeInputField.interactable = false;
            infoPanel_sexDropdown.interactable = editMode;
            infoPanel_affectedSideDropdown.interactable = editMode;

            saveButton.gameObject.SetActive(editMode);
            editButton.gameObject.SetActive(!editMode);
        }


        private void OnSearchTextChanged(string text)
        {

            if (!string.IsNullOrEmpty(text))
            {
                isSearching = true;
                if (searchPatientInfos != null)
                    searchPatientInfos.Clear();

                searchPatientInfos = patientInfos.FindAll((x) =>
                {
                    return x.CardNum.Contains(text) || x.Name.Contains(text);
                });
                patientListView.DataSource.Clear();
                patientListView.DataSource.BeginUpdate();
                foreach (var patientInfo in searchPatientInfos)
                {
                    var item = new PatientItem() { ID = patientInfo.ID, Name = patientInfo.Name, Age = patientInfo.Age, Sex = patientInfo.Sex, CreateTime = patientInfo.CreateTime };
                    patientListView.DataSource.Add(item);
                }

                patientListView.DataSource.EndUpdate();
                SetListViewDefaultSelect();

            }
            else
            {
                isSearching = false;

                patientListView.DataSource.Clear();
                patientListView.DataSource.BeginUpdate();
                foreach (var patientInfo in patientInfos)
                {
                    var item = new PatientItem() { ID = patientInfo.ID, Name = patientInfo.Name, Age = patientInfo.Age, Sex = patientInfo.Sex, CreateTime = patientInfo.CreateTime };
                    patientListView.DataSource.Add(item);
                }

                patientListView.DataSource.EndUpdate();
                SetListViewDefaultSelect();
            }

        }

        private void OnListViewDataSourceChanged()
        {
            if (patientInfos.Count > 0)
            {
                openAddPanelButton.transform.SetParent(patientListView.Container);
                openAddPanelButton.transform.SetAsFirstSibling();
            }
            else
            {
                openAddPanelButton.transform.SetParent(patientListPanel);
                openAddPanelButton.transform.localPosition = new Vector3(-610, 308);
                openAddPanelButton.transform.SetAsLastSibling();
            }

        }
        private void SetListViewDefaultSelect(int selectedIndex = 0)
        {
            var items = patientListView.DataSource;
            // int selectedIndex = patientListView.SelectedIndex < 0 ? 0 : patientListView.SelectedIndex;
            if (CurPatientInfo != null && !isSearching)
            {
                int index = items.FindIndex(x => x.ID == CurPatientInfo.ID);
                if (index != -1)
                {
                    patientListView.Select(index);
                    // patientListView.ScrollTo(index);
                }
            }
            else
            {
                //默认选择
                if (items.Count > 0)
                {
                    selectedIndex = selectedIndex < items.Count ? selectedIndex : items.Count - 1;
                    patientListView.Select(selectedIndex);
                    // patientListView.ScrollTo(selectedIndex);
                }
                else
                {
                    Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventPatientChanged>().Fill(null));
                }
            }
        }
        private void OnPatientItemSelected(int index, ListViewItem component)
        {

            var item = patientListView.DataSource[index];
            var patientInfo = patientInfos.Find(e => e.ID == item.ID);
            if (patientInfo != null)
            {
                CurPatientInfo = patientInfo;
                Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventPatientChanged>().Fill(patientInfo));
            }

        }


        private void OnOpenAddPatientPanel()
        {

            addPanel_sexDropdown.value = 0;
            addPanel_affectedSideDropdown.value = 0;
            addPanel_avatarImage.sprite = Global.IconDic["women"];
            addPatientPanelContent.sprite = Global.IconDic["womenBigK"];
            addPanel_nameInputField.text = "";
            addPanel_cardNumInputField.text = DateTime.Now.ToFileTime().ToString();
            addPanel_ageInputField.text = "";
            addPanel_statureInputField.text = "";
            addPanel_weightInputField.text = "";

            addPatientPanel.gameObject.SetActive(true);
        }

        private void CloseAddPatientPanel()
        {

            addPatientPanel.gameObject.SetActive(false);
            patientInfoPanel.gameObject.SetActive(false);
        }

        private void OnCloseAddPatientPanel()
        {
            CloseAddPatientPanel();
        }


        //新增患者
        private void OnAddPatient()
        {

            if (addPanel_nameInputField.text == "" ||
                addPanel_cardNumInputField.text == "" ||
                addPanel_statureInputField.text == "" ||
                addPanel_weightInputField.text == "")
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "标记*为必填项,不能为空!", "确定");
                return;
            }


            if (!int.TryParse(addPanel_statureInputField.text, out var stature) || stature <= 0 || stature > 250)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "身高必须大于0,小于250厘米!", "确定");
                return;
            }

            if (!int.TryParse(addPanel_weightInputField.text, out var weight) || weight <= 0 || weight > 150)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "体重必须大于0,小于150千克!", "确定");

                return;
            }

            var patientInfo = new PatientInfo
            {
                ID = DateTime.Now.ToFileTime().ToString(),
                Name = addPanel_nameInputField.text,
                CardNum = addPanel_cardNumInputField.text,
                Doctor = Global.CurAdminInfo.Account,
                Sex = (Sex)addPanel_sexDropdown.value,
                AffectedSide = (AffectedSide)addPanel_affectedSideDropdown.value,
                Age = int.TryParse(addPanel_ageInputField.text, out var age) ? age : 0,
                Weight = weight,
                Stature = stature,
                AdmissionDate = DateTime.Now,
                CreateTime = DateTime.Now
            };

            bool result = DBManagerModule.Instance.InsertPatientInfo(patientInfo);
            if (result)
            {
                patientInfos.Add(patientInfo);
                patientListView.DataSource.BeginUpdate();
                patientListView.DataSource.Add(new PatientItem() { ID = patientInfo.ID, Name = patientInfo.Name, Age = patientInfo.Age, Sex = patientInfo.Sex, CreateTime = patientInfo.CreateTime });
                patientListView.DataSource.EndUpdate();
                CurPatientInfo = patientInfo;


                CloseAddPatientPanel();
                SetListViewDefaultSelect();

                UINotifyDialogHelper.Current.ShowNotifyAutoHide("新增患者成功");
            }
            else
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "新增患者失败!", "确定");
            }


        }
        public void OpenPatientDetailPanel(int index)
        {
            patientListView.Select(index);

            if (CurPatientInfo == null) return;
            infoPanel_nameInputField.text = CurPatientInfo.Name;
            infoPanel_cardNumInputField.text = CurPatientInfo.CardNum;
            infoPanel_ageInputField.text = CurPatientInfo.Age.ToString();
            infoPanel_createTimeInputField.text = CurPatientInfo.CreateTime.ToString("yyyy-MM-dd HH:mm");
            infoPanel_weightInputField.text = CurPatientInfo.Weight > 0 ? CurPatientInfo.Weight.ToString() : "";
            infoPanel_statureInputField.text = CurPatientInfo.Stature > 0 ? CurPatientInfo.Stature.ToString() : "";
            infoPanel_avatarImage.sprite = CurPatientInfo.Sex == Sex.Female ? Global.IconDic["women"] : Global.IconDic["man"];
            patientInfoPanelContent.sprite = CurPatientInfo.Sex == Sex.Female ? Global.IconDic["womenBigK"] : Global.IconDic["manBigK"];
            infoPanel_sexDropdown.value = (int)CurPatientInfo.Sex;
            infoPanel_affectedSideDropdown.value = (int)CurPatientInfo.AffectedSide;

            SetPatientInfoPanelEditMode(false);

            patientInfoPanel.gameObject.SetActive(true);
        }

        //删除患者
        private void OnRemovePatient()
        {
            var patientInfo = CurPatientInfo;
            if (patientInfo == null) return;
            Action yesCallback = () =>
            {
                bool result = DBManagerModule.Instance.DeletePatientInfo(patientInfo);
                if (result)
                {

                    //DBManagerModule.Instance.DeleteAllPatientReport(patientInfo.ID);

                    patientInfos.Remove(patientInfo);

                    int selectedIndex = patientListView.SelectedIndex;
                    patientListView.DataSource.BeginUpdate();
                    patientListView.DataSource.Remove(patientListView.DataSource[selectedIndex]);
                    patientListView.DataSource.EndUpdate();
                    CurPatientInfo = null;
                    SetListViewDefaultSelect(selectedIndex);
                    CloseAddPatientPanel();
                    UINotifyDialogHelper.Current.ShowNotifyAutoHide("删除患者成功");

                }
                else
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "删除患者失败!", "确定");
                }
            };



            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "确定删除患者?", "确定", "取消", (index) =>
               {
                   Main.m_UI.OpenUI<UIAdminPasswordDialog>(yesCallback).Forget();
                   return true;
               });
        }


        private void OnSavePatient()
        {

            if (CurPatientInfo == null) return;
            if (infoPanel_nameInputField.text == "" ||
                infoPanel_cardNumInputField.text == "" ||
                infoPanel_statureInputField.text == "" ||
                infoPanel_weightInputField.text == "")
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "信息不能为空!", "确定");
                return;
            }

            if (!int.TryParse(infoPanel_statureInputField.text, out var stature) || stature <= 0 || stature > 250)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "身高必须大于0,小于250厘米!", "确定");
                return;
            }

            if (!int.TryParse(infoPanel_weightInputField.text, out var weight) || weight <= 0 || weight > 150)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "体重必须大于0,小于150千克!", "确定");

                return;
            }

            // Action yesCallback = () =>
            // {
            var patientInfo = new PatientInfo();
            patientInfo = CurPatientInfo.Clone();
            patientInfo.Name = infoPanel_nameInputField.text;
            patientInfo.Sex = (Sex)infoPanel_sexDropdown.value;
            patientInfo.Age = int.TryParse(infoPanel_ageInputField.text, out var age) ? age : 0;
            patientInfo.Weight = weight;
            patientInfo.Stature = stature;
            patientInfo.CardNum = infoPanel_cardNumInputField.text;
            patientInfo.AffectedSide = (AffectedSide)infoPanel_affectedSideDropdown.value;
            bool result = DBManagerModule.Instance.UpdatePatientInfo(patientInfo);
            if (result)
            {
                int selectedIndex = patientListView.SelectedIndex;
                patientListView.DataSource.BeginUpdate();
                if (selectedIndex >= 0 && selectedIndex < patientListView.DataSource.Count)
                {
                    // patientListView.DataSource[selectedIndex] = new PatientItem() { ID = patientInfo.ID, Name = patientInfo.Name, Age = patientInfo.Age, Sex = patientInfo.Sex };
                    patientListView.DataSource.Remove(patientListView.DataSource[selectedIndex]);
                    patientListView.DataSource.Add(new PatientItem()
                    {
                        ID = patientInfo.ID,
                        Name = patientInfo.Name,
                        Age = patientInfo.Age,
                        Sex = patientInfo.Sex,
                        CreateTime = patientInfo.CreateTime
                    });
                }
                patientListView.DataSource.EndUpdate();

                int index = patientListView.DataSource.FindIndex(e => e.ID == patientInfo.ID);
                if (index >= 0) patientListView.Select(index);
                index = patientInfos.FindIndex(e => e.ID == patientInfo.ID);
                if (index >= 0) patientInfos[index] = patientInfo.Clone();

                SetPatientInfoPanelEditMode(false);

                UINotifyDialogHelper.Current.ShowNotifyAutoHide("保存患者信息成功");

            }
            else
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "保存患者信息失败!", "确定");

            }
            OnCloseAddPatientPanel();
            // };
            // Main.m_UI.OpenUI<UIAdminPasswordDialog>(yesCallback);
        }
        private void OnEditPatient()
        {
            SetPatientInfoPanelEditMode(true);
        }


        /// <summary>
        /// 打开UI
        /// </summary>
        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);

            if (patientInfos == null || curDoctor != Global.CurAdminInfo.Account)
            {
                CurPatientInfo = null;
                patientInfos = DBManagerModule.Instance.QueryAllPatientInfos(Global.CurAdminInfo);
                curDoctor = Global.CurAdminInfo.Account;


                patientListView.DataSource.Clear();
                patientListView.DataSource.BeginUpdate();
                foreach (var patientInfo in patientInfos)
                {
                    var item = new PatientItem() { ID = patientInfo.ID, Name = patientInfo.Name, Age = patientInfo.Age, Sex = patientInfo.Sex, CreateTime = patientInfo.CreateTime };
                    patientListView.DataSource.Add(item);
                }

                patientListView.DataSource.EndUpdate();

            }
            CloseAddPatientPanel();
            SetListViewDefaultSelect();

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
        /// 关闭UI
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();

            patientListView.SelectedIndex = -1;
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        public override void OnDestroy()
        {

            base.OnDestroy();
            isSearching = false;
            patientListView.OnSelect.RemoveAllListeners();
            patientListView.DataSource.OnCollectionChange -= OnListViewDataSourceChanged;

            openAddPanelButton.onClick.RemoveAllListeners();

            cancelButton.onClick.RemoveAllListeners();
            addCloseBtn.onClick.RemoveAllListeners();
            addButton.onClick.RemoveAllListeners();
            deleteButton.onClick.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            editButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();

            searchInputField.onValueChanged.RemoveAllListeners();
            addPanel_sexDropdown.onValueChanged.RemoveAllListeners();
            infoPanel_sexDropdown.onValueChanged.RemoveAllListeners();


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
