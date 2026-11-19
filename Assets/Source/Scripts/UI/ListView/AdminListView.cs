using System;
using UIWidgets;

namespace LB
{
    /// <summary>
    /// AdminGroupedListView.
    /// </summary>
    public class AdminListView : ListViewCustom<AdminItemComponent, AdminItem>
    {
        [NonSerialized]
        bool isListViewInited;


        /// <summary>
        /// Items comparison.
        /// </summary>
        /// <param name="item1">First AdminItem.</param>
        /// <param name="item2">Second AdminItem.</param>
        /// <returns>Comparison result.</returns>
        public virtual int ItemsComparison(AdminItem item1, AdminItem item2)
        {
            return UtilitiesCompare.Compare(item1.Name, item2.Name);
        }

        public override void Init()
        {
            if (isListViewInited)
            {
                return;
            }

            isListViewInited = true;

            base.Init();

        }
    }
}