using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    public SnakeController snakeController;   // referencia al SnakeController
    public SnakeCollision snakeCollision;     // referencia al script de colisiones
    public UIManager uiManager;               // referencia al UIManager
    public AppleSpawner appleSpawner;         // referencia al spawner de manzanas

    [Header("Game State")]
    public bool isGameOver = false;

    // Evento para que otros sistemas (UI, etc.) puedan escuchar
    public event System.Action OnGameOver;

    void Start()
    {
        if (snakeCollision != null)
        {
            // Suscribirse a eventos de colisión
            snakeCollision.OnWallCollision += HandleGameOver;
            snakeCollision.OnSelfCollision += HandleGameOver;
        }
    }

    void HandleGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GameController: Game Over activado");

        // Retroceder la serpiente a la última posición válida
        if (snakeController != null)
        {
            snakeController.RevertLastStep();
            snakeController.enabled = false; // detener movimiento
        }

        // Notificar a la UI
        OnGameOver?.Invoke();

        if (uiManager != null)
            uiManager.ShowGameOver();
    }

    public void RestartGame()
    {
        // 🔑 Reinicia manzanas y marcador
        if (appleSpawner != null)
            appleSpawner.ResetApples();

        if (uiManager != null)
            uiManager.UpdateAppleCounter(0, appleSpawner.applesPerLevel);

        // Reactiva la serpiente
        isGameOver = false;
        if (snakeController != null)
            snakeController.enabled = true;

        Debug.Log("GameController: Juego reiniciado");
    }
}