using UnityEngine;

public class SnakeStateManager : MonoBehaviour
{
    [Header("Referencia al SnakeController")]
    public SnakeController snake;
    [Header("Referencia al UIManager")]
    public UIManager uiManager;

    private SnakeCheckpointData checkpointData;

    public void SaveState()
    {
        if (snake == null) return;
        checkpointData = snake.GetCheckpoint();
        Debug.Log("Checkpoint guardado en SnakeStateManager");
    }

    public void RestoreState()
    {
        if (snake == null || checkpointData == null) return;
        snake.RestoreCheckpoint(checkpointData);
        Debug.Log("Checkpoint restaurado desde SnakeStateManager");
    }

    public void ClearState()
    {
        checkpointData = null;
    }

    /// <summary>
    /// Maneja el Game Over: pausa el juego y muestra el panel.
    /// </summary>
    public void GameOver()
    {
        Debug.Log("Game Over desde SnakeStateManager");

        // Pausar movimiento de la serpiente
        if (snake != null)
            snake.enabled = false;

        // Pausar tiempo global
        Time.timeScale = 0f;

        // Mostrar panel de Game Over
        if (uiManager != null)
            uiManager.ShowGameOver();
    }
}