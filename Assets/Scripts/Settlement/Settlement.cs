using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.SceneManagement;

public class Settlement : MonoBehaviour
{
    public GameObject Stamp;

    [Header("UI 參考 (請拖入 Contant 物件)")]
    public TextMeshProUGUI numContantText;       // 對應 Num -> Contant
    public TextMeshProUGUI finalSettlementText; // 對應 Final Settlement -> Contant

    private bool _hasSign=false;
    void Start()
    {
        Stamp.SetActive(false);
        DisplaySettlement();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Stamp.SetActive(true);
            _hasSign=true;
        }

        if (_hasSign)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("Go To Home");
                ProgressTransferManager.Instance.ClearSessionData();
                SceneManager.LoadScene("Start");
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("Keep bamboooooo");
                ProgressTransferManager.Instance.ClearSessionData();
                SceneManager.LoadScene("Maimboo");
            }
        }
        
    }

    void DisplaySettlement()
    {
        var data = ProgressTransferManager.Instance;
        if (data == null) return;

        StringBuilder numBuilder = new StringBuilder();
        StringBuilder priceBuilder = new StringBuilder();

        // 強制列出這五個等級
        string[] allLevels = { "SSS", "SSR", "SR", "R", "N" };

        foreach (string lv in allLevels)
        {
            // 獲取數量：如果字典裡沒有這個 Key，則數量為 0
            int count = 0;
            if (data.GradeCountSummary != null && data.GradeCountSummary.ContainsKey(lv))
            {
                count = data.GradeCountSummary[lv];
            }

            // 獲取單價
            int unitPrice = 0;
            if (data.GradePriceMap.ContainsKey(lv))
            {
                unitPrice = data.GradePriceMap[lv];
            }

            int totalForThisLevel = count * unitPrice;

            // 組合字串，即使是 0 也會列出
            numBuilder.AppendLine($"{count}");
            priceBuilder.AppendLine($"$ {totalForThisLevel}");
        }

        // 填入 UI
        if (numContantText != null) numContantText.text = numBuilder.ToString();
        
        // 總報價直接從 DDoL 讀取
        if (finalSettlementText != null)
            finalSettlementText.text = $"$ {data.FinalSettlement}";
    }
}