using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

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
        public float timeToWatered=3;
        public float timeToSpread =3;
        public SpriteRenderer bambooForm;

        //是否變醜
        public bool hasWatered =true;
        public bool hasSpread = true;
        public bool harvestInTime =true;  //爲未來要新增清晨採收暫留

        //操作設定
        public GameObject Bamboo;
        public KeyCode key;
        public SpriteRenderer BambooSpRender;
        public SpriteRenderer StateSpRender;
    }

    public List<bambooData> Bamboos = new List<bambooData>();
    //public GameObject[] Bamboos;
    public Sprite[] babooForm;


    int bambooNum;
    float GameTime=0;
    bool gameStop=false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        bambooNum = gameObject.transform.childCount;
        //物件基本設定抓取
        for (int i = 0; i < bambooNum; i++)
        {
            Bamboos[i].Bamboo = transform.GetChild(i).gameObject;  //抓竹筍本人
            Bamboos[i].BambooSpRender = transform.GetChild(i).GetChild(0)
                .gameObject.GetComponent<SpriteRenderer>();  //抓竹筍子物件的狀態框
            Bamboos[i].StateSpRender = transform.GetChild(i).GetChild(1)
                .gameObject.GetComponent<SpriteRenderer>();  //抓竹筍子物件的狀態圖

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStop)
        {
            GameTime += Time.time;
        }
    }
}
