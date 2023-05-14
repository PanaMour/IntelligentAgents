using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load the SampleScene
        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        // Exit the application
        Application.Quit();
    }
}
