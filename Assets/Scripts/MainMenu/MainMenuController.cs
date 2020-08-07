using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    public void playGame()
    {
        SceneManager.LoadScene("TestLevel");
    }

    // Update is called once per frame
    public void options()
    {
        
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
