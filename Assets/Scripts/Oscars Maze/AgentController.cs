using System.Collections;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public Sprite agentSprite;
    public float tileScale = 1f;

    private GameObject agentObj;
    private Vector2Int goalPosition;
    private bool[,] maze;

    public UIController uiController;

    public void InitializeMaze(bool[,] generatedMaze, Vector2Int goalPos)
    {
        maze = generatedMaze;
        goalPosition = goalPos;

        // Spawn the agent
        SpawnAgent(new Vector2Int(1, 1));  // Hardcoded start position at (1, 1)

        // Start exploration
        StartCoroutine(ExploreMaze(new Vector2Int(1, 1)));
    }

    public void SpawnAgent(Vector2Int startPosition)
    {
        if (agentObj != null)
        {
            Destroy(agentObj);
        }

        agentObj = new GameObject("Agent");
        agentObj.transform.position = new Vector3(startPosition.x * tileScale, startPosition.y * tileScale, 0);
        SpriteRenderer renderer = agentObj.AddComponent<SpriteRenderer>();
        renderer.sprite = agentSprite;
        renderer.sortingOrder = 10; // Renders above other tiles
    }

    private IEnumerator ExploreMaze(Vector2Int currentPosition)
    {
        while (true)
        {
            if (currentPosition == goalPosition)
            {
                Debug.Log("Goal Reached!");
                uiController.StopTimerAndShowPanel();  // Corrected method name
                yield break;
            }

            // Move the agent and explore logic here
            yield return new WaitForSeconds(0.1f); // Adjust delay as needed
        }
    }
}
