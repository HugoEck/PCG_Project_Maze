using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown sceneDropdown;
    [SerializeField] private List<string> sceneNames;
    [SerializeField] private TMP_Dropdown agentDropdown;
    [SerializeField] private TMP_InputField agentCountInput;
    [SerializeField] private AgentMazeRefinement agentMazeRefinement;

    private enum AgentOptions
    {
        NoAgent,
        RemoveDeadEnds,
        OpenMaze,
        Both
    }

    void Start()
    {
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(sceneNames);
        sceneDropdown.onValueChanged.AddListener(delegate { LoadSelectedScene(); });

        agentDropdown.ClearOptions();
        agentDropdown.AddOptions(new List<string> { "No Agent", "Remove Dead Ends", "Open Maze", "Both" });
        agentDropdown.onValueChanged.AddListener(delegate { SetAgentOption(); });

        agentCountInput.onEndEdit.AddListener(delegate { SetAgentCount(); });
    }

    void LoadSelectedScene()
    {
        string selectedScene = sceneNames[sceneDropdown.value];

        SceneManager.LoadScene(selectedScene);
    }

    void SetAgentOption()
    {
        // Get the selected agent option from the dropdown
        AgentOptions selectedOption = (AgentOptions)agentDropdown.value;

        // Update the agentMazeRefinement with the selected option
        switch (selectedOption)
        {
            case AgentOptions.NoAgent:
                agentMazeRefinement.actionType = AgentMazeRefinement.ActionType.Close; // Default to Close, but no agents will be used
                agentMazeRefinement.numAgents = 0;
                break;
            case AgentOptions.RemoveDeadEnds:
                agentMazeRefinement.actionType = AgentMazeRefinement.ActionType.Close;
                break;
            case AgentOptions.OpenMaze:
                agentMazeRefinement.actionType = AgentMazeRefinement.ActionType.Open;
                break;
            case AgentOptions.Both:
                agentMazeRefinement.actionType = AgentMazeRefinement.ActionType.CloseAndOpen;
                break;
        }
    }

    void SetAgentCount()
    {
        if (int.TryParse(agentCountInput.text, out int agentCount))
        {
            agentMazeRefinement.numAgents = agentCount;
        }
    }
}
