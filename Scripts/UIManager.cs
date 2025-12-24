using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static event System.Action SnakeCrashed;

    [Header("UI References")]
    public TMP_Text appleCounterText;
    public TMP_Text coinCounterText;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public SnakeController snakeController;

    [Header("Game Over Buttons")]
    public GameObject watchVideoButton;

    [Header("Currency Settings")]
    public int appleValue = 10;

    private int coinsTotal = 0;
    private int coinsLevelDelta = 0;

    private bool waitingForInput = false;

    private int applesCollected = 0;
    private int applesTotal = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (watchVideoButton != null) watchVideoButton.SetActive(false);

        coinsTotal = PlayerPrefs.GetInt("CoinsTotal", 0);
        coinsLevelDelta = 0;
        UpdateCoinUI();
    }

    void Update()
    {
        if (waitingForInput)
        {
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                if (snakeController != null)
                    snakeController.ResumeAfterInput();

                waitingForInput = false;
            }
        }
    }

    public void UpdateAppleCounter(int collected, int total)
    {
        applesCollected = collected;
        applesTotal = total;

        if (appleCounterText != null)
            appleCounterText.text = collected.ToString("00") + "/" + total.ToString("00");

        AddCoinsToLevelDelta(appleValue);
    }

    public void AddCoinsToLevelDelta(int amount)
    {
        coinsLevelDelta += amount;
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        if (coinCounterText != null)
            coinCounterText.text = (coinsTotal + coinsLevelDelta).ToString("000");
    }

    private void ConsolidateLevelCoins()
    {
        if (coinsLevelDelta > 0)
        {
            coinsTotal += coinsLevelDelta;
            coinsLevelDelta = 0;
            PlayerPrefs.SetInt("CoinsTotal", coinsTotal);
            PlayerPrefs.Save();
        }
        UpdateCoinUI();
    }

    private void DiscardLevelDelta()
    {
        coinsLevelDelta = 0;
        UpdateCoinUI();
        PlayerPrefs.SetInt("CoinsTotal", coinsTotal);
        PlayerPrefs.Save();
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (snakeController != null)
            snakeController.PauseSnake();

        if (watchVideoButton != null)
            watchVideoButton.SetActive(true);

        DiscardLevelDelta();
        SnakeCrashed?.Invoke();
    }

    public void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (snakeController != null)
            snakeController.PauseSnake();

        ConsolidateLevelCoins();

        string currentScene = SceneManager.GetActiveScene().name;
        string[] parts = currentScene.Split('_');
        if (parts.Length > 1 && int.TryParse(parts[1], out int levelNumber))
        {
            GameManager.Instance.GuardarNivelCompletado(levelNumber);
        }
    }

    // 🔹 Revivir desde anuncio: siempre desde inicio
  public void OnWatchVideoButtonPressed()
{
    if (AdManager.Instance != null)
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            Debug.Log("Jugador vio el anuncio, revive desde inicio con 50% de segmentos");

            // 🔹 Ocultar panel de Game Over y botón de video
            if (gameOverPanel != null) 
                gameOverPanel.SetActive(false);
            
            if (watchVideoButton != null) 
                watchVideoButton.SetActive(false);

            // 🔹 Limpiar selección de UI para evitar bloqueo de input
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            // 🔹 Revivir Snake desde inicio con 50% de segmentos
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RevivirSnakeDesdeAnuncio();
                Debug.Log("Snake revivido correctamente desde prefab inicial");
            }
            else
            {
                Debug.LogWarning("GameManager.Instance es null, no se pudo revivir Snake");
            }

            // 🔹 Actualizar contador de manzanas en UI
            if (appleCounterText != null)
                appleCounterText.text = applesCollected.ToString("00") + "/" + applesTotal.ToString("00");

            Debug.Log("Jugador debe recolectar " + (applesTotal - applesCollected) + " manzanas restantes");

            // 🔹 Asegurar que el flujo espere input antes de mover
            waitingForInput = true;
        });
    }
    else
    {
        Debug.LogWarning("AdManager.Instance es null, no se pudo mostrar anuncio");
    }
}

    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
    public void RestartLevel()
    {
        DiscardLevelDelta();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string[] parts = currentScene.Split('_');
        if (parts.Length > 1 && int.TryParse(parts[1], out int levelNumber))
        {
            int nextLevel = levelNumber + 1;
            string nextSceneName = parts[0] + "_" + nextLevel.ToString("00");
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else
                Debug.LogWarning("No existe la escena: " + nextSceneName);
        }
    }
    public void GoToSettings() => SceneManager.LoadScene("Settings");
}