using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public GameObject panel;  // Changed from "gameOverPanel" to "panel" to match your setup

    private float elapsedTime = 0f;
    private bool isTimerRunning = false;

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = FormatTime(elapsedTime);
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
        panel.SetActive(false); // Ensure panel is hidden during gameplay
    }

    public void StopTimerAndShowPanel()
    {
        isTimerRunning = false;
        panel.SetActive(true);  // Display panel when goal is reached
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}
