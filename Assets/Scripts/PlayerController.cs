using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;           // Speed of movement
    public float moveDelay = 0.1f;         // Delay between moves when holding a key

    private Vector2 targetPosition;
    private bool isMoving;
    private float moveTimer;

    private void Start()
    {
        // Set the initial target position to the player's starting position
        targetPosition = transform.position;
    }

    private void Update()
    {
        // If the player is already moving, update the position towards the target
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Stop moving if the target position is reached
            if ((Vector2)transform.position == targetPosition)
                isMoving = false;
        }
        else
        {
            // Handle continuous movement if a key is held down
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    TryMove(Vector2.up);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    TryMove(Vector2.down);
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    TryMove(Vector2.left);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    TryMove(Vector2.right);
                }
            }
        }
    }

    private void TryMove(Vector2 direction)
    {
        // Calculate the new position based on the direction
        Vector2 newPosition = (Vector2)transform.position + direction;

        // Check if the new position is walkable
        if (IsWalkable(newPosition))
        {
            targetPosition = newPosition; // Set the target position
            isMoving = true;              // Start moving
            //moveTimer = moveDelay;        // Reset the move delay

            // Ensure player stays slightly in front by setting z-position to -1
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        }
    }

    private bool IsWalkable(Vector2 position)
    {
        // Check for obstacles (like walls) at the target position using a Collider2D overlap
        Collider2D hit = Physics2D.OverlapPoint(position);

        // Ensure the target is a walkable floor (e.g., tagged as "Floor")
        return hit != null && hit.CompareTag("Floor");
    }
}
