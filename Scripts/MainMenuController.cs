using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;   // 🔹 Importante para usar TextMeshPro

public class MainMenuController : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelNivelesPasados;
    public Transform contentNiveles;       // Content del Scroll View
    public Button nivelButtonPrefab;       // Prefab de botón para cada nivel

    [Header("Config")]
    public int totalLevels = 20;           // 🔹 Número total de niveles en tu juego

    // Botón Play → siempre al siguiente nivel
    public void PlayGame()
    {
        int siguienteNivel = GameManager.Instance.GetSiguienteNivel();

        if (siguienteNivel <= totalLevels)
        {
            string sceneName = "Level_" + siguienteNivel.ToString("D2");
            Debug.Log("Play → cargando " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ya no hay más niveles disponibles");
            // Aquí puedes mandar al menú final o créditos
        }
    }

    // Botón Settings
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings"); 
    }

    // Botón Salir
    public void QuitGame()
    {
        Debug.Log("Salir del juego...");
        Application.Quit();
    }

    // Panel de niveles → solo mostrar desbloqueados
    public void MostrarNivelesPasados()
    {
        panelNivelesPasados.SetActive(true);

        // limpiar contenido previo
        foreach (Transform child in contentNiveles)
            Destroy(child.gameObject);

        // 🔹 Obtener el máximo desbloqueado desde GameManager
        int nivelDesbloqueado = Mathf.Max(1, GameManager.Instance.GetMaxUnlockedLevel());

        // 🔹 Generar botones SOLO hasta el nivel desbloqueado
        for (int i = 1; i <= nivelDesbloqueado; i++)
        {
            Button btn = Instantiate(nivelButtonPrefab, contentNiveles);

            // Buscar hijo "NivelText"
            Transform textTransform = btn.transform.Find("NivelText");
            if (textTransform != null)
            {
                TMP_Text nivelText = textTransform.GetComponent<TMP_Text>();
                nivelText.text = "Nivel " + i;
            }
            else
            {
                Debug.LogWarning("No se encontró el hijo 'NivelText' en el prefab del botón");
            }

            // Buscar hijo "LockIcon"
            Transform lockIcon = btn.transform.Find("LockIcon");
            if (lockIcon != null)
            {
                // Ocultar candado porque todos aquí están desbloqueados
                lockIcon.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No se encontró el hijo 'LockIcon' en el prefab del botón");
            }

            int nivelSeleccionado = i;

            // Botón interactivo
            btn.interactable = true;
            btn.onClick.AddListener(() =>
            {
                Debug.Log("Seleccionaste nivel " + nivelSeleccionado);
                SceneManager.LoadScene("Level_" + nivelSeleccionado.ToString("D2"));
            });
        }
    }

    public void CerrarPanelNiveles()
    {
        panelNivelesPasados.SetActive(false);
    }
}