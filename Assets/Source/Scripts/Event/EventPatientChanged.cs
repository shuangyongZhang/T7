using HT.Framework;
namespace LB
{
    /// <summary>
    ///
    /// </summary>
    public sealed class EventPatientChanged : EventHandlerBase
    {
        public PatientInfo PatientInfo;
        /// <summary>
        /// 填充数据，所有属性、字段的初始化工作可以在这里完成
        /// </summary>
        public EventPatientChanged Fill(PatientInfo patientInfo)
        {
            PatientInfo = patientInfo;
            return this;
        }
        /// <summary>
        /// 重置引用，当被引用池回收时调用
        /// </summary>
        public override void Reset()
        {

        }
    }
}