using HT.Framework;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace LB
{
    /// <summary>
    /// 
    /// </summary>
    public class PatientItemComponent : ListViewItem, IViewData<PatientItem>
    {

        [SerializeField]
        private ProceduralImage bgK;

        [SerializeField]
        private Image AvatarImage;

        [SerializeField]
        private TMP_Text SexText;

        [SerializeField]
        private TMP_Text NameText;

        [SerializeField]
        private TMP_Text AgeText;

        [SerializeField]
        private TMP_Text DateText;
        [SerializeField]
        private RectTransform ButtonPanel;
        // [SerializeField]
        // private Button TrainButton;

        [SerializeField]
        private Button DetailButton;
        public PatientItem Item
        {
            get;
            protected set;
        }
        protected override void Start()
        {
            DetailButton.onClick.AddListener(OnOpenPatientDetail);
            // TrainButton.onClick.AddListener(OnPatientTrain);
        }
        // private void OnPatientTrain()
        // {
        //     var patientInfoUI = Main.m_UI.GetOpenedUI<UIPatientInfo>();
        //     patientInfoUI?.GoToPatientTrainPanel(Index);
        // }

        private void OnOpenPatientDetail()
        {
            var patientInfoUI = Main.m_UI.GetOpenedUI<UIPatientInfo>();
            patientInfoUI?.OpenPatientDetailPanel(Index);

        }
        protected override void OnDestroy()
        {
            DetailButton.onClick.RemoveAllListeners();
            // TrainButton.onClick.RemoveAllListeners();

        }
        /// <summary>
        /// Set data.
        /// </summary>
        /// <param PatientItem="item">Item.</param>
        public void SetData(PatientItem item)
        {

            // if (item != Item)
            {
                Item = item;
                NameText.text = item.Name;
                AgeText.text = item.Age.ToString();
                DateText.text = item.CreateTime.ToString("yyyy-MM-dd");

                SexText.text = item.Sex == Sex.Female ? "女" : "男";
                AvatarImage.sprite = item.Sex == Sex.Female ? Global.IconDic["women"] : Global.IconDic["man"];
                bgK.sprite = item.Sex == Sex.Female ? Global.IconDic["womenK"] : Global.IconDic["manK"];
            }
        }
        public override void StateDefault()
        {
            ButtonPanel.gameObject.SetActive(false);
        }

        public override void StateSelected()
        {
            ButtonPanel.gameObject.SetActive(true);
        }
    }
}