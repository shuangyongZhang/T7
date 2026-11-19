using HT.Framework;
namespace LB
{
    /// <summary>
    /// 代偿开关改变
    /// </summary>
    public sealed class EventChangeCompensation : EventHandlerBase
    {
        //代偿开关状态
        public bool IsCompensation;

        /// <summary>
        /// 填充数据，所有属性、字段的初始化工作可以在这里完成
        /// </summary>
        public EventChangeCompensation Fill(bool isCompensation)
        {
            IsCompensation = isCompensation;
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