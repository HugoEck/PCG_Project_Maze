using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float moveDelay = 0.1f;
    public Vector2 goalPosition;
    private TextMeshProUGUI stepText;

    private Vector2 targetPosition;
    private bool isMoving;
    private float moveTimer;
    private int stepsTaken = 0;
    private float timer = 0f;
    private bool timerStarted = false;
    private bool goalReached = false; // New variable to track if the goal is reached

    private void Start()
    {
        stepText = FindObjectOfType<TextMeshProUGUI>();

        if (stepText == null)
        {
            Debug.LogError("Step_Text TextMeshProUGUI not found in the scene.");
            return;
        }

        targetPosition = transform.position;
        UpdateDisplayText();
    }

    private void Update()
    {
        // Update the timer only if it has started and the goal is not reached
        if (timerStarted && !goalReached)
        {
            timer += Time.deltaTime;
            UpdateDisplayText();
        }

        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if ((Vector2)transform.position == targetPosition)
            {
                isMoving = false;

                // Check if the player has reached the goal position
                if ((Vector2)transform.position == goalPosition)
                {
                    goalReached = true;        // Set goalReached to true
                    timerStarted = false;      // Stop the timer
                    Debug.Log("Maze completed! Steps taken: " + stepsTaken + " | Time: " + Mathf.FloorToInt(timer) + "s");
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.W)) TryMove(Vector2.up);
            else if (Input.GetKey(KeyCode.S)) TryMove(Vector2.down);
            else if (Input.GetKey(KeyCode.A)) TryMove(Vector2.left);
            else if (Input.GetKey(KeyCode.D)) TryMove(Vector2.right);
        }
    }

    private void TryMove(Vector2 direction)
    {
        Vector2 newPosition = (Vector2)transform.position + direction;

        if (IsWalkable(newPosition))
        {
            targetPosition = newPosition;
            isMoving = true;
            moveTimer = moveDelay;
            stepsTaken++;

            // Start the timer on the first move if it hasn't started yet
            if (!timerStarted && !goalReached)
            {
                timerStarted = true;
            }

            UpdateDisplayText();
        }
    }

    private bool IsWalkable(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position);
        return hit != null && hit.CompareTag("Floor");
    }

    private void UpdateDisplayText()
    {
        if (stepText != null)
        {
            stepText.text = $"Steps: {stepsTaken} | Time: {Mathf.FloorToInt(timer)}s";
        }
    }
}
