using UnityEngine;
using UnityEngine.SceneManagement;

public class start : MonoBehaviour
{
    public string SceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene(SceneName);
    }
}
