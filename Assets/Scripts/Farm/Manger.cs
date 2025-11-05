using UnityEditor.Tilemaps;
using UnityEngine;


public class Manger : MonoBehaviour
{
    public class bambooData  //¦Ëµ«¸ê®Æ
    {
        float timeToWatered;
        float timeToSpread;
        SpriteRenderer bambooForm;

        bool hasWatered;
        bool hasSpread;
    }


    public GameObject[] Bamboos;
    public Sprite[] babooForm;

    int bambooNum;
    float GameTime=0;
    bool gameStop=false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        bambooNum = gameObject.transform.childCount;
        Bamboos = new GameObject[bambooNum];

        for (int i = 0; i < bambooNum; i++)
        {
            Bamboos[i] = transform.GetChild(i).gameObject;
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
