using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using static MazeManager;
using Unity.VisualScripting;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown sceneDropdown;
    [SerializeField] private List<string> algorithmNames;
    [SerializeField] private List<string> sceneNames;
    [SerializeField] private TMP_Dropdown agentDropdown;
    [SerializeField] private TMP_InputField agentCountInput;
    [SerializeField] private TMP_InputField mazeSizeInput;
    [SerializeField] private Button startButton;

    private enum AgentOptions
    {
        NoAgents,
        Close,
        Open,
        CloseAndOpen
    }

    private enum AlgorithmOptions
    {

        DFS,
        Kruskal,
        Prim
    }

    private string selectedScene;

    void Start()
    {
        // Set up the scene dropdown
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(algorithmNames);
        sceneDropdown.onValueChanged.AddListener(delegate { SetSelectedAlgorithm(); });

        // Set up the agent options dropdown
        agentDropdown.ClearOptions();
        agentDropdown.AddOptions(new List<string> { "No Agent", "Remove Dead Ends", "Open Maze", "Both" });
        agentDropdown.onValueChanged.AddListener(delegate { SetAgentOption(); });

        // Set up the algorithm options dropdown

        // Set up the input fields for agent count and maze size
        agentCountInput.onEndEdit.AddListener(delegate { SetAgentCount(); });
        mazeSizeInput.onEndEdit.AddListener(delegate { SetMazeSize(); });

        // Set up the start button
        startButton.onClick.AddListener(StartScene);

        // Initialize the toggle state based on the toggle UI
        agentToggle.isOn = PlayerPrefs.GetInt("IncludeAgentPrefab", 0) == 1;

        // Add a listener to save the preference whenever the toggle is changed
        agentToggle.onValueChanged.AddListener(SetIncludeAgentPrefab);
    }

    public Toggle agentToggle;

    // Variable to store the toggle state
    public bool includeAgentPrefab;

    private void SetIncludeAgentPrefab(bool isEnabled)
    {
        PlayerPrefs.SetInt("IncludeAgentPrefab", isEnabled ? 1 : 0);
    }

    void SetSelectedAlgorithm()
    {
        // Get the selected algorithm option based on dropdown index
        AlgorithmOptions selectedAlgorithmOption = (AlgorithmOptions)sceneDropdown.value;
        selectedScene = sceneNames[sceneDropdown.value];
        // Map the selected algorithm to PlayerPrefs using MazeManager's MazeAlgorithm enum
        switch (selectedAlgorithmOption)
        {
            //case AlgorithmOptions.PickAlgorithm:

            //    break;
            case AlgorithmOptions.DFS:
                PlayerPrefs.SetInt("algorithmType", (int)MazeManager.MazeAlgorithm.DFS);
                Debug.Log("Selected Algorithm Type: " + PlayerPrefs.GetInt("algorithmType")); // Expecting 0
                break;
            case AlgorithmOptions.Kruskal:
                PlayerPrefs.SetInt("algorithmType", (int)MazeManager.MazeAlgorithm.Kruskal);
                Debug.Log("Selected Algorithm Type: " + PlayerPrefs.GetInt("algorithmType")); // Expecting 1
                break;
            case AlgorithmOptions.Prim:
                PlayerPrefs.SetInt("algorithmType", (int)MazeManager.MazeAlgorithm.Prim);
                Debug.Log("Selected Algorithm Type: " + PlayerPrefs.GetInt("algorithmType")); // Expecting 2
                break;
        }
    }

    void SetAgentOption()
    {
        // Get the selected agent option from the dropdown
        AgentOptions selectedOption = (AgentOptions)agentDropdown.value;

        // Update the PlayerPrefs for agent option
        switch (selectedOption)
        {
            case AgentOptions.NoAgents:
                PlayerPrefs.SetInt("actionType", (int)AgentMazeRefinement.ActionType.Close); // Default to Close, but no agents will be used
                PlayerPrefs.SetInt("numAgents", 0);
                break;
            case AgentOptions.Close:
                PlayerPrefs.SetInt("actionType", (int)AgentMazeRefinement.ActionType.Close);
                break;
            case AgentOptions.Open:
                PlayerPrefs.SetInt("actionType", (int)AgentMazeRefinement.ActionType.Open);
                break;
            case AgentOptions.CloseAndOpen:
                PlayerPrefs.SetInt("actionType", (int)AgentMazeRefinement.ActionType.CloseAndOpen);
                break;
        }
    }


    void SetAgentCount()
    {
        // Parse the agent count input field and set the value in PlayerPrefs
        if (int.TryParse(agentCountInput.text, out int agentCount))
        {
            PlayerPrefs.SetInt("numAgents", agentCount);
        }
    }

    void SetMazeSize()
    {
        // Parse the maze size input field and set both width and height in PlayerPrefs
        if (int.TryParse(mazeSizeInput.text, out int mazeSize))
        {
            PlayerPrefs.SetInt("mazeWidth", mazeSize);
            PlayerPrefs.SetInt("mazeHeight", mazeSize);
        }
    }

    void StartScene()
    {
        // Save agent settings before loading the scene
        PlayerPrefs.SetInt("numAgents", PlayerPrefs.GetInt("numAgents"));
        PlayerPrefs.SetInt("actionType", PlayerPrefs.GetInt("actionType"));
        PlayerPrefs.SetInt("algorithmType", PlayerPrefs.GetInt("algorithmType"));
        PlayerPrefs.SetInt("mazeWidth", PlayerPrefs.GetInt("mazeWidth"));
        PlayerPrefs.SetInt("mazeHeight", PlayerPrefs.GetInt("mazeHeight"));

        // Load the selected scene
        if (!string.IsNullOrEmpty(selectedScene))
        {
            SceneManager.LoadScene(selectedScene);
        }
    }
}
