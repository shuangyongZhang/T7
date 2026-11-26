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

        public EvalDataGroup EvalDataGroup
        {
            get; private set;
        }

        public INIParser IniParser
        {
            get; private set;
        }

        public async Task ReadConfigData()
        {
            string evalDataPath = FileCopyModule.Instance.GetReadPath(Global.EvalDataGroupPath);
            EvalDataGroup = await FileHelper.ReadSerialConfigData<EvalDataGroup>(evalDataPath);
            string iniPath = FileCopyModule.Instance.GetReadPath(Global.IniParserPath);
            IniParser = FileHelper.InitParser(iniPath);
        }

        public string GetValue(string key, string defaultValue)
        {
            return IniParser.ReadValue(SECTION_NAME, key, defaultValue);
        }
        public void SetValue(string key, string defaultValue)
        {
            IniParser.WriteValue(SECTION_NAME, key, defaultValue);
        }
    }
}