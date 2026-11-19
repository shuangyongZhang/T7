namespace LB
{
    using System;
    using UIWidgets;


    public class PatientListView : ListViewCustom<PatientItemComponent, PatientItem>
    {

        [NonSerialized]
        bool isListViewInited;

        public virtual int ItemsComparison(PatientItem item1, PatientItem item2)
        {
            return UtilitiesCompare.Compare(PinYinHelper.GetAllChineseLetter(item1.Name), PinYinHelper.GetAllChineseLetter(item2.Name));
        }

        public override void Init()
        {
            if (isListViewInited)
            {
                return;
            }

            isListViewInited = true;

            base.Init();

            ScrollRect.enabled = false;

        }

    }
}