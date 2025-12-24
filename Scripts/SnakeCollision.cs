using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class SnakeCollision : MonoBehaviour
{
    [Header("References")]
    public SnakeController snakeController;
    public Tilemap wallTilemap;
    public UIManager uiManager; // 👈 referencia al UIManager

    // Eventos para notificar al GameController
    public event Action OnWallCollision;
    public event Action OnSelfCollision;

    void Start()
    {
        // Suscribirse a los eventos
        OnWallCollision += HandleWallCollision;
        OnSelfCollision += HandleSelfCollision;
    }

    void Update()
    {
        CheckWallCollision();
        CheckSelfCollision();
    }

    void CheckWallCollision()
    {
        if (snakeController == null || wallTilemap == null) return;

        Vector3Int headCell = snakeController.CurrentCell;

        if (wallTilemap.HasTile(headCell))
        {
            Debug.Log("SnakeCollision: choque con muro en celda " + headCell);
            OnWallCollision?.Invoke();
        }
    }

    void CheckSelfCollision()
    {
        if (snakeController == null) return;

        Vector3Int headCell = snakeController.CurrentCell;
        var history = snakeController.GetCellHistory();

        for (int i = 1; i < history.Count; i++)
        {
            if (history[i] == headCell)
            {
                Debug.Log("SnakeCollision: choque con cuerpo en celda " + headCell);
                OnSelfCollision?.Invoke();
                break;
            }
        }
    }

    // -------------------
    // Handlers de eventos
    // -------------------
    private void HandleWallCollision()
    {
        if (uiManager != null)
            uiManager.ShowGameOver();
    }

    private void HandleSelfCollision()
    {
        if (uiManager != null)
            uiManager.ShowGameOver();
    }
}