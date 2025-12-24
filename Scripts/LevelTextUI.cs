using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTextUI : MonoBehaviour
{
    public TMP_Text levelText;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name; // ej: "_level_01"
        string levelNumber = ExtractLevelNumber(sceneName);

        if (levelText != null)
            levelText.text = "Nivel " + levelNumber;
    }

    private string ExtractLevelNumber(string sceneName)
    {
        // Busca el número después de "_level_"
        string[] parts = sceneName.Split('_');
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int number))
            {
                return number.ToString();
            }
        }
        return "?"; // fallback si no encuentra número
    }
}