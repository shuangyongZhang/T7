using HT.Framework;
using UnityEngine;
namespace LB
{
    public class BetaLogoShow : HTBehaviour
    {
        //启用自动化
        protected override bool IsAutomate => true;


    private static BetaLogoShow _instance;

    public static BetaLogoShow Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("BETA_LOGO");
                _instance = go.AddComponent<BetaLogoShow>();
            }

            return _instance;
        }
    }

    private GUIStyle style;

    protected override void Awake()
    {
        if (_instance == null)
            _instance = this;
        DontDestroyOnLoad(this);
    }

    public void Init()
    {
        style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.red;
        style.fontStyle = FontStyle.BoldAndItalic;
        style.fontSize = 35;
    }

        private void OnGUI()
        {
#if UNITY_EDITOR
            if (SerialPortModule.Instance.IsConnected())
                SerialPortModule.Instance.BluetoothHelper.DrawGUI();
#endif
            GUI.Label(new Rect(0, 0, 1920, 100),
                "  BETA_LOGO  BETA_LOGO  BETA_LOGO  BETA_LOGO  BETA_LOGO  BETA_LOGO  BETA_LOGO  BETA_LOGO  ", style);
        }
    }
}
