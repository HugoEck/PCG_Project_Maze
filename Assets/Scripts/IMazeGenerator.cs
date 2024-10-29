using UnityEngine;

public interface IMazeGenerator
{
    int[,] GenerateMaze(int width, int height);  // Generates and returns the maze array
    Vector2Int GetStartPosition();               // Returns start position
    Vector2Int GetGoalPosition();                // Returns goal position
}
