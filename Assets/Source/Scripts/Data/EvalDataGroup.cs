namespace LB
{
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
        public bool isFullAxis;
        public int dire;

        public override string ToString()
        {
            return string.Format("体侧: {0}, Full Axis: {1}", cnTitle, isFullAxis);
        }
    }
    public class EvalDataGroup
    {
        public EvalData[] infos;
    }
}
