using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 竹筍本人與狀態的顯示與動作
/// 主遊戲的狀態顯示與讀條
/// 竹筍跳澆水&施肥
/// 
/// ------------未做------------
/// 竹筍跨日採收
/// 不同竹筍的生成抽獎
/// 醜損判定
/// 遊戲暫停
/// 青農日記
/// 結算日
/// 
/// </summary>

public class Manger : MonoBehaviour
{
    [System.Serializable]
    public class bambooData  //竹筍資料
    {
        [Header("狀態出現&圖示修改")]
        public int timeToWatered = 3, timeToSpread = 3 , bambooFormIndex = 0;
        public SpriteRenderer bambooForm;

        [Header("條件狀態")]
        public bool hasWatered =false;     //判斷玩家有無做到
        public bool requiredWatered =false;   //判斷程式是否到此階段
        public bool isWatering = false;        //澆水階段中
        public bool hasSpread = false;
        public bool requiredSpread = false;
        public bool isSpreaing = false;
        public bool hasHarvest =false;  //爲未來要新增清晨採收暫留
        public bool stateIsFilling = false;    //避免操作時間一直去觸發協程

        [Header("操作空間設定抓取")]
        public GameObject Bamboo;
        public KeyCode key;
        public Image StateSpRender,OutlineFill;   //狀態列相關空間
        //public float timeCunt=0;

    }

    [System.Serializable]
    public class LevelData
    {
        public string Level;
        public Sprite Type;
    }


    /*---------------------------*/
    [Header("竹筍資料庫與進度追蹤")]
    public BambooLevelStyleList StyleListDB; // 拖曳 BambooStyleList ScriptableObject 進來
    private PlayerProgressData _playerProgress;
    /*---------------------------*/


    public List<bambooData> Bamboos = new List<bambooData>();
    public Sprite[] bambooSpForm,stateSpForm;
    public GameObject canvas;
    public Image Daytimer;
    public bool Day01=true,Day02=false;

    
    public float countTime = 0, Day01Time = 15;    //Day01Time:第一天只給15秒動作
    public bool Day01HadCover=false;

    int waitTime = 3,HarvestTime=8;   //waitTime:每個澆水&施肥動作只給3秒 ; HarvestTime:採收只給8秒動作
    float bambooNum,HarvestDay;
    bool gameStop=false, hasFilled = false;    //hasFilled:UIStateFill

    void Awake()
    {
        // 載入玩家進度 (請自行實現 LoadProgressFromSaveFile 或 PlayerPrefs)
        _playerProgress = LoadPlayerProgress();
    }


    void Start()
    {

        bambooNum = gameObject.transform.childCount;

        //物件基本設定抓取。資料初始化
        for (int i = 0; i < bambooNum; i++)
        {
            Bamboos[i].Bamboo = transform.GetChild(i).gameObject;  //抓竹筍本人物件
            Bamboos[i].bambooForm = transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>();  //抓竹筍本人的Sp
            Bamboos[i].StateSpRender = canvas.transform.GetChild(i).
                gameObject.GetComponent<Image>();  //抓竹筍子物件的狀態
            Bamboos[i].OutlineFill = canvas.transform.GetChild(i).GetChild(0).
                gameObject.GetComponent<Image>();  //抓竹筍子物件的狀態填充
            Bamboos[i].timeToWatered = UnityEngine.Random.Range(3, 6);
            Bamboos[i].timeToSpread = UnityEngine.Random.Range(1, 4);
            Bamboos[i].timeToSpread += waitTime + Bamboos[i].timeToWatered;   //真正施肥時間
            Bamboos[i].bambooForm.sprite = bambooSpForm[0];
        }


        //關一下
        for (int i=0 ; i<canvas.transform.childCount;i++)   
        {
            canvas.transform.GetChild(i).gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStop)   //Game is running
        {
            countTime += Time.deltaTime;   //各筍子計時
            

            if (Day01)     //Dau01 竹筍澆水施肥動作
            {
                StartCoroutine(Day01Timer(Daytimer));
                for (int i = 0; i < bambooNum; i++)
                {

                    #region WaterProcess
                    //要求澆水
                    if (!Bamboos[i].requiredWatered &&
                        countTime >= Bamboos[i].timeToWatered)
                    {
                        Bamboos[i].requiredWatered = true;
                        Bamboos[i].StateSpRender.sprite = stateSpForm[0];
                        Bamboos[i].OutlineFill.sprite = stateSpForm[0];
                        Bamboos[i].StateSpRender.gameObject.SetActive(true);
                    }

                    //操作時間
                    if (Bamboos[i].requiredWatered &&
                        countTime - (Bamboos[i].timeToWatered + waitTime) < 0)
                    {
                        Bamboos[i].isWatering = true;

                        if (!Bamboos[i].stateIsFilling)
                        {
                            Bamboos[i].stateIsFilling = true;
                            StartCoroutine(StateFill(Bamboos[i], Bamboos[i].OutlineFill));
                        }


                        if (Input.GetKeyDown(Bamboos[i].key) && !Bamboos[i].hasWatered)   //澆水了
                        {
                            Bamboos[i].hasWatered = true;
                            Bamboos[i].StateSpRender.gameObject.SetActive(false);
                            Bamboos[i].bambooFormIndex += 1;
                            Bamboos[i].bambooForm.sprite = bambooSpForm[Bamboos[i].bambooFormIndex];
                        }

                    }

                    //Operate time over
                    if (Bamboos[i].requiredWatered &&
                        countTime - (Bamboos[i].timeToWatered + waitTime) > 0)
                        Bamboos[i].isWatering = false;
                    #endregion



                    #region SpreadProcess
                    //要求施肥
                    if (!Bamboos[i].requiredSpread &&
                        countTime >= Bamboos[i].timeToSpread)
                    {
                        Bamboos[i].requiredSpread = true;
                        Bamboos[i].StateSpRender.sprite = stateSpForm[1];
                        Bamboos[i].OutlineFill.sprite = stateSpForm[1];
                        Bamboos[i].StateSpRender.gameObject.SetActive(true);
                        hasFilled = false;
                    }

                    //操作時間
                    if (Bamboos[i].requiredSpread &&
                        countTime - (Bamboos[i].timeToSpread + waitTime) < 0)
                    {
                        Bamboos[i].isSpreaing = true;
                        if (!Bamboos[i].stateIsFilling)
                        {
                            Bamboos[i].stateIsFilling = true;
                            StartCoroutine(StateFill(Bamboos[i], Bamboos[i].OutlineFill));
                        }
                        if (Input.GetKeyDown(Bamboos[i].key) && !Bamboos[i].hasSpread)   //施肥了
                        {
                            Bamboos[i].hasSpread = true;
                            Bamboos[i].StateSpRender.gameObject.SetActive(false);
                            Bamboos[i].bambooFormIndex += 1;
                            Bamboos[i].bambooForm.sprite = bambooSpForm[Bamboos[i].bambooFormIndex];
                        }
                    }

                    //Operate time end
                    if (Bamboos[i].requiredSpread &&
                        countTime - (Bamboos[i].timeToSpread + waitTime) > 0) { }
                    Bamboos[i].isSpreaing = false;
                    #endregion

                }

            }


            if (Day02)   
            {
                HarvestDay = countTime + HarvestTime;     //時間內不採會醜掉
                StartCoroutine(Day02Timer(Daytimer));    //該日計時
                for(int i=0; i < bambooNum; i++)
                {
                    // 進入 Day02 時，先進行抽獎判定
                    // 為了避免重複抽選，我們使用 hasWatered 旗標作為 Day02 初始化的標記
                    if (!Bamboos[i].hasWatered)
                    {
                        // 執行抽獎，並取得抽到的竹筍造型資料
                        BambooStyleData harvestedStyle = PerformLottery(Bamboos[i]);

                        if (harvestedStyle != null)
                        {
                            // **設定竹筍的最終造型！**
                            Bamboos[i].bambooForm.sprite = harvestedStyle.StyleSprite;

                            // 使用 hasWatered 來標記該竹筍已完成 Day02 的初始抽獎，避免重複抽
                            Bamboos[i].hasWatered = true;
                        }
                        else
                        {
                            // 如果抽獎失敗，設一個預設或錯誤造型
                            Bamboos[i].bambooForm.sprite = bambooSpForm[0];
                        }
                    }

                    if (!Bamboos[i].hasHarvest && Input.GetKeyDown(Bamboos[i].key))
                    {
                        Bamboos[i].hasHarvest=true;
                    }
                }


            }

        }
    }

    /*
    string bambooLevel(bambooData bamboo)
    {
        if (!Day01HadCover)
        {
            return "F";
        }

        bool bothDone = bamboo.hasWatered && bamboo.hasSpread;
        int lottery = Random.Range(1, 101);

        if (bothDone)
        {
            if (lottery <= 5)
                return "SSS";
            else if (lottery <= 15)
                return "SSR";
            else if (lottery <= 50)
                return "SR";
            else if (lottery <= 90)
                return "R";
            return "N";
        }
        else
        {
            if (lottery <= 5)
                return "SR";
            else if (lottery <= 35)
                return "R";
            else if (lottery <= 85)
                return "N";
            return "F";
            
        }

    }

    */

    /*-----------------------------------------------------------------------------*/

    public BambooStyleData PerformLottery(bambooData bamboo)
    {
        string level;
        // 1. 判斷竹筍等級
        if (!Day01HadCover)
        {
            // Day01HadCover 判斷為 Day01 結束時是否成功完成所有竹筍的動作
            level = "F";
        }
        else
        {
            bool bothDone = bamboo.hasWatered && bamboo.hasSpread;
            int lottery = UnityEngine.Random.Range(1, 101);

            if (bothDone) // 澆水和施肥都做了
            {
                if (lottery <= 5) level = "SSS";
                else if (lottery <= 15) level = "SSR";
                else if (lottery <= 50) level = "SR";
                else if (lottery <= 90) level = "R";
                else level = "N";
            }
            else // 至少一項沒做 (或都沒做)
            {
                if (lottery <= 5) level = "SR";
                else if (lottery <= 35) level = "R";
                else if (lottery <= 85) level = "N";
                else level = "F";
            }
        }

        // 2. 根據等級，從資料庫中隨機抽出一個造型 (抽獎)
        Dictionary<string, List<BambooStyleData>> stylesByLevel = StyleListDB.GetStylesByLevel();

        if (stylesByLevel.ContainsKey(level) && stylesByLevel[level].Count > 0)
        {
            List<BambooStyleData> availableStyles = stylesByLevel[level];
            int randomIndex = UnityEngine.Random.Range(0, availableStyles.Count);
            BambooStyleData selectedStyle = availableStyles[randomIndex];

            // 3. 檢查並解鎖造型 (更新玩家進度)
            if (!_playerProgress.IsStyleUnlocked(selectedStyle.StyleID))
            {
                _playerProgress.UnlockStyle(selectedStyle.StyleID);
                Debug.Log($"🎉 Day02 發現新造型! 等級:{level}, 造型名稱: {selectedStyle.StyleName}");
                // 注意：存檔會在遊戲關閉/暫停時自動執行 (根據前一次討論的 OnApplicationQuit/OnApplicationPause)
            }
            else
            {
                Debug.Log($"Day02 抽到已解鎖造型: {selectedStyle.StyleName}");
            }

            return selectedStyle;
        }
        else
        {
            Debug.LogError($"無法找到等級 {level} 的竹筍造型，請檢查 BambooStyleList 設定。");
            return null; // 返回 null 避免錯誤
        }
    }

    /*-----------------------------------------------------------------------------*/


    int bambooLottery(string level)
    {
        int roll = Random.Range(1, 101);
        switch (level)
        {
            case "SSS":

                break;
            case "SSR":
                break;
            case "SR":
                break;
            case "R":
                break;
            case "N":
                break;
            case "F":
                break;
        }
        return 0;
    }




    //目前遊戲暫停UIfill會出bug
    IEnumerator StateFill(bambooData bamboo ,Image fillSp)       //StateUI_3S_UIFill
    {
        float elapsed = 0f;   //passedTime
        fillSp.fillAmount = 0f;  //startValue


        while (elapsed < waitTime && !gameStop)
        {
            if(!Day01)
                break;
            elapsed += Time.deltaTime;
            fillSp.fillAmount = Mathf.Clamp01(elapsed / waitTime);
            yield return null;
        }

        fillSp.fillAmount = 1f;    //confirmTheEndValueIs01
        fillSp.gameObject.transform.parent.gameObject.SetActive(false);
        bamboo.stateIsFilling=false;
        //print("close");
    }

    IEnumerator Day01Timer(Image timer)
    {
        float elapsed = 0f;   //passedTime
        timer.fillAmount = 0f;  //startValue

        while (elapsed < Day01Time && !gameStop)
        {
            if (!Day01)
                break;
            elapsed += Time.deltaTime;
            timer.fillAmount = Mathf.Clamp01(elapsed / Day01Time);
            yield return null;
        }

        timer.fillAmount = 1f;

    }

    IEnumerator Day02Timer(Image timer)
    {
        float elapsed = 0f;   //passedTime
        timer.fillAmount = 0f;  //startValue

        while (elapsed < HarvestTime && !gameStop)
        {
            if (!Day02)
                break;
            elapsed += Time.deltaTime;
            timer.fillAmount = Mathf.Clamp01(elapsed / HarvestTime);
            yield return null;
        }

        timer.fillAmount = 1f;

    }


    /*--------------------------------------------------------------------------*/
    // 用來處理抽獎和解鎖的方法
    public BambooStyleData PerformLotteryAndUnlock(string level)
    {
        // 1. 取得該等級所有造型
        var styles = StyleListDB.GetStylesByLevel()[level];

        if (styles == null || styles.Count == 0)
        {
            Debug.LogError($"Level {level} has no defined styles.");
            return null;
        }

        // 2. 隨機抽出一個造型
        int randomIndex = Random.Range(0, styles.Count);
        BambooStyleData selectedStyle = styles[randomIndex];

        // 3. 判斷是否為新解鎖，並更新進度
        bool isNewUnlock = !_playerProgress.IsStyleUnlocked(selectedStyle.StyleID);

        if (isNewUnlock)
        {
            _playerProgress.UnlockStyle(selectedStyle.StyleID);
            // 儲存玩家進度 (請自行實現 SaveProgressToSaveFile 或 PlayerPrefs)
            SavePlayerProgress(_playerProgress);

            // 可以觸發UI顯示 "New Unlock!"
            Debug.Log($"🎉 新造型解鎖: {selectedStyle.StyleName}");
        }
        else
        {
            Debug.Log($"已獲得造型: {selectedStyle.StyleName}");
        }

        return selectedStyle;
    }

    /*
    // 假設您在 Day02 採收時呼叫此方法
    void Day02HarvestLogic(bambooData bamboo)
    {
        string level = bambooLevel(bamboo); // 呼叫您既有的等級判定
        BambooStyleData harvestedStyle = PerformLotteryAndUnlock(level);

        // 使用 harvestedStyle 的資料來更新竹筍顯示或進入結算畫面
        if (harvestedStyle != null)
        {
            // 將竹筍 Sprite 設為抽到的造型
            bamboo.bambooForm.sprite = harvestedStyle.StyleSprite;
        }

        // ... (其他 Day02 邏輯)
    }
    */
    // 存檔/讀檔的 Placeholder (您需要根據您的專案選擇實現方式)
    private PlayerProgressData LoadPlayerProgress()
    {
        // 範例：從 PlayerPrefs 讀取 JSON 字符串
        string json = PlayerPrefs.GetString("PlayerProgress", "{}");
        try
        {
            return JsonUtility.FromJson<PlayerProgressData>(json);
        }
        catch
        {
            return new PlayerProgressData();
        }
    }
    private void SavePlayerProgress(PlayerProgressData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PlayerProgress", json);
        PlayerPrefs.Save();
    }
    /*--------------------------------------------------------------------------*/

}
