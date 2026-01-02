using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class start : MonoBehaviour
{
    public string SceneName;
    public GameObject Intro;

    private bool _hasIntro=false;

    void Start()
    {
        Intro.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_hasIntro)
        {
            Intro.SetActive(true);
            _hasIntro=true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && _hasIntro)
        {
            ProgressTransferManager.Instance.BgmChange(1);
            SceneManager.LoadScene(SceneName);
        }

        if (Input.GetKeyDown(KeyCode.C))   //清空PlayerPrefs
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("所有 PlayerPrefs 硬碟資料已清空！");

            // 同時重置記憶體中的資料物件，否則 Diary 依然會顯示已解鎖
            if (ProgressTransferManager.Instance != null)
            {
                ProgressTransferManager.Instance.ClearSessionData();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 最簡單的方法：重新載入當前場景
                ProgressTransferManager.Instance.ResetAllData();
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Q))  //退出遊戲
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            ProgressTransferManager.Instance.ClearSessionData();
            ProgressTransferManager.Instance.ResetAllData();

            Debug.Log("正在退出遊戲...");

            Application.Quit(); // 正式環境退出
        }
    }
    
}
