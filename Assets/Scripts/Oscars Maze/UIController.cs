using UnityEngine;
using TMPro;  // If using TextMeshPro
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI timerText;  // Reference to the timer text (UI Text component)
    public GameObject gameOverPanel;   // Reference to the GameOver panel to enable/disable
    private float elapsedTime;         // Tracks the time taken to reach the goal
    private bool isTimerRunning;       // Whether the timer is running or not

    void Start()
    {
        // Ensure the game over panel is hidden at the start
        gameOverPanel.SetActive(false);

        // Start the timer when the game starts
        StartTimer();
    }

    void Update()
    {
        // If the timer is running, update the time display
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // Starts the timer
    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
    }

    // Stops the timer when the goal is reached
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // Updates the timer text display in 00:00:00 format (minutes/seconds/milliseconds)
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000F) % 1000F);
        timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    // This function will be called when the agent reaches the goal
    public void ShowGameOverScreen()
    {
        // Stop the timer
        StopTimer();

        // Enable the game over panel to show the timer and restart button
        gameOverPanel.SetActive(true);
    }

    // Called when the restart button is clicked, loads the main menu scene
    public void OnRestartButtonClick()
    {
        SceneManager.LoadScene("MainMenuScene");  // Change this to your main menu scene name
    }
}
