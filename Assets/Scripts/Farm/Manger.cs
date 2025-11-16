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
                    Bamboos[i].bambooForm.sprite = bambooSpForm[3];
                    bambooLevel(Bamboos[i]);    //要做等級之下的角色卡分類

                    if (!Bamboos[i].hasHarvest && Input.GetKeyDown(Bamboos[i].key))
                    {
                        Bamboos[i].hasHarvest=true;
                    }
                }


            }

        }
    }

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

}
