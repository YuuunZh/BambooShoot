using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class start : MonoBehaviour
{
    public string SceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene(SceneName);

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
            }

        }

        if (Input.GetKeyDown(KeyCode.Q))  //退出遊戲
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            ProgressTransferManager.Instance.ClearSessionData();

            Debug.Log("正在退出遊戲...");

            Application.Quit(); // 正式環境退出
        }
    }
}
