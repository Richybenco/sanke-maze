using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referencias")]
    public SnakeController snakeController;
    public AppleSpawner appleSpawner;

    private SnakeCheckpointData checkpointData;

    private int currentLevel;
    private int maxUnlockedLevel;

    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        Debug.Log($"Nivel restaurado: {currentLevel}, máximo desbloqueado: {maxUnlockedLevel}");

        // 🔹 Asignación automática con nueva API
        if (snakeController == null)
        {
            snakeController = Object.FindFirstObjectByType<SnakeController>();
            if (snakeController != null)
                Debug.Log("GameManager asignó SnakeController en runtime: " + snakeController.name);
            else
                Debug.LogWarning("No se encontró ningún SnakeController en la escena");
        }
    }
    else
    {
        Destroy(gameObject);
    }
}
    public void GuardarCheckpoint(SnakeCheckpointData data)
    {
        checkpointData = data;
        Debug.Log("Checkpoint guardado en GameManager");
    }

    public void ReiniciarSnakeDesdeCheckpoint()
    {
        if (snakeController != null && checkpointData != null)
        {
            snakeController.enabled = true;
            snakeController.gameObject.SetActive(true);

            snakeController.RestoreCheckpoint(checkpointData);

            if (appleSpawner != null)
                appleSpawner.ApplyCheckpointPenalty();

            Debug.Log("Snake reiniciado desde checkpoint con penalización aplicada");
        }
        else
        {
            Debug.LogWarning("No hay checkpoint guardado o SnakeController no asignado en el Inspector");
        }
    }

    // 🔹 Revivir desde anuncio: siempre desde inicio con 50% de segmentos
    public void RevivirSnakeDesdeAnuncio()
    {
        if (snakeController != null)
        {
            if (!snakeController.gameObject.activeSelf)
                snakeController.gameObject.SetActive(true);
            snakeController.enabled = true;

            snakeController.ReviveFromAd();

            Debug.Log("Snake revivido desde inicio con 50% de segmentos (anuncio)");
        }
        else
        {
            Debug.LogWarning("SnakeController no asignado en el Inspector o no encontrado en runtime");
        }
    }

    public SnakeCheckpointData GetCheckpointData() => checkpointData;
    public void ClearCheckpoint() => checkpointData = null;

    public void GuardarNivelCompletado(int level)
    {
        currentLevel = level;
        int siguiente = level + 1;
        if (siguiente > maxUnlockedLevel)
        {
            maxUnlockedLevel = siguiente;
            PlayerPrefs.SetInt("MaxUnlockedLevel", maxUnlockedLevel);
        }

        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();

        Debug.Log($"Nivel completado: {currentLevel}, máximo desbloqueado: {maxUnlockedLevel}");
    }

    public int GetNivelActual() => currentLevel;
    public int GetMaxUnlockedLevel() => maxUnlockedLevel;
    public int GetSiguienteNivel() => currentLevel + 1;

    public void GameOver()
    {
        Debug.Log("Game Over!");
        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver();
    }
}