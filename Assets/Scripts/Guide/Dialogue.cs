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

    // 【新增】互動等待狀態
    private bool _waitingInteract = false;
    private string _pendingCmd = null;


    Dictionary<string, GameObject> _diaTrigger = new Dictionary<string, GameObject>();
    private class EventDef
    {
        public Action action;                 // 普通立即事件
        public Func<IEnumerator> coroutine;   // 協程事件
        public bool waitUntilDone = true;     // 是否要等協程跑完才解除鎖定
    }

    private Dictionary<string, EventDef> _eventDic = new Dictionary<string, EventDef>();
    private bool _eventRunning = false;       // 【新增】事件執行鎖

    // 【新增】ObjAnimation 編號 -> 按鍵 對照表
    private readonly Dictionary<string, KeyCode> _cmdKey = new Dictionary<string, KeyCode>()
    {
        { "0", KeyCode.K },
        { "1", KeyCode.A },
        { "2", KeyCode.K },
        { "3", KeyCode.A },
        { "4", KeyCode.A },
        { "5", KeyCode.Space },
        { "6", KeyCode.Space },
        { "7", KeyCode.Space },
        { "8", KeyCode.K },
        { "9", KeyCode.K },
    };

    private void Awake()
    {
        _diaTrigger["intro"] = introDia;
        _eventDic["0"] = new EventDef { action = Obj_event.WaterShow };
        _eventDic["1"] = new EventDef { action = Obj_event.Water };
        _eventDic["2"] = new EventDef { action = Obj_event.SpreadShow };
        _eventDic["3"] = new EventDef { action = Obj_event.Spread };
        _eventDic["4"] = new EventDef { action = Obj_event.Harvest };
        _eventDic["5"] = new EventDef { action = Obj_event.ClothesController };
        _eventDic["6"] = new EventDef { coroutine = Obj_event.ChangeDay };
        _eventDic["7"] = new EventDef { action = Obj_event.ToGrading };
        _eventDic["8"] = new EventDef { action = Obj_event.ToSettlement };
        _eventDic["9"] = new EventDef { coroutine = Obj_event.GameStatr };
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
        if (_eventRunning) return;
        // 若正在等待互動：只吃「對應互動鍵」，並阻止 K 下一句
        if (_waitingInteract && !string.IsNullOrEmpty(_pendingCmd))
        {
            if (_cmdKey.TryGetValue(_pendingCmd, out KeyCode key))
            {
                if (Input.GetKeyDown(key))
                {
                    /*// 真正執行事件（此時才 Invoke）
                    if (_eventDic.TryGetValue(_pendingCmd, out var act))
                    {
                        act.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning($"找不到事件編號 {_pendingCmd} 的對應 function");
                    }

                    // 清除等待狀態，互動完成
                    _waitingInteract = false;
                    _pendingCmd = null;

                    // 互動完成後：你可以選擇「自動下一句」或「等玩家按K」
                    // A) 自動下一句（打開就用）
                    // nextSentence();

                    // B) 等玩家按 K 再下一句（預設）*/
                    if (Input.GetKeyDown(key))
                    {
                        RunEvent(_pendingCmd);  // 用統一入口

                        // 只要按下互動鍵，就視為互動已完成（避免重複觸發）
                        _waitingInteract = false;
                        _pendingCmd = null;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"ObjAnimation 編號 {_pendingCmd} 沒有設定按鍵，請在 _cmdKey 加對照。");
            }

            return; // 等互動時，不往下跑 K 的邏輯
        }



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

                
                // 改成：如果 ObjAnimation != 10001，就「掛起等待玩家按鍵」
                _waitingInteract = false;
                _pendingCmd = null;

                string cmd = _rowCells[5].Trim();
                if (cmd != "10001")
                {
                    _waitingInteract = true;
                    _pendingCmd = cmd;

                }

                /*string cmd = _rowCells[5];
                if (int.Parse(_rowCells[5]) != 10001)  //事件執行區，index順序請去excel對
                {
                    _eventDic[cmd].Invoke();
                }*/


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

    // 顯示用：把 KeyCode 轉成比較直觀的字
    private string KeyToHint(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            default: return key.ToString();
        }
    }

    private void RunEvent(string cmd)
    {
        if (_eventRunning) return;

        if (!_eventDic.TryGetValue(cmd, out var def) || def == null)
        {
            Debug.LogWarning($"找不到事件編號 {cmd} 的對應 function");
            return;
        }

        // 普通事件
        if (def.action != null)
        {
            _eventRunning = true;
            def.action.Invoke();
            _eventRunning = false;
            return;
        }

        // 協程事件
        if (def.coroutine != null)
        {
            _eventRunning = true;
            StartCoroutine(RunEventCoroutine(def));
            return;
        }

        Debug.LogWarning($"事件編號 {cmd} 沒有 action 或 coroutine");
    }

    private IEnumerator RunEventCoroutine(EventDef def)
    {
        yield return def.coroutine.Invoke();

        // 若你希望協程跑完才解鎖
        if (def.waitUntilDone)
            _eventRunning = false;
        else
            _eventRunning = false; // 目前先一律解鎖；若要不等待，可在此調整
    }

}
