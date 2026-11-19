using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    /// <summary>
    /// 
    /// </summary>
    public class AdminItemComponent : ListViewItem, IViewData<AdminItem>
    {

        [SerializeField]
        public TMP_Text IndexText;
        /// <summary>
        /// NameText.
        /// </summary>
        [SerializeField]
        public TMP_Text NameText;

        /// <summary>
        /// SexText.
        /// </summary>
        [SerializeField]
        public Image SexImage;

        [SerializeField]
        public Image ProhibitedSp;


        public AdminItem Item
        {
            get;
            protected set;
        }

        /// <summary>
        /// Set data.
        /// </summary>
        /// <param NameText="item">Item.</param>
        public void SetData(AdminItem item)
        {
            Item = item;
            if (IndexText != null)
                IndexText.text = (item.Index + 1).ToString();
            if (NameText != null)
                NameText.text = item.Account;
            if (SexImage != null)
                SexImage.sprite = item.Sex == Sex.Male ? Global.IconDic["maleDoctor"] : Global.IconDic["femaleDoctor"];
            if (ProhibitedSp != null)
                ProhibitedSp.gameObject.SetActive(item.Prohibited);
        }
    }
}