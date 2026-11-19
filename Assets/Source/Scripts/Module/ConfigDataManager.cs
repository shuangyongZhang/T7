using System.Linq;
using System.Threading.Tasks;
using HT.Framework;

namespace LB
{
    [CustomModule("ConfigDataManager", true)]
    public class ConfigDataManager : CustomModuleBase
    {
        private static ConfigDataManager _instance;
        public static ConfigDataManager Instance
        {
            get
            {
                _instance ??= Main.m_CustomModule["ConfigDataManager"].Cast<ConfigDataManager>();
                return _instance;
            }
        }

        public const string SECTION_NAME = "PZConf";
        public SerialConfigData SerialConfigData
        {
            get; private set;
        }

        public EvalDataGroup EvalDataGroup
        {
            get; private set;
        }

        public INIParser IniParser
        {
            get; private set;
        }
        public EvalData evalData;

        public bool IsCompensation
        {
            get => SaveConfigModule.Instance.GetUserData<bool>("compensation", false);
        }

        public async Task ReadConfigData()
        {
            string keyInputPath = FileCopyModule.Instance.GetReadPath(Global.KeyInputConfigPath);
            SerialConfigData = await FileHelper.ReadSerialConfigData<SerialConfigData>(keyInputPath);
            SerialPortModule.Instance.SetMessageData(SerialConfigData);

            string evalDataPath = FileCopyModule.Instance.GetReadPath(Global.EvalDataGroupPath);
            EvalDataGroup = await FileHelper.ReadSerialConfigData<EvalDataGroup>(evalDataPath);
            string iniPath = FileCopyModule.Instance.GetReadPath(Global.IniParserPath);
            IniParser = FileHelper.InitParser(iniPath);
            // GameSetting.ReadConfig();

            string cnTitle = GetValue("体侧", string.Empty);
            evalData = GetConfigDataFromTitle(EvalDataGroup, cnTitle);

#if UNITY_STANDALONE_WIN 
            // GameSetting.SetScreenDisplay();
#endif
            // IsCompensation = SaveConfigModule.Instance.GetUserData<bool>("compensation", false);
        }

        public string GetValue(string key, string defaultValue)
        {
            return IniParser.ReadValue(SECTION_NAME, key, defaultValue);
        }
        public void SetValue(string key, string defaultValue)
        {
            IniParser.WriteValue(SECTION_NAME, key, defaultValue);
        }
        private EvalData GetConfigDataFromTitle(EvalDataGroup group, string cnTitle)
        {
            return group.infos.FirstOrDefault(d => d.cnTitle == cnTitle);
        }
        public void SetEvalData(string cnTitle)
        {
            evalData = GetConfigDataFromTitle(EvalDataGroup, cnTitle);
            SerialPortModule.Instance.isReverse = evalData.reverse;
        }
    }
}