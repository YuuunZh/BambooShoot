using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ObjEvent : MonoBehaviour
{
    public GameObject StateCanvas;
    public GameObject SampleStateBox;
    public Animator ClothAni;
    public Animator ChangeDayUI;
    public GameObject Grading;
    public GameObject Settlement;
    public GameObject Game;
    public List<SpriteRenderer> Bamboos= new List<SpriteRenderer>();


    [Header("素材區")]
    public Sprite[] StateSp;
    public Sprite[] BambooSp;

    private bool _clothState = false;   //布是關的

    void Start()
    {
        SampleStateBox.SetActive(false);
        Grading.SetActive(false);
        Settlement.SetActive(false);
        Game.SetActive(false);
        
    }

    public void WaterShow()
    {
        SampleStateBox.SetActive(true);
    }

    public void Water()
    {
        SampleStateBox.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            Bamboos[i].sprite = BambooSp[1];
        }
    }
    public void SpreadShow()
    {
        SampleStateBox.GetComponent<Image>().sprite = StateSp[1];
        SampleStateBox.SetActive(true);
    }
    public void Spread()
    {
        SampleStateBox.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            Bamboos[i].sprite = BambooSp[2];
        }
    }
    public void Harvest()
    {
        StateCanvas.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            Bamboos[i].gameObject.SetActive(false);
        }

    }

    public void ClothesController()
    {
        if (!_clothState)
        {
            ClothAni.SetTrigger("Go");
            _clothState = true;
            SampleStateBox.SetActive(true);
            return;
        }
        else
        {
            ClothAni.SetTrigger("Back");
            _clothState = false;
            return;
        }
    }

    public IEnumerator ChangeDay()
    {
        ClothAni.SetTrigger("Back");
        _clothState = false;
        yield return new WaitForSeconds(1f);
        ChangeDayUI.SetTrigger("Down");
        SampleStateBox.SetActive(false);

        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < 6; i++)
        {
            Bamboos[i].sprite = BambooSp[3];
        }
        ChangeDayUI.SetTrigger("Up");

    }

    public void ToGrading()
    {
        ClothAni.SetTrigger("Back");
        _clothState = false;

        SampleStateBox.SetActive(false);
        Grading.SetActive(true);
    }
    public void ToSettlement()
    {

        Settlement.SetActive(true);
    }
    public IEnumerator GameStatr()
    {
        Game.SetActive(true);

        yield return new WaitForSeconds(2f);

        ProgressTransferManager.Instance.BgmChange(1);
        SceneManager.LoadScene("Maimboo");
    }


}
