using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public TMP_InputField mazeSizeInputTMP;  // TextMeshPro InputField for maze size
    public TMP_Dropdown algorithmDropdownTMP;  // TextMeshPro Dropdown for algorithm selection
    public UnityEngine.UI.Button startButtonTMP;  // Unity UI Button

    public static int mazeSize;  // To store the selected maze size
    public static string selectedAlgorithm;  // To store the selected algorithm

    void Start()
    {
        // Set default values for the input field and dropdown
        mazeSizeInputTMP.text = "20";  // Default maze size
        algorithmDropdownTMP.options.Clear();

        // Add options to the TextMeshPro dropdown
        algorithmDropdownTMP.options.Add(new TMP_Dropdown.OptionData() { text = "DFS" });
        algorithmDropdownTMP.options.Add(new TMP_Dropdown.OptionData() { text = "Prim's Algorithm" });

        // Add listener for the start button
        startButtonTMP.onClick.AddListener(OnStartButtonClick);
    }

    // This function is called when the Start button is clicked
    void OnStartButtonClick()
    {
        // Get the maze size from the TMP InputField
        if (int.TryParse(mazeSizeInputTMP.text, out int size))
        {
            mazeSize = size;
        }
        else
        {
            mazeSize = 20;  // Default to 20 if input is invalid
        }

        // Get the selected algorithm from the TMP Dropdown
        selectedAlgorithm = algorithmDropdownTMP.options[algorithmDropdownTMP.value].text;

        // Load the game scene where the maze will be generated
        SceneManager.LoadScene("Oscar's Maze");  // Make sure your maze scene is named "GameScene"
    }
}
