namespace LB
{

    public enum ValueAxis
    {
        xyz,
        yzx,
        zxy
    }
    [System.Serializable]
    public struct EvalData
    {
        public string cnTitle;
        public string animName;
        public string FilterAnimName
        {
            get
            {
                string[] retval = animName.Split('_');
                return retval[0];
            }
        }
        public ValueAxis valueAxis;
        public bool reverse;
        public bool isFullAxis;
        public int dire;

        public override string ToString()
        {
            return string.Format("体侧: {0}, Axis: {1}, Full Axis: {2}", cnTitle, valueAxis, isFullAxis);
        }
    }
    public class EvalDataGroup
    {
        public EvalData[] infos;
    }
}
