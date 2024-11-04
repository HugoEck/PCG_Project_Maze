using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
        Prim,
        Kruskal,
        DFS
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
    }

    void SetSelectedAlgorithm()
    {
        // Get the selected scene name from the dropdown
        selectedScene = algorithmNames[sceneDropdown.value];
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
