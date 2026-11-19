using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HT.Framework;
using UniRx;
using UnityEngine;

namespace LB
{

    //App 数据集管理
    [CustomModule("DataSetManager", true)]
    public class DataSetManager : CustomModuleBase
    {
        private static DataSetManager _instance;
        public static DataSetManager Instance
        {
            get
            {
                _instance ??= Main.m_CustomModule["DataSetManager"].Cast<DataSetManager>();
                return _instance;
            }
        }


        public List<IconCollectionData> Icons = new List<IconCollectionData>();


        /// <summary>
        /// 初始化模块
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

        }
        /// <summary>
        /// 模块准备工作
        /// </summary>
        public override void OnReady()
        {
            base.OnReady();


            IsRunning = false;
        }
        public async UniTask LoadDataSetAsync()
        {
            //加载常用图标集合
            var data = await Main.m_Resource.LoadAsset<IconCollection>("IconCollection");
            // Main.m_DataSet.AddDataSet(data);
            Global.IconDic.Clear();
            foreach (var item in data.Icons)
            {
                Global.IconDic.Add(item.Name, item.Sprite);
            }
            // Main.m_DataSet.RemoveDataSet(data);
        }

        /// <summary>
        /// 刷新模块
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        /// <summary>
        /// 终结模块
        /// </summary>
        public override void OnTerminate()
        {
            base.OnTerminate();
        }

        /// <summary>
        /// 暂停模块
        /// </summary>
        public override void OnPause()
        {
            base.OnPause();
        }
        /// <summary>
        /// 恢复模块
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();
        }

    }

}
