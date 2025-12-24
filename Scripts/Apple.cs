using UnityEngine;

public class Apple : MonoBehaviour
{
    private AppleSpawner spawner;
    private SnakeController snakeController;
    private SnakeStateManager stateManager;

    // Inicializar referencias desde el spawner
    public void Init(AppleSpawner spawnerRef, SnakeController snakeRef, SnakeStateManager stateManagerRef)
    {
        spawner = spawnerRef;
        snakeController = snakeRef;
        stateManager = stateManagerRef;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SnakeHead"))
        {
            // 👇 crecer la serpiente
            if (snakeController != null)
                snakeController.Grow();

            // 👇 guardar estado de la serpiente para continuar después
            if (stateManager != null)
                stateManager.SaveState();

            // 👇 notificar al spawner primero
            if (spawner != null)
                spawner.OnAppleCollected(gameObject);

            // 👇 destruir esta manzana después
            Destroy(gameObject);
        }
    }
}