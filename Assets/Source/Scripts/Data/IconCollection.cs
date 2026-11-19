using System.Collections.Generic;
using UnityEngine;
using HT.Framework;
using System;
/// <summary>
/// 新建数据集
/// </summary>
[Serializable]
[CreateAssetMenu(menuName = "HTFramework DataSet/IconCollection")]
public class IconCollection : DataSetBase
{
    /// <summary>
    /// 图标集合
    /// </summary>
    public List<IconCollectionData> Icons = new List<IconCollectionData>();

    [Button("设置图标名称")]
    public void SetIconName()
    {
        for (int i = 0; i < Icons.Count; i++)
        {
            Icons[i].Name = Icons[i].Sprite.name;
        }
    }

    /// <summary>
    /// 通过Json数据填充数据集
    /// </summary>
    public override void Fill(JsonData data)
    {
        base.Fill(data);
    }
    /// <summary>
    /// 将数据集打包为Json数据
    /// </summary>
    public override JsonData Pack()
    {
        JsonData data = new JsonData();
        return data;
    }
}
[Serializable]
public class IconCollectionData
{
    public string Name;
    public Sprite Sprite;
}