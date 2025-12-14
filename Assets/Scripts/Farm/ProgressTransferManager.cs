using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 跨場景傳遞資料的單例管理器 (使用 DontDestroyOnLoad)。
/// 負責保存 Manger (Day02) 採收到的竹筍清單，供 GradingManager 使用。
/// 也負責處理玩家永久進度 (PlayerProgressData) 的存檔/讀檔。
/// </summary>
public class ProgressTransferManager : MonoBehaviour
{
    // 單例模式 (Singleton Pattern)
    public static ProgressTransferManager Instance { get; private set; }

    // 儲存 Day02 採收到的竹筍造型資料 (臨時資料，用於跨場景傳遞)
    public List<BambooStyleData> Day02HarvestedBamboos = new List<BambooStyleData>();

    // 儲存玩家永久解鎖進度 (用於日記的解鎖狀態)
    public PlayerProgressData PlayerProgress { get; private set; }


    private void Awake()
    {
        // 確保只有一個實例存在
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            // 載入永久進度資料
            LoadPlayerProgress();
        }
    }

    // 清除單次遊戲的資料，適用於新遊戲開始或回到主菜單
    public void ClearSessionData()
    {
        Day02HarvestedBamboos.Clear();
    }

    // 從 PlayerPrefs 載入永久進度
    private void LoadPlayerProgress()
    {
        string json = PlayerPrefs.GetString("PlayerProgress", "{}");
        try
        {
            PlayerProgress = JsonUtility.FromJson<PlayerProgressData>(json);
            // 驗證讀取
            if (PlayerProgress == null) PlayerProgress = new PlayerProgressData();
        }
        catch
        {
            PlayerProgress = new PlayerProgressData();
        }
    }

    // 將永久進度儲存到 PlayerPrefs
    public void SavePlayerProgress(PlayerProgressData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PlayerProgress", json);
        PlayerPrefs.Save();

        Debug.Log("--- [DDoL Manager Save Log] ---");
        Debug.Log($"進度已存檔至 PlayerPrefs。JSON 字串長度: {json.Length}");
        Debug.Log("---------------------------");
    }
}