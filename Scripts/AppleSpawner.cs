using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[ExecuteAlways]
public class AppleSpawner : MonoBehaviour
{
    public static event System.Action AppleEaten;

    [Header("References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public GameObject applePrefab;
    public SnakeController snakeController;   // ← asignado en el inspector
    public UIManager uiManager;
    public SnakeStateManager stateManager;

    [Header("Level Settings")]
    public int applesPerLevel = 15;
    public int initialApples = 1;
    [SerializeField] private int maxApplesAllowed = 0;

    private int collectedCount = 0;
    private readonly List<Vector3Int> validCells = new List<Vector3Int>();
    private readonly List<GameObject> currentApples = new List<GameObject>();

    void Start()
    {
        CacheValidCells();
        ClampApplesPerLevel();

        SafeClearAllApples();

        if (Application.isPlaying)
        {
            for (int i = 0; i < initialApples; i++)
                SpawnApple();

            UpdateUI();
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            CacheValidCells();
            ClampApplesPerLevel();
        }
    }

    void CacheValidCells()
    {
        validCells.Clear();
        if (floorTilemap == null) return;

        BoundsInt bounds = floorTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            bool hasFloor = floorTilemap.HasTile(pos);
            bool hasWallTile = (wallTilemap != null && wallTilemap.HasTile(pos));
            if (hasFloor && !hasWallTile)
            {
                validCells.Add(pos);
            }
        }

        maxApplesAllowed = validCells.Count;
    }

    void ClampApplesPerLevel()
    {
        if (applesPerLevel > maxApplesAllowed)
            applesPerLevel = maxApplesAllowed;
        if (applesPerLevel < 1)
            applesPerLevel = 1;
    }

    public void SpawnApple()
    {
        if (!Application.isPlaying) return;
        if (validCells.Count == 0 || applePrefab == null || floorTilemap == null) return;

        HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

        if (snakeController != null && snakeController.grid != null && snakeController.head != null)
        {
            occupied.Add(snakeController.grid.WorldToCell(snakeController.head.transform.position));

            foreach (GameObject seg in snakeController.GetBodySegments())
            {
                if (seg == null) continue;
                occupied.Add(snakeController.grid.WorldToCell(seg.transform.position));
            }
        }

        foreach (GameObject apple in currentApples)
        {
            if (apple != null)
                occupied.Add(floorTilemap.WorldToCell(apple.transform.position));
        }

        List<Vector3Int> freeCells = new List<Vector3Int>();
        foreach (var c in validCells)
            if (!occupied.Contains(c)) freeCells.Add(c);

        if (freeCells.Count == 0)
        {
            Debug.LogWarning("AppleSpawner: No hay celdas libres para colocar manzana");
            return;
        }

        Vector3Int chosenCell = freeCells[Random.Range(0, freeCells.Count)];
        Vector3 worldPos = floorTilemap.GetCellCenterWorld(chosenCell);

        GameObject newApple = Instantiate(applePrefab, worldPos, Quaternion.identity);
        currentApples.Add(newApple);

        Apple appleScript = newApple.GetComponent<Apple>();
        if (appleScript != null)
            appleScript.Init(this, snakeController, stateManager);

        newApple.tag = "Apple";
    }

    public void OnAppleCollected(GameObject apple)
    {
        if (!Application.isPlaying) return;

        collectedCount++;
        currentApples.Remove(apple);
        UpdateUI();

        AppleEaten?.Invoke();

        // Guardar checkpoint en la última manzana comida
        if (snakeController != null && GameManager.Instance != null)
        {
            GameManager.Instance.GuardarCheckpoint(snakeController.GetCheckpoint());
        }

        // Cada X manzanas aumenta velocidad
        if (snakeController != null && snakeController.applesPerSpeedUp > 0 &&
            collectedCount > 0 && collectedCount % snakeController.applesPerSpeedUp == 0)
        {
            snakeController.IncreaseSpeed();
            Debug.Log($"Snake speed increased {snakeController.speedIncreasePercent * 100}% at {collectedCount} apples");
        }

        if (applesPerLevel > 0 && collectedCount >= applesPerLevel)
        {
            Debug.Log("AppleSpawner: Nivel completado!");
            if (uiManager != null)
                uiManager.ShowLevelComplete();
        }
        else
        {
            RespawnApple();
        }
    }

    public void RespawnApple()
    {
        if (!Application.isPlaying) return;
        SpawnApple();
    }

    private void UpdateUI()
    {
        if (!Application.isPlaying) return;
        if (uiManager != null)
            uiManager.UpdateAppleCounter(collectedCount, applesPerLevel);
    }

    private void SafeClearAllApples()
    {
        var applesInScene = GameObject.FindGameObjectsWithTag("Apple");
        foreach (var apple in applesInScene)
        {
            if (apple == null) continue;
            if (Application.isPlaying) Destroy(apple);
            else DestroyImmediate(apple);
        }

        foreach (var apple in currentApples)
        {
            if (apple == null) continue;
            if (Application.isPlaying) Destroy(apple);
            else DestroyImmediate(apple);
        }

        currentApples.Clear();
        collectedCount = 0;
    }

    public void ResetApples()
    {
        if (!Application.isPlaying) return;

        SafeClearAllApples();
        UpdateUI();

        for (int i = 0; i < initialApples; i++)
            SpawnApple();
    }

    void OnDisable()
    {
        foreach (var apple in currentApples)
        {
            if (apple != null)
                DestroyImmediate(apple);
        }
        currentApples.Clear();
    }

    // 🔹 Penalización al revivir desde checkpoint
    public void ApplyCheckpointPenalty()
    {
        // Aumentar la meta de manzanas pendientes en 50% de lo que falta
        int faltantes = applesPerLevel - collectedCount;
        int penalty = Mathf.FloorToInt(faltantes * 0.5f);
        applesPerLevel += penalty;

        Debug.Log($"Checkpoint penalty aplicado: ahora faltan {applesPerLevel - collectedCount} manzanas");
        UpdateUI();
    }
}