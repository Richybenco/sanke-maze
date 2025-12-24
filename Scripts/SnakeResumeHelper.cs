using UnityEngine;

public class SnakeResumeHelper : MonoBehaviour
{
    public SnakeController snakeController;
    public SnakeStateManager stateManager;

    /// <summary>
    /// Restaura el estado guardado y deja la serpiente en pausa.
    /// </summary>
    public void RestoreAndPause()
    {
        if (stateManager != null)
            stateManager.RestoreState();

        if (snakeController != null)
        {
            // Resetear dirección para evitar quedarse en el muro
            snakeController.ResetDirection();

            // Pausar movimiento (no deshabilitar el componente)
            snakeController.PauseSnake();
        }
    }

    /// <summary>
    /// Reanuda la serpiente al detectar el primer input.
    /// </summary>
    public void ResumeOnFirstInput()
    {
        if (snakeController != null)
            snakeController.ResumeAfterInput();
    }
}