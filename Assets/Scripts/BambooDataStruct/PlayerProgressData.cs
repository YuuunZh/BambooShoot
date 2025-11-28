using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgressData
{// 儲存所有已解鎖的造型ID
    public HashSet<string> unlockedStyleIDs = new HashSet<string>();

    public void UnlockStyle(string styleID)
    {
        unlockedStyleIDs.Add(styleID);
    }

    public bool IsStyleUnlocked(string styleID)
    {
        return unlockedStyleIDs.Contains(styleID);
    }
}
