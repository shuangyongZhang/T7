
namespace LB
{
    [System.Serializable]
    public class KeyPortValueData
    {
        public string key;
        public string min;
        public string max;
        public float origin;
    }

    [System.Serializable]
    public class SerialConfigData
    {
        public string name;
        public string type = "key";
        public bool isDegree;
        public KeyPortValueData[] value;
        public float[] levels;
    }
}