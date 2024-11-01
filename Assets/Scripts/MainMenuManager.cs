using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private List<string> sceneNames;

    void Start()
    {
        // Clear existing options and add new ones based on the scene names list.
        dropdown.ClearOptions();
        dropdown.AddOptions(sceneNames);

        // Add a listener to handle the selection change
        dropdown.onValueChanged.AddListener(delegate { LoadSelectedScene(); });
    }

    void LoadSelectedScene()
    {
        // Get the selected scene name from the dropdown
        string selectedScene = sceneNames[dropdown.value];

        // Load the selected scene
        SceneManager.LoadScene(selectedScene);
    }
}