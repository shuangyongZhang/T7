using System;
using System.Collections.Generic;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace LB
{
    /// <summary>
    /// 患者信息UI（由 WebUI2UnityUI 转换的 UIPatientInfoPanel 驱动）
    /// </summary>
    [UIResource("UIPatientInfoPanel")]
    public class UIPatientInfo : UILogicResident
    {
        [InjectPath("ConsolePanel/ConsoleHeader/CountBadge/PatientCountText")] private TMP_Text patientCountText;
        [InjectPath("ConsolePanel/SearchPanel/SearchInputField")] private TMP_InputField searchInputField;
        [InjectPath("ConsolePanel/AddPatientButton")] private Button openAddPanelButton;

        [InjectPath("CanvasPanel/PatientRowsContent")] private RectTransform rowsContent;
        [InjectPath("CanvasPanel/PatientRowsContent/PatientRowTemplate")] private GameObject rowTemplate;

        [InjectPath("CanvasPanel/PatientPager")] private RectTransform patientPager;
        [InjectPath("CanvasPanel/PatientPager/PrevPageButton")] private Button prevPageButton;
        [InjectPath("CanvasPanel/PatientPager/NextPageButton")] private Button nextPageButton;
        [InjectPath("CanvasPanel/PatientPager/PageNumberTemplate")] private GameObject pageNumberTemplate;
        [InjectPath("CanvasPanel/PatientPager/PageEllipsisTemplate")] private GameObject pageEllipsisTemplate;


        [InjectPath("AddPatientPanel")] private GameObject addPatientPanel;
        [InjectPath("AddPatientPanel/AddCloseButton")] private Button addCloseButton;
        [InjectPath("AddPatientPanel/AddNameInputField")] private TMP_InputField addNameInput;
        [InjectPath("AddPatientPanel/AddCardNumInputField")] private TMP_InputField addCardNumInput;
        [InjectPath("AddPatientPanel/AddAgeInputField")] private TMP_InputField addAgeInput;
        [InjectPath("AddPatientPanel/AddStatureInputField")] private TMP_InputField addStatureInput;
        [InjectPath("AddPatientPanel/AddWeightInputField")] private TMP_InputField addWeightInput;
        [InjectPath("AddPatientPanel/AddSexDropdown")] private TMP_Dropdown addSexDropdown;
        [InjectPath("AddPatientPanel/AddAffectedSideDropdown")] private TMP_Dropdown addAffectedSideDropdown;
        [InjectPath("AddPatientPanel/AddDiagnoseInputField")] private TMP_InputField addDiagnoseInput;
        [InjectPath("AddPatientPanel/CancelAddButton")] private Button cancelAddButton;
        [InjectPath("AddPatientPanel/ConfirmAddButton")] private Button confirmAddButton;

        [InjectPath("EditPatientPanel")] private GameObject editPatientPanel;
        [InjectPath("EditPatientPanel/EditCloseButton")] private Button editCloseButton;
        [InjectPath("EditPatientPanel/EditNameInputField")] private TMP_InputField editNameInput;
        [InjectPath("EditPatientPanel/EditCardNumInputField")] private TMP_InputField editCardNumInput;
        [InjectPath("EditPatientPanel/EditAgeInputField")] private TMP_InputField editAgeInput;
        [InjectPath("EditPatientPanel/EditStatureInputField")] private TMP_InputField editStatureInput;
        [InjectPath("EditPatientPanel/EditWeightInputField")] private TMP_InputField editWeightInput;
        [InjectPath("EditPatientPanel/EditSexDropdown")] private TMP_Dropdown editSexDropdown;
        [InjectPath("EditPatientPanel/EditAffectedSideDropdown")] private TMP_Dropdown editAffectedSideDropdown;
        [InjectPath("EditPatientPanel/EditDiagnoseInputField")] private TMP_InputField editDiagnoseInput;
        [InjectPath("EditPatientPanel/EditCreateTimeInputField")] private TMP_InputField editCreateTimeInput;
        [InjectPath("EditPatientPanel/DeletePatientButton")] private Button deletePatientButton;
        [InjectPath("EditPatientPanel/CancelEditButton")] private Button cancelEditButton;
        [InjectPath("EditPatientPanel/ConfirmEditButton")] private Button confirmEditButton;

        private List<PatientInfo> patientInfos;
        private List<PatientInfo> displayedPatients;
        private PatientInfo curPatient;
        private GameObject selectedRow;
        private string curDoctor;
        private int curPage;
        private const int PageSize = 11;

        public override void OnInit()
        {
            base.OnInit();

            var sexOptions = EnumHelper.GetDescriptions(typeof(Sex));
            var sideOptions = EnumHelper.GetDescriptions(typeof(AffectedSide));
            addSexDropdown.ClearOptions();
            addSexDropdown.AddOptions(sexOptions);
            editSexDropdown.ClearOptions();
            editSexDropdown.AddOptions(sexOptions);
            addAffectedSideDropdown.ClearOptions();
            addAffectedSideDropdown.AddOptions(sideOptions);
            editAffectedSideDropdown.ClearOptions();
            editAffectedSideDropdown.AddOptions(sideOptions);

            searchInputField.onValueChanged.AddListener(OnSearchTextChanged);
            openAddPanelButton.onClick.AddListener(OnOpenAddPatientPanel);
            prevPageButton.onClick.AddListener(() => TurnPage(-1));
            nextPageButton.onClick.AddListener(() => TurnPage(1));

            addCloseButton.onClick.AddListener(CloseAddPatientPanel);
            cancelAddButton.onClick.AddListener(CloseAddPatientPanel);
            confirmAddButton.onClick.AddListener(OnAddPatient);

            editCloseButton.onClick.AddListener(CloseEditPatientPanel);
            cancelEditButton.onClick.AddListener(CloseEditPatientPanel);
            confirmEditButton.onClick.AddListener(OnSavePatient);
            deletePatientButton.onClick.AddListener(OnRemovePatient);
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);

            if (patientInfos == null || curDoctor != Global.CurAdminInfo.Account)
            {
                patientInfos = DBManagerModule.Instance.QueryAllPatientInfos(Global.CurAdminInfo) ?? new List<PatientInfo>();
                curDoctor = Global.CurAdminInfo.Account;
                curPatient = null;
            }

            searchInputField.text = "";
            CloseAddPatientPanel();
            CloseEditPatientPanel();
            RefreshPatientList(null, true);
        }

        private void OnSearchTextChanged(string text)
        {
            curPage = 0;
            RefreshPatientList(text);
        }

        private void RefreshPatientList(string keyword = null, bool followSelection = false)
        {
            if (patientInfos == null) patientInfos = new List<PatientInfo>();
            string key = string.IsNullOrEmpty(keyword) ? searchInputField.text : keyword;

            displayedPatients = string.IsNullOrEmpty(key)
                ? new List<PatientInfo>(patientInfos)
                : patientInfos.FindAll(p =>
                    (p.CardNum != null && p.CardNum.Contains(key)) ||
                    (p.Name != null && p.Name.Contains(key)) ||
                    (p.Diagnose != null && p.Diagnose.Contains(key)));

            patientCountText.text = string.Format("共 {0} 名患者", displayedPatients.Count);

            int pageCount = Mathf.Max(1, Mathf.CeilToInt(displayedPatients.Count / (float)PageSize));
            PatientInfo keep = curPatient != null ? curPatient : Global.CurPatientInfo;
            int keepIndex = keep != null ? displayedPatients.FindIndex(p => p.ID == keep.ID) : -1;
            if (keepIndex >= 0 && followSelection) curPage = keepIndex / PageSize;
            curPage = Mathf.Clamp(curPage, 0, pageCount - 1);

            int start = curPage * PageSize;
            int sliceCount = Mathf.Min(PageSize, displayedPatients.Count - start);
            ClearRows();
            for (int i = 0; i < sliceCount; i++) CreateRow(displayedPatients[start + i], i);

            selectedRow = null;
            if (sliceCount > 0)
            {
                int keepInSlice = keepIndex - start;
                if (keepIndex >= 0 && keepInSlice >= 0 && keepInSlice < sliceCount)
                {
                    SelectPatient(displayedPatients[keepIndex]);
                    Transform keepRow = rowsContent.Find("PatientRow_" + keepInSlice);
                    if (keepRow != null) HighlightRow(keepRow.gameObject);
                }
                else if (keepIndex < 0)
                {
                    SelectPatient(displayedPatients[start]);
                    Transform firstRow = rowsContent.Find("PatientRow_0");
                    if (firstRow != null) HighlightRow(firstRow.gameObject);
                }
            }
            else
            {
                curPatient = null;
                Global.CurPatientInfo = null;
                Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventPatientChanged>().Fill(null));
            }

            UIPagerBar.Refresh(patientPager, prevPageButton, nextPageButton, pageNumberTemplate, pageEllipsisTemplate,
                pageCount, curPage, OnPageSelected);
        }

        private void TurnPage(int delta)
        {
            curPage += delta;
            RefreshPatientList();
        }

        private void OnPageSelected(int page)
        {
            curPage = page;
            RefreshPatientList();
        }

        private void ClearRows()
        {
            for (int i = rowsContent.childCount - 1; i >= 0; i--)
            {
                Transform child = rowsContent.GetChild(i);
                if (child == rowTemplate.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private void CreateRow(PatientInfo patient, int index)
        {
            GameObject row = UnityEngine.Object.Instantiate(rowTemplate, rowsContent);
            row.name = "PatientRow_" + index;
            row.SetActive(true);

            RectTransform templateRect = rowTemplate.GetComponent<RectTransform>();
            RectTransform rowRect = row.GetComponent<RectTransform>();
            if (templateRect != null && rowRect != null)
            {
                Vector2 basePosition = templateRect.anchoredPosition;
                rowRect.anchoredPosition = new Vector2(basePosition.x, basePosition.y - index * 56f);
            }

            SetChildText(row, "RowCardNumText", patient.CardNum);
            SetChildText(row, "RowNameText", patient.Name);
            SetChildText(row, "RowSexText", patient.Sex == Sex.Female ? "女" : "男");
            SetChildText(row, "RowAgeText", patient.Age > 0 ? patient.Age + " 岁" : "-");
            SetChildText(row, "RowStatureText", patient.Stature > 0 ? patient.Stature.ToString() : "-");
            SetChildText(row, "RowWeightText", patient.Weight > 0 ? patient.Weight.ToString() : "-");
            SetChildText(row, "RowDiagnoseText", string.IsNullOrEmpty(patient.Diagnose) ? "未填写诊断" : patient.Diagnose);
            SetChildText(row, "RowCreateTimeText", patient.CreateTime == default(DateTime) ? "-" : patient.CreateTime.ToString("yyyy-MM-dd"));

            PatientInfo captured = patient;
            Button rowButton = row.GetComponent<Button>();
            if (rowButton != null)
            {
                rowButton.transition = Selectable.Transition.None;
                rowButton.onClick.RemoveAllListeners();
                rowButton.onClick.AddListener(() => SelectPatient(captured));
                rowButton.onClick.AddListener(() => HighlightRow(row));
            }

            Transform editNode = row.transform.Find("SelectRowButton");
            if (editNode != null)
            {
                Button editButton = editNode.GetComponent<Button>();
                editButton.onClick.RemoveAllListeners();
                editButton.onClick.AddListener(() =>
                {
                    SelectPatient(captured);
                    HighlightRow(row);
                    OnOpenEditPatientPanel();
                });
            }
        }

        private void HighlightRow(GameObject row)
        {
            if (selectedRow != null)
            {
                Image oldImage = selectedRow.GetComponent<Image>();
                if (oldImage != null) oldImage.color = Color.white;
            }
            selectedRow = row;
            Image image = row != null ? row.GetComponent<Image>() : null;
            if (image != null) image.color = new Color(0.937f, 0.965f, 1f, 1f);
        }

        private static void SetChildText(GameObject parent, string childName, string value)
        {
            Transform node = parent.transform.Find(childName);
            if (node == null) return;
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text != null) text.text = value ?? "";
        }

        private void SelectPatient(PatientInfo patient)
        {
            if (patient == null) return;
            curPatient = patient;
            Global.CurPatientInfo = patient;

            Main.m_Event.Throw(Main.m_ReferencePool.Spawn<EventPatientChanged>().Fill(patient));
        }

        /// <summary>
        /// 供列表项等外部逻辑按索引打开患者详情
        /// </summary>
        public void OpenPatientDetailPanel(int index)
        {
            if (displayedPatients == null || index < 0 || index >= displayedPatients.Count) return;
            SelectPatient(displayedPatients[index]);
        }

        private void OnOpenAddPatientPanel()
        {
            addNameInput.text = "";
            addCardNumInput.text = DateTime.Now.ToFileTime().ToString();
            addAgeInput.text = "";
            addStatureInput.text = "";
            addWeightInput.text = "";
            addDiagnoseInput.text = "";
            addSexDropdown.SetValueWithoutNotify(0);
            addAffectedSideDropdown.SetValueWithoutNotify(0);
            ShowModalOnMain(addPatientPanel);
        }

        private void CloseAddPatientPanel()
        {
            HideModalOnMain(addPatientPanel);
        }

        private void OnOpenEditPatientPanel()
        {
            if (curPatient == null)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("请先选择患者");
                return;
            }
            editNameInput.text = curPatient.Name ?? "";
            editCardNumInput.text = curPatient.CardNum ?? "";
            editAgeInput.text = curPatient.Age > 0 ? curPatient.Age.ToString() : "";
            editStatureInput.text = curPatient.Stature > 0 ? curPatient.Stature.ToString() : "";
            editWeightInput.text = curPatient.Weight > 0 ? curPatient.Weight.ToString() : "";
            editSexDropdown.SetValueWithoutNotify((int)curPatient.Sex);
            editAffectedSideDropdown.SetValueWithoutNotify((int)curPatient.AffectedSide);
            editDiagnoseInput.text = curPatient.Diagnose ?? "";
            editCreateTimeInput.text = curPatient.CreateTime == default(DateTime) ? "" : curPatient.CreateTime.ToString("yyyy-MM-dd HH:mm");
            ShowModalOnMain(editPatientPanel);
        }

        private void CloseEditPatientPanel()
        {
            HideModalOnMain(editPatientPanel);
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

        private void OnAddPatient()
        {
            if (string.IsNullOrEmpty(addNameInput.text) || string.IsNullOrEmpty(addCardNumInput.text) ||
                string.IsNullOrEmpty(addStatureInput.text) || string.IsNullOrEmpty(addWeightInput.text))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "标记*为必填项，不能为空!", "确定");
                return;
            }

            int stature, weight;
            if (!int.TryParse(addStatureInput.text, out stature) || stature <= 0 || stature > 250)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "身高必须大于0，小于250厘米!", "确定");
                return;
            }
            if (!int.TryParse(addWeightInput.text, out weight) || weight <= 0 || weight > 150)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "体重必须大于0，小于150千克!", "确定");
                return;
            }

            var patient = new PatientInfo
            {
                ID = DateTime.Now.ToFileTime().ToString(),
                Name = addNameInput.text,
                CardNum = addCardNumInput.text,
                Doctor = Global.CurAdminInfo.Account,
                Sex = (Sex)addSexDropdown.value,
                AffectedSide = (AffectedSide)addAffectedSideDropdown.value,
                Age = int.TryParse(addAgeInput.text, out int age) ? age : 0,
                Stature = stature,
                Weight = weight,
                Diagnose = addDiagnoseInput.text,
                AdmissionDate = DateTime.Now,
                CreateTime = DateTime.Now
            };

            if (!DBManagerModule.Instance.InsertPatientInfo(patient))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "新增患者失败!", "确定");
                return;
            }

            patientInfos.Add(patient);
            curPatient = patient;
            CloseAddPatientPanel();
            searchInputField.text = "";
            RefreshPatientList(null, true);
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("新增患者成功");
        }

        private void OnSavePatient()
        {
            if (curPatient == null) return;
            if (string.IsNullOrEmpty(editNameInput.text) || string.IsNullOrEmpty(editCardNumInput.text) ||
                string.IsNullOrEmpty(editStatureInput.text) || string.IsNullOrEmpty(editWeightInput.text))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "信息不能为空!", "确定");
                return;
            }

            int stature, weight;
            if (!int.TryParse(editStatureInput.text, out stature) || stature <= 0 || stature > 250)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "身高必须大于0，小于250厘米!", "确定");
                return;
            }
            if (!int.TryParse(editWeightInput.text, out weight) || weight <= 0 || weight > 150)
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "体重必须大于0，小于150千克!", "确定");
                return;
            }

            PatientInfo updated = curPatient.Clone();
            updated.Name = editNameInput.text;
            updated.CardNum = editCardNumInput.text;
            updated.Age = int.TryParse(editAgeInput.text, out int age) ? age : 0;
            updated.Stature = stature;
            updated.Weight = weight;
            updated.Sex = (Sex)editSexDropdown.value;
            updated.AffectedSide = (AffectedSide)editAffectedSideDropdown.value;
            updated.Diagnose = editDiagnoseInput.text;

            if (!DBManagerModule.Instance.UpdatePatientInfo(updated))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "保存患者信息失败!", "确定");
                return;
            }

            int index = patientInfos.FindIndex(p => p.ID == updated.ID);
            if (index >= 0) patientInfos[index] = updated.Clone();
            curPatient = updated;
            Global.CurPatientInfo = updated;
            CloseEditPatientPanel();
            RefreshPatientList(null, true);
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("保存患者信息成功");
        }

        private void OnRemovePatient()
        {
            PatientInfo target = curPatient;
            if (target == null) return;

            Action yesCallback = () =>
            {
                if (!DBManagerModule.Instance.DeletePatientInfo(target))
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "删除患者失败!", "确定");
                    return;
                }

                patientInfos.RemoveAll(p => p.ID == target.ID);
                curPatient = null;
                Global.CurPatientInfo = null;
                CloseEditPatientPanel();
                RefreshPatientList();
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("删除患者成功");
            };

            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "确定删除患者?", "确定", "取消", index =>
            {
                Main.m_UI.OpenUI<UIAdminPasswordDialog>(yesCallback).Forget();
                return true;
            });
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
            CloseAddPatientPanel();
            CloseEditPatientPanel();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            searchInputField.onValueChanged.RemoveAllListeners();
            openAddPanelButton.onClick.RemoveAllListeners();
            prevPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.RemoveAllListeners();
            addCloseButton.onClick.RemoveAllListeners();
            cancelAddButton.onClick.RemoveAllListeners();
            confirmAddButton.onClick.RemoveAllListeners();
            editCloseButton.onClick.RemoveAllListeners();
            cancelEditButton.onClick.RemoveAllListeners();
            confirmEditButton.onClick.RemoveAllListeners();
            deletePatientButton.onClick.RemoveAllListeners();
        }
    }
}
