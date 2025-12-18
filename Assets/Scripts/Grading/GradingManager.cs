using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // 用於 Linq 排序功能
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;

/// <summary>
/// 處理 Grading 場景的邏輯：
/// 1. 讀取 Day02 抽到的所有竹筍造型資料。
/// 2. 處理 UI 互動 (開啟/關閉書本、切換竹筍資料)。
/// 3. 將竹筍資料顯示在書本 UI 上。
/// 
/// 
/// #待辦注意#
/// 離開場景要清空DDoL的List
/// </summary>
public class GradingManager : MonoBehaviour
{
    [Header("【模擬】資料來源 (開發/測試用)")]
    public BambooLevelStyleList StyleListDB;

    // 一個代表未解鎖的 'Unknown' 資料 (請在 Inspector 中設定) ***
    public BambooStyleData UnknownStyleData;


    [Header("分級蓋章")]
    public Image BoxImage;
    public GameObject NoneBamBoo;
    public GameObject FinishUI;
    public Animator BoxAni;
    public Image[] Stamp;
    private int stampIndex;
    private int _gradingIndex = 0;
    // 定義各等級對應的按鍵
    private List<BambooStyleData> _harvestedQueue = new List<BambooStyleData>();
    private Dictionary<string, KeyCode> _gradeKeys = new Dictionary<string, KeyCode> {
        { "SSS", KeyCode.Y }, { "SSR", KeyCode.U }, { "SR", KeyCode.I }, { "R", KeyCode.O }, { "N", KeyCode.P }
    };
    private Dictionary<string, int> _stampAni = new Dictionary<string, int> {
        { "SSS", 0 }, { "SSR", 1}, { "SR",2 }, { "R", 3 }, { "N",4 }
    };


    [Header("UI 設定")]
    public GameObject BookUI; // 書本的 GameObject

    [Header("圖片欄位")]
    public Image BambooStyleImage;  // 竹筍照片 (StyleSprite / StyleForDiary)
    public Image OriginalPlaceImage; // 地方照片 (OriginalPlace)
    public Image GradeImge; // 等級照片 (OriginalPlace)

    [Header("文字欄位 (TextMeshPro)")]
    public TextMeshProUGUI BambooNameText;      // 竹筍名稱 (StyleName)
    public TextMeshProUGUI OriginPlaceNameText; // 產地名稱
    public TextMeshProUGUI Personality;     // 個性介紹
    public TextMeshProUGUI Characteristic;     // 特徵介紹
    public TextMeshProUGUI ReasonShowUp;     // 出現原因
    public TextMeshProUGUI PlaceIntroText;      // 產地介紹

    public TextMeshProUGUI PageNumberText; // 頁碼顯示 (e.g., 1/5)

    public GameObject SwitchTipK; // K 鍵提示
    public GameObject SwitchTipL; // L 鍵提示




    // 儲存從 Manger 讀取到的竹筍資料
    private List<BambooStyleData> _diaryBamboos = new List<BambooStyleData>();
    private int _currentIndex = 0; // 目前顯示的竹筍索引
    private GameObject DDoLObj;

    void Start()
    {
        DDoLObj = GameObject.Find("Manger(DDoL)");
        if (DDoLObj == null)
            Debug.LogError("找不到採收序列");

        // 載入當次採收資料（用於分級，排除 F 等級）
        _harvestedQueue = ProgressTransferManager.Instance.Day02HarvestedBamboos
            .Where(b => b.Level != "F").ToList();

        if (_harvestedQueue == null)
            NoneBamBoo.SetActive(true);
        else NoneBamBoo.SetActive(false);

        FinishUI.SetActive(false);

        ShowCurrentBox();

        // 確保 DDoL Manager 存在
        if (ProgressTransferManager.Instance == null)
        {
            Debug.LogError("ProgressTransferManager 未找到。Grading 場景無法載入資料。");
            return;
        }

        // 預設關閉書本 UI
        if (BookUI != null)
        {
            BookUI.SetActive(false);
        }

        // 1. **【修改】** 讀取日記所需的資料
        LoadDiaryData();


        // 2. 排序竹筍資料：按照 Level (SSS > SSR > SR > R > N > F)
        if (_diaryBamboos.Any())
        {
            // 定義等級順序
            var levelOrder = new List<string> { "SSS", "SSR", "SR", "R", "N", "F" };

            // 根據定義的順序進行排序
            _diaryBamboos = _diaryBamboos
                .OrderBy(b => levelOrder.IndexOf(b.Level))
                .ToList();

            // 初始顯示第一個竹筍
            UpdateBookUI();
        }
        else
        {
            Debug.LogWarning("未讀取到任何已採收的竹筍資料！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadScene("Maimboo");

            DDoLObj.GetComponent<ProgressTransferManager>().Day02HarvestedBamboos.Clear();
        }
        
        //分級
        HandleGradingInput();

        // 處理開啟/關閉書本
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleBookUI();
        }

        if (BookUI != null && BookUI.activeSelf && _diaryBamboos.Count > 0)
        {
            // 處理切換竹筍內容
            if (Input.GetKeyDown(KeyCode.K)) // 往右翻
            {
                _currentIndex++;
                if (_currentIndex >= _diaryBamboos.Count)
                {
                    _currentIndex = 0; // 循環到第一個
                }
                UpdateBookUI();
            }
            else if (Input.GetKeyDown(KeyCode.J)) // 往左翻
            {
                _currentIndex--;
                if (_currentIndex < 0)
                {
                    _currentIndex = _diaryBamboos.Count - 1; // 循環到最後一個
                }
                UpdateBookUI();
            }
        }
    }

    // --- 分級邏輯區 ---------------------------------------------------------//
    private void ShowCurrentBox()
    {
        BoxImage.sprite = _harvestedQueue[_gradingIndex].BoxSprite; // 需在 Data 新增此欄位
        BoxAni.SetTrigger("SlideIn");
    }

    //分級判斷
    private void HandleGradingInput()
    {
        if (_gradingIndex >= _harvestedQueue.Count) return;

        string reqLevel = _harvestedQueue[_gradingIndex].Level;
        // 1. 確認字典裡有這個等級的定義
        if (_gradeKeys.ContainsKey(reqLevel) && _stampAni.ContainsKey(reqLevel))
        {
            // 2. 檢查玩家是否按下對應的 KeyCode
            if (Input.GetKeyDown(_gradeKeys[reqLevel]))
            {
                // 3. 取得該等級對應的整數索引 (0-4)
                stampIndex = _stampAni[reqLevel];

                // 4. 根據索引啟動對應的 Image
                Stamp[stampIndex].gameObject.SetActive(true);

                StartCoroutine(ProcessStampSequence(reqLevel));
                
            }
        }
    }

    IEnumerator ProcessStampSequence(string level)
    {
        yield return new WaitForSeconds(1.5f);

        Stamp[stampIndex].gameObject.SetActive(false);
        BoxAni.SetTrigger("SlideOut");

        yield return new WaitForSeconds(1.5f);

        BoxAni.SetTrigger("Idel");
        // 3. 切換下一個或結束
        _gradingIndex++;
        if (_gradingIndex < _harvestedQueue.Count)
        {
            ShowCurrentBox();
        }
        else
        {
            StartCoroutine(FinishGrading());

        }
    }

    IEnumerator FinishGrading()
    {
        Debug.Log("分級完成，已切換至圖鑑模式。按下 L 可開啟書本。");
        FinishUI.SetActive(true);
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Maimboo");

        //刪一下序列
        DDoLObj.GetComponent<ProgressTransferManager>().Day02HarvestedBamboos.Clear();

    }
    //-------------------------------------------------------------------------//


    //---日記功能-----------------------------------------------------//
    /// <summary>
    /// 切換書本 UI 的顯示狀態
    /// </summary>
    private void ToggleBookUI()
    {
        if (BookUI != null)
        {
            BookUI.SetActive(!BookUI.activeSelf);
        }
    }

    /// <summary>
    /// 根據當前索引更新書本 UI 的顯示內容
    /// </summary>
    private void UpdateBookUI()
    {
        if (_diaryBamboos.Count == 0)
        {
            BookUI.SetActive(false);
            return;
        }

        // 確保索引在範圍內
        _currentIndex = Mathf.Clamp(_currentIndex, 0, _diaryBamboos.Count - 1);
        BambooStyleData currentBamboo = _diaryBamboos[_currentIndex];
        // 圖片更新 (Image)
        if (BambooStyleImage != null)
        {
            // 優先使用 StyleForDiary 的圖片，若沒有則使用 StyleSprite
            Sprite styleSprite = currentBamboo.StyleForDiary != null ? currentBamboo.StyleForDiary : currentBamboo.StyleSprite;
            BambooStyleImage.sprite = styleSprite;
        }

        // 使用 OriginalPlace 圖片
        if (OriginalPlaceImage != null)
            OriginalPlaceImage.sprite = currentBamboo.OriginalPlace;
        
        if (GradeImge != null)
            GradeImge.sprite = currentBamboo.Grade;

        // 文字更新 (TextMeshProUGUI)
        if (BambooNameText != null)
            BambooNameText.text = currentBamboo.StyleName;

        if (Personality != null)
            Personality.text = currentBamboo.Personality;
        
        if (Characteristic != null)
            Characteristic.text = currentBamboo.Characteristic;
        
        if (ReasonShowUp != null)
            ReasonShowUp.text = currentBamboo.ReasonShowUp;

        // 產地欄位
        if (OriginPlaceNameText != null)
            OriginPlaceNameText.text = currentBamboo.Breif;

        if (PlaceIntroText != null)
            PlaceIntroText.text = currentBamboo.PlaceIntro;

        // 頁碼更新
        if (PageNumberText != null)
            PageNumberText.text = $"{_currentIndex + 1} / {_diaryBamboos.Count}";
        // 更新 K/L 提示 (如果只有一個竹筍，則隱藏提示)
        bool showTips = _diaryBamboos.Count > 1;
        SwitchTipK?.SetActive(showTips);
        SwitchTipL?.SetActive(showTips);
    }


    private void LoadDiaryData()
    {
        if (StyleListDB == null)
        {
            Debug.LogError("請在 Inspector 中拖曳 BambooLevelStyleList ScriptableObject 到 GradingManager！");
            return;
        }

        if (UnknownStyleData == null)
        {
            Debug.LogError("請在 Inspector 中設定 UnknownStyleData！");
            return;
        }

        // ***【修改點 1: 從 DDoL Manager 取得玩家進度和已採收的竹筍資料】***
        if (ProgressTransferManager.Instance == null)
        {
            Debug.LogError("ProgressTransferManager 未載入！");
            return;
        }

        // 取得玩家永久進度 (已解鎖的竹筍列表)
        PlayerProgressData progress = ProgressTransferManager.Instance.PlayerProgress;

        // 1. 取得所有竹筍資料 (去重)
        var allStyles = StyleListDB.AllStyles;


        _diaryBamboos.Clear();

        // 3. 遍歷所有可能的竹筍
        foreach (var style in allStyles)
        {
            if (progress.IsStyleUnlocked(style.StyleID))
            {
                // 已解鎖：使用原始資料
                _diaryBamboos.Add(style);
            }
            else
            {
                // 未解鎖：使用 Unknown 替換資料，但保留原始 StyleID/Level 方便排序
                BambooStyleData unknownEntry = ScriptableObject.Instantiate(UnknownStyleData);
                // 必須保留 StyleID 和 Level，才能排序或未來從日記中找到原始資料
                unknownEntry.StyleID = style.StyleID;
                unknownEntry.Level = style.Level;

                _diaryBamboos.Add(unknownEntry);
            }
        }

        Debug.Log($"日記載入完成，共 {_diaryBamboos.Count} 筆資料 (包含 Unknown)。");
        Debug.Log($" GradingManager 確認永久進度：從 DDoL 載入，共 {ProgressTransferManager.Instance.Day02HarvestedBamboos.Count} 個竹筍在 Day02 被採集。");

    }

}


