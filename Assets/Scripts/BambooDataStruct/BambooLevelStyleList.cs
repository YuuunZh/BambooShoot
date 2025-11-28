using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BambooStyleList", menuName = "Bamboo/Bamboo Style List")]
public class BambooLevelStyleList : ScriptableObject
{
    [Header("所有竹筍造型資料")]
    public List<BambooStyleData> AllStyles;

    // 依等級分類的字典 (方便 Manger 腳本快速查找)
    private Dictionary<string, List<BambooStyleData>> _stylesByLevel;

    public Dictionary<string, List<BambooStyleData>> GetStylesByLevel()
    {
        if (_stylesByLevel == null)
        {
            _stylesByLevel = new Dictionary<string, List<BambooStyleData>>()
            {
                {"SSS", new List<BambooStyleData>()},
                {"SSR", new List<BambooStyleData>()},
                {"SR", new List<BambooStyleData>()},
                {"R", new List<BambooStyleData>()},
                {"N", new List<BambooStyleData>()},
                {"F", new List<BambooStyleData>()},
            };

            foreach (var style in AllStyles)
            {
                if (_stylesByLevel.ContainsKey(style.Level))
                {
                    _stylesByLevel[style.Level].Add(style);
                }
            }
        }
        return _stylesByLevel;
    }
}
