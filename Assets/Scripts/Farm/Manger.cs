using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 竹筍本人與狀態的顯示與動作
/// 主遊戲的狀態顯示與讀條
/// </summary>

public class Manger : MonoBehaviour
{
    [System.Serializable]
    public class bambooData  //竹筍資料
    {
        //狀態出現&圖示修改
        public int timeToWatered = 3, timeToSpread = 3;
        public SpriteRenderer bambooForm;

        //是否變醜
        public bool hasWatered =false;
        public bool hasSpread = false;
        public bool harvestInTime =true;  //爲未來要新增清晨採收暫留

        //操作空間設定抓取
        public GameObject Bamboo;
        public KeyCode key;
        //public SpriteRenderer BambooSpRender;  //竹子本人
        public Image OutlineRenderer,OutlineFill,StateSpRender;   //狀態列相關空間
        public float timeCunt=0;
    }

    public List<bambooData> Bamboos = new List<bambooData>();
    public Sprite[] bambooSpForm,stateSpForm;
    public GameObject canvas;

    int bambooNum;
    float GameTime=0;
    bool gameStop=false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        bambooNum = gameObject.transform.childCount;
        //物件基本設定抓取。資料初始化
        for (int i = 0; i < bambooNum; i++)
        {
            Bamboos[i].Bamboo = transform.GetChild(i).gameObject;  //抓竹筍本人物件
            Bamboos[i].OutlineRenderer = canvas.transform.GetChild(i).
                gameObject.GetComponent<Image>();  //抓竹筍子物件的狀態框
            Bamboos[i].OutlineFill = canvas.transform.GetChild(i).GetChild(0).
                gameObject.GetComponent<Image>();  //抓竹筍子物件的狀態框
            Bamboos[i].StateSpRender = canvas.transform.GetChild(i).GetChild(1).
                gameObject.GetComponent<Image>();  //抓竹筍子物件的狀態框
            Bamboos[i].timeToWatered = UnityEngine.Random.Range(2, 5);
            Bamboos[i].timeToSpread = UnityEngine.Random.Range(3, 7);
            Bamboos[i].harvestInTime = true;
}

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStop)   //Game is running
        {
            GameTime += Time.time;
        }
    }
}
