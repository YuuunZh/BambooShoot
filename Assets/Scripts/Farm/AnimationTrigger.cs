using System.Collections;
using UnityEngine;
using static Manger;

public class AnimationTrigger : MonoBehaviour
{
    public Animator ClothAnimatorAni;
    public Animator NextDayAni;
    public KeyCode key;

    Manger _manger;
    bool _hasClose=false;


    void Start()
    {
        _manger = GameObject.Find("Farm(DoNotChangeContant)").GetComponent<Manger>();

        ClothAnimatorAni.SetTrigger("Go");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key) && !_hasClose)
        {
            _hasClose = true;

            _manger.Day01HadCover = true;
            ClothAnimatorAni.SetTrigger("Back");

            StartCoroutine(NextDayUI());
        }

        //次日UI自己下來
        if (_manger.countTime > _manger.Day01Time + 1.5f && !_hasClose)
        {
            _hasClose = true;
            StartCoroutine(NextDayUI());
        }
    }

    IEnumerator NextDayUI()
    {
        _manger.Day01 = false;
        NextDayAni.SetTrigger("Down");

        yield return new WaitForSeconds(2);

        NextDayAni.SetTrigger("Up");
        _manger.Day02 = true;
        

        yield return new WaitForSeconds(2);   //第二日開布

        _manger.Daytimer.fillAmount = 0;
        ClothAnimatorAni.SetTrigger("Go");

        yield return new WaitForSeconds(1);   //第二日出UI
        _manger.StartDay02HarvestUI();
    }



}
