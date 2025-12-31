using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// 竹筍本人與狀態的顯示與動作
/// 主遊戲的狀態顯示與讀條
/// 竹筍跳澆水&施肥
/// 不同竹筍的生成抽獎
/// 醜損判定
/// 竹筍跨日採收
/// 去分級
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
        public bool Day02Initialized = false;   //第2日初始化，是否抽獎

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
    
    /*---------------------------*/


    public List<bambooData> Bamboos = new List<bambooData>();
    public Sprite[] bambooSpForm,stateSpForm;
    public GameObject canvas;
    public Image Daytimer;
    public bool Day01=true,Day02=false;
    public string NextScene;



    public float countTime = 0, Day01Time = 15;    //Day01Time:第一天只給15秒動作
    public bool Day01HadCover=false;
    public Animator ToGtadingAni;

    int waitTime = 3,HarvestTime=8;   //waitTime:每個澆水&施肥動作只給3秒 ; HarvestTime:採收只給8秒動作
    float bambooNum,HarvestDay;
    bool gameStop=false, hasFilled = false;    //hasFilled:UIStateFill

    
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
                Daytimer.gameObject.SetActive(false);
                //StartCoroutine(Day02Timer(Daytimer));    //該日計時
                for (int i=0; i < bambooNum; i++)
                {
                    // 進入 Day02 時，先進行抽獎判定
                    if (!Bamboos[i].Day02Initialized)
                    {
                        // 執行抽獎，並取得抽到的竹筍造型資料
                        BambooStyleData harvestedStyle = PerformLottery(Bamboos[i]);

                        if (harvestedStyle != null)
                        {
                            // **設定竹筍的最終造型！**
                            Bamboos[i].bambooForm.sprite = harvestedStyle.StyleSprite;


                            // 使用 hasWatered 來標記該竹筍已完成 Day02 的初始抽獎，避免重複抽
                            Bamboos[i].Day02Initialized = true;
                        }
                        else
                        {
                            // 如果抽獎失敗，設一個預設或錯誤造型
                            Bamboos[i].bambooForm.sprite = bambooSpForm[0];
                            Bamboos[i].Day02Initialized = true;
                        }
                    }

                    if (!Bamboos[i].hasHarvest && Input.GetKeyDown(Bamboos[i].key))
                    {
                        Bamboos[i].hasHarvest=true;

                        // 【修改】：取消直接隱藏，改為啟動動畫協程
                        StartCoroutine(HarvestAnimation(
                            Bamboos[i].Bamboo,        // 竹筍的 GameObject
                            Bamboos[i].bambooForm     // 竹筍的 SpriteRenderer
                        ));

                        // 隱藏 State UI，表示該竹筍已完成採收
                        // 由於 State UI 和竹筍是不同的物件，UI 可以直接隱藏
                        Bamboos[i].StateSpRender.gameObject.SetActive(false);
                    }
                }

                if (IsHarvestComplete() && Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Day02 採收完畢，切換到下一個場景: " + NextScene);
                    // 執行場景切換
                    StartCoroutine(TransitionToGrading());
                }

            }

        }
    }


    /*-----------------------------------------------------------------------------*/

    public BambooStyleData PerformLottery(bambooData bamboo)
    {
        string level;
        // 確保 DDoL Manager 存在
        if (ProgressTransferManager.Instance == null)
        {
            Debug.LogError("ProgressTransferManager 未找到。無法進行抽獎和存檔。");
            return null;
        }

        // 從 DDoL 實例取得永久進度
        PlayerProgressData playerProgress = ProgressTransferManager.Instance.PlayerProgress;

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
                else if (lottery <= 40) level = "SR";
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
            if (!playerProgress.IsStyleUnlocked(selectedStyle.StyleID))
            {
                playerProgress.UnlockStyle(selectedStyle.StyleID);
                Debug.Log($"🎉 Day02 發現新造型! 等級:{level}, 造型名稱: {selectedStyle.StyleName}");
                // 注意：存檔會在遊戲關閉/暫停時自動執行 (根據前一次討論的 OnApplicationQuit/OnApplicationPause)
                
                // 新增 DDoL Manager 存檔呼叫
                ProgressTransferManager.Instance.SavePlayerProgress(playerProgress);

            }
            else
            {
                Debug.Log($"Day02 抽到已解鎖造型: {selectedStyle.StyleName}");
            }

            //  寫入 DDoL Manager 進行跨場景傳遞
            ProgressTransferManager.Instance.Day02HarvestedBamboos.Add(selectedStyle);
            return selectedStyle;
        }
        else
        {
            Debug.LogError($"無法找到等級 {level} 的竹筍造型，請檢查 BambooStyleList 設定。");
            return null; // 返回 null 避免錯誤
        }
    }

    /*-----------------------------------------------------------------------------*/



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
    
    

    // Day02 開始時，設定 UI 為採收模式。
    public void StartDay02HarvestUI()
    {
        // 設定所有竹筍的狀態 UI
        for (int i = 0; i < bambooNum; i++)
        {
            // 1. State 的 UI 出現
            Bamboos[i].StateSpRender.gameObject.SetActive(true);

            // 2. stateSpForm要是第3個 (stateSpForm[2])
            Bamboos[i].StateSpRender.sprite = stateSpForm[2];
            Bamboos[i].OutlineFill.sprite = stateSpForm[2];

            // 確保 Day02 狀態列的填充條為滿
            Bamboos[i].OutlineFill.fillAmount = 0f;
        }
    }

    IEnumerator HarvestAnimation(GameObject bambooObject, SpriteRenderer spriteRenderer, float moveDistance = 1.0f, float duration = 0.5f)
    {
        Vector3 startPosition = bambooObject.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * moveDistance;
        Color startColor = spriteRenderer.color;
        float startTime = Time.time;

        // 動畫迴圈
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;

            // 1. 位移：使用 Ease Out 效果讓移動更自然（t*t*t*t 可以替換成 t*t*t 甚至 Linear (t)）
            bambooObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // 2. 淡出：調整 SpriteRenderer 的 alpha 值
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = newColor;

            yield return null; // 等待下一幀
        }

        // 動畫結束，確保物件隱藏
        bambooObject.SetActive(false);

        // 重設 SpriteRenderer 顏色，以備下次使用（如果竹筍物件會被重用）
        spriteRenderer.color = startColor;
    }

    bool IsHarvestComplete()
    {
        for (int i = 0; i < bambooNum; i++)
        {
            if (!Bamboos[i].hasHarvest)
            {
                return false; // 只要有一個還沒採收就回傳 false
            }
        }
        return true; // 所有竹筍都採收完畢
    }

    // Day02 結束時的過場動畫及場景切換
    IEnumerator TransitionToGrading()
    {
        AnimationTrigger animTrigger = FindObjectOfType<AnimationTrigger>();

        if (animTrigger != null)
        {
            // 1. 執行蓋布動畫
            animTrigger.ClothAnimatorAni.SetTrigger("Back");
            ToGtadingAni.SetTrigger("Grading");

            // 2. 等待 2 秒讓動畫執行完畢
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            Debug.LogError("無法找到 AnimationTrigger 腳本，跳過過場動畫。");
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(NextScene);
    }


    /*--------------------------------------------------------------------------*/

}
