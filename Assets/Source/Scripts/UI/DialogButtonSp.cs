using HT.Framework;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.UI;
namespace LB
{
    public class DialogButtonSp : HTBehaviour
    {
        //启用自动化
        protected override bool IsAutomate => true;

        public Sprite confirmSp;
        public Sprite cancelSp;
        public Image btnImg;
        public BetterTextMeshProUGUI btnText;

        public void OnEnable()
        {
            if (btnText.text == "确定")
            {
                btnImg.gameObject.SetActive(true);
                btnImg.sprite = confirmSp;
            }
            else if (btnText.text == "取消")
            {
                btnImg.gameObject.SetActive(true);
                btnImg.sprite = cancelSp;
            }
            else
            {
                btnImg.gameObject.SetActive(false);
            }
        }
    }
}
