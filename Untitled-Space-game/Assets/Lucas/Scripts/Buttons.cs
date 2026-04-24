using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitsies");

    }
    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }
    public void ToMenu() 
    {
        SceneManager.LoadScene("Start");
    }
}

