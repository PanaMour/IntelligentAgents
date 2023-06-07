using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if !UNITY_EDITOR
using SFB;
#endif

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI filePathLabel;
    public TextMeshProUGUI agentNumLabel;
    public string filePath;

    public void StartGame()
    {
        // Load the SampleScene
        SceneManager.LoadScene("SampleScene");
    }

    public void OnLoadFileButtonClicked()
{
#if UNITY_EDITOR
    // Open file selection dialog
    string selectedFilePath = EditorUtility.OpenFilePanel("Select File", "", "");
#else
    // Open file selection dialog using StandaloneFileBrowser
    string[] paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", "", false);
    string selectedFilePath = paths.Length > 0 ? paths[0] : "";
#endif

    if (!string.IsNullOrEmpty(selectedFilePath))
    {
        // Store the selected file path
        filePath = selectedFilePath;
        // Update the UI label to display the selected file path
        filePathLabel.text = filePath;
        FindAgentNumber(System.IO.File.ReadAllText(filePath));

        // Load the file and perform necessary actions
        PlayerPrefs.SetString("environment", filePath);
        PlayerPrefs.Save();
    }
}


    public int FindAgentNumber(string text)
    {
        int largestSingleDigitNumber = -1;
        foreach (char c in text)
        {
            if (char.IsDigit(c))
            {
                int digit = int.Parse(c.ToString());
                if (digit >= 0 && digit <= 9)
                {
                    largestSingleDigitNumber = Math.Max(largestSingleDigitNumber, digit);
                    agentNumLabel.text = (largestSingleDigitNumber + 1).ToString();
                }
            }
        }
        return largestSingleDigitNumber;
    }
    public void ExitGame()
    {
        // Exit the application
        Application.Quit();
    }
}
