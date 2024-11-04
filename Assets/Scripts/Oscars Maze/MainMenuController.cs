using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public TMP_InputField mazeSizeInput;    // Reference to the input field for maze size
    public TMP_Dropdown algorithmDropdown;  // Reference to the dropdown to select maze generation algorithm
    public GameObject startButton;          // Reference to the start button GameObject

    public void OnStart()
    {
        Debug.Log("Start button clicked!");

        // Validate Maze Size Input
        int mazeSize = 0;
        if (!int.TryParse(mazeSizeInput.text, out mazeSize) || mazeSize <= 0)
        {
            Debug.LogError("Invalid Maze Size. Please enter a positive integer.");
            return;
        }

        // Assign Maze Size and Algorithm Index to GameSettings (assuming GameSettings is a static class or singleton)
        GameSettings.MazeSize = mazeSize;
        GameSettings.AlgorithmIndex = algorithmDropdown.value;

        // Load the next scene - Oscar's Maze
        SceneManager.LoadScene("Oscar's Maze");
    }
}
