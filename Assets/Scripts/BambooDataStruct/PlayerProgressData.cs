using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgressData
{
    // 必須使用 [SerializeField] 標記私有欄位，或直接使用 public 欄位。
    [SerializeField]
    private List<string> _unlockedStyleIDs = new List<string>();

    // 請確認您的 IsStyleUnlocked 和 UnlockStyle 方法是操作這個欄位：
    public bool IsStyleUnlocked(string styleID)
    {
        return _unlockedStyleIDs.Contains(styleID);
    }

    public void UnlockStyle(string styleID)
    {
        if (!_unlockedStyleIDs.Contains(styleID))
            _unlockedStyleIDs.Add(styleID);
    }

    // 如果您在 GradingManager 的 Log 中需要取得已解鎖的數量，
    // 請在 PlayerProgressData 中新增一個 public 屬性來獲取這個列表：
    public List<string> GetUnlockedStyles()
    {
        return _unlockedStyleIDs;
    }

}
