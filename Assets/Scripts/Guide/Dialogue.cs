using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

/// <summary>
/// 掛在Npc上
/// 對話導入
/// </summary>
public class Dialogue : MonoBehaviour
{
    [Header("對話")]
    public TextAsset dialogueDateFile;   //對話文本
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject nextBtn;
    //public Button _nextn;   //同上個BTN同物件，但呼叫屬性不同
    public GameObject dialogueBox;
    public float letterShowSpeed;
    [Header("教學動畫")]
    public Animator FocusAni;
    public SpriteRenderer BGPlace;
    public List<Sprite> BgImg = new List<Sprite>();
    [Header("物件區開關")]     //避免再次觸發
    public GameObject introDia;
    [Header("抓取腳本")]
    public ObjEvent Obj_event;

    private string[] _dialogueRows;
    private string[] _rowCells;
    private int dialogueIndex;
    private bool _inDialogue;    //對話中防二次觸發

    Dictionary<string, GameObject> _diaTrigger = new Dictionary<string, GameObject>();
     Dictionary<string, Action> _eventDic = new Dictionary<string, Action>();


    private void Awake()
    {
        _diaTrigger["intro"] = introDia;
        _eventDic["0"] = Obj_event.WaterShow;
        _eventDic["1"] = Obj_event.Water;
        _eventDic["2"] = Obj_event.SpreadShow;
        _eventDic["3"] = Obj_event.Spread;
        _eventDic["4"] = Obj_event.Harvest;
    }

    // Start is called before the first frame update
    void Start()
    {
        readFile(dialogueDateFile);
        dialogueBox.SetActive(true);
        _inDialogue = false;

        TriggerDialogue(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) /*&& dialogueText.text.Length== _rowCells[2].Length*/)    //有輸入 且 畫面上的字數跟預計出現的字數一樣
        {
            if (int.Parse(_rowCells[3]) != 10001)
            {
                nextSentence();
            }
            else if (int.Parse(_rowCells[3]) == 10001 && dialogueBox.activeSelf)
            {
                QuiteDialogue();
                //對話結束，這邊可以做跳出教學
            }
        }
        
    }


    public void TriggerDialogue(int _startIndex)    //其他段落用
    {
        if (!_inDialogue)
        {
            dialogueIndex = _startIndex;  //將開頭句子指定出去
            _inDialogue = true;
            dialogueBox.SetActive(true);
            showSentence();
        }
    }

    public void UpdateText(string _name,string _dialogue)
    {
        nameText.text = _name;
        dialogueText.text = _dialogue; 
    }

    public void readFile(TextAsset _dialogueFile)
    {
        _dialogueRows = _dialogueFile.text.Split('\n');
        
    }

    /// <summary>
    /// 送句子去上特效
    /// 改人物臉部表情&明暗
    /// 改背景
    /// 判斷這句是不是最後一句
    /// </summary>
    public void showSentence()
    {
        foreach (var row in _dialogueRows)
        {
            if (string.IsNullOrEmpty(row)) continue; // 略過空白行

            _rowCells = row.Split(',');

            // 使用 Trim() 確保抓到的是乾淨的數字字串
            int currentID;
            if (!int.TryParse(_rowCells[0].Trim(), out currentID)) continue;

            if (int.Parse(_rowCells[0]) == dialogueIndex) //比對表格index是不是要的句子
            {
                //UpdateText(_rowCells[2], _rowCells[3]);  
                StartCoroutine(TypeEffect(_rowCells[2], letterShowSpeed));  //送句子去上特效
                

                if(int.Parse(_rowCells[4]) != 10001)  //動畫執行區，index順序請去excel對
                {
                    FocusAni.SetTrigger(_rowCells[4]);
                    print(_rowCells[4]);
                }

                string cmd = _rowCells[5];
                if (int.Parse(_rowCells[5]) != 10001)  //事件執行區，index順序請去excel對
                {
                    _eventDic[cmd].Invoke();
                }

                if (int.Parse(_rowCells[3]) != 10001)
                { 
                    dialogueIndex = int.Parse(_rowCells[3]); //指定下一句
                    break;
                }
                else if(int.Parse(_rowCells[3]) == 10001)      //結尾句
                {
                    _diaTrigger["intro"].SetActive(false);    //避免再次觸發
                    break;
                }
                
            }
        }
        
    }

    IEnumerator TypeEffect(string _sentence,float late)
    {
        nextBtn.SetActive(false);
        string _text = "";  //初始化字串
        foreach(char _letter in _sentence.ToCharArray())
        {
            _text += _letter;
            UpdateText(_rowCells[1], _text); //送string去顯示
            yield return new WaitForSeconds(late);
        }
        nextBtn.SetActive(true);
    }

    public void objClose(GameObject obj)
    {
        obj.SetActive(false);
    }

    /*------------------------btn區---------------------------*/
    public void nextSentence()
    {
        showSentence();
        //print("next");
    }
    public void QuiteDialogue()
    {
        print("Quit");
        dialogueBox.SetActive(false);
        //dialogueIndex = startIndex; //重置對話Index
        _inDialogue = false;
        BGPlace.sprite = null;
        //FacePlace.sprite = null;
        //body.sprite = null;
        /*if(dialogueIndex == 15)
        {
            _main.GetStart = true;
        }
        else
        {
            _tutorialMap.startTutorial();
        }*/
    }

}
