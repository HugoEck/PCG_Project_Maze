using UnityEngine;

public class OscarsMazeGenerator : MonoBehaviour
{
    public DFSGenerator dfsGenerator;
    public UIController uiController;

    private bool[,] mazeData;

    void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        Vector2Int mazeSize = GameSettings.MazeSize;

        // Generate the maze data using DFS
        mazeData = dfsGenerator.Generate(mazeSize);

        // Render the maze based on the generated data
        RenderMaze();

        // Start the timer in UIController after maze generation
        uiController.StartTimer();

        // Place the agent after maze generation
        PlaceAgent();
    }

    private void RenderMaze()
    {
        for (int x = 0; x < mazeData.GetLength(0); x++)
        {
            for (int y = 0; y < mazeData.GetLength(1); y++)
            {
                if (mazeData[x, y])
                {
                    PlaceFloorTile(new Vector2Int(x, y));
                }
                else
                {
                    PlaceWallTile(new Vector2Int(x, y));
                }
            }
        }
    }

    private void PlaceFloorTile(Vector2Int position)
    {
        GameObject floorTile = new GameObject("FloorTile");
        floorTile.transform.position = new Vector3(position.x, position.y, 0);
        // Add a SpriteRenderer and configure as needed
    }

    private void PlaceWallTile(Vector2Int position)
    {
        GameObject wallTile = new GameObject("WallTile");
        wallTile.transform.position = new Vector3(position.x, position.y, 0);
        // Add a SpriteRenderer and configure as needed
    }

    public void PlaceAgent()
    {
        AgentController agentController = FindObjectOfType<AgentController>();
        if (agentController != null)
        {
            agentController.SpawnAgent(new Vector2Int(1, 1)); // Set start position at bottom-left

            // Set the goal at top-right (within bounds)
            Vector2Int goalPosition = new Vector2Int(GameSettings.MazeSize.x - 2, GameSettings.MazeSize.y - 2);
            agentController.InitializeMaze(mazeData, goalPosition);
        }
        else
        {
            Debug.LogError("AgentController not found in scene!");
        }
    }
}
