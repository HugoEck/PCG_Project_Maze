using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float moveDelay = 0.1f;
    public Vector2 goalPosition;
    private TextMeshProUGUI stepText; // Reference to Step_Text TextMeshPro component

    private Vector2 targetPosition;
    private bool isMoving;
    private float moveTimer;
    private int stepsTaken = 0;
    private float timer = 0f; // Timer variable
    private bool timerStarted = false; // Flag to start the timer on first move

    private void Start()
    {
        // Find the TextMeshProUGUI components at runtime
        stepText = FindObjectOfType<TextMeshProUGUI>();

        if (stepText == null)
        {
            Debug.LogError("Step_Text TextMeshProUGUI not found in the scene.");
            return;
        }

        // Set the initial target position to the player's starting position
        targetPosition = transform.position;

        // Initialize the step text display
        UpdateDisplayText();
    }

    private void Update()
    {
        // Update the timer only if it has started
        if (timerStarted)
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

                if ((Vector2)transform.position == goalPosition)
                {
                    Debug.Log("Maze completed! Steps taken: " + stepsTaken + " | Time: " + Mathf.FloorToInt(timer) + "s");
                }
            }
        }
        else
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                if (Input.GetKey(KeyCode.W)) TryMove(Vector2.up);
                else if (Input.GetKey(KeyCode.S)) TryMove(Vector2.down);
                else if (Input.GetKey(KeyCode.A)) TryMove(Vector2.left);
                else if (Input.GetKey(KeyCode.D)) TryMove(Vector2.right);
            }
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

            // Start the timer on the first move
            if (!timerStarted)
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
        // Update the text to display the current step count and timer on the same line
        if (stepText != null)
        {
            stepText.text = $"Steps: {stepsTaken} | Time: {Mathf.FloorToInt(timer)}s";
        }
    }
}
