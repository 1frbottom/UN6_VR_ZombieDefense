using UnityEngine;
using UnityEngine.SceneManagement;



public class SC_SceneManager : MonoBehaviour
{



    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void GameStart()
    {
        SceneManager.LoadScene("L_Stage_1");

    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("L_MainMenu");

    }

    public void RestartGame()
    {
        // 시간 다시 흐르게 하고 씬 재로딩
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
