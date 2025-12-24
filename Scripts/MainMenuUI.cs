using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;   // 🔹 Importante para usar TextMeshPro

public class MainMenuUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelNivelesPasados;
    [SerializeField] private Transform contentNiveles;
    [SerializeField] private Button nivelButtonPrefab;

    void Awake()
    {
        if (panelNivelesPasados == null)
        {
            var go = GameObject.Find("PanelNivelesPasados");
            if (go != null) panelNivelesPasados = go;
        }

        if (contentNiveles == null && panelNivelesPasados != null)
        {
            var scroll = panelNivelesPasados.GetComponentInChildren<ScrollRect>(true);
            if (scroll != null) contentNiveles = scroll.content;
        }

        if (panelNivelesPasados == null)
            Debug.LogError("MainMenuUI: panelNivelesPasados no asignado en el Inspector.");
        if (contentNiveles == null)
            Debug.LogError("MainMenuUI: contentNiveles (Scroll Content) no asignado en el Inspector.");
        if (nivelButtonPrefab == null)
            Debug.LogError("MainMenuUI: nivelButtonPrefab no asignado en el Inspector.");
    }

    // 🔹 Botón Play → siempre al siguiente nivel
    public void OnPlayButtonPressed()
    {
        int siguienteNivel = GameManager.Instance.GetSiguienteNivel();
        string sceneName = "Level_" + siguienteNivel.ToString("00");

        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("No existe la escena: " + sceneName);
    }

    // 🔹 Panel → mostrar solo niveles desbloqueados
    public void OnNivelesPasadosButtonPressed()
    {
        if (panelNivelesPasados == null || contentNiveles == null || nivelButtonPrefab == null)
        {
            Debug.LogError("MainMenuUI: faltan referencias (panel/content/prefab). Revisa el Inspector.");
            return;
        }

        panelNivelesPasados.SetActive(true);

        foreach (Transform child in contentNiveles)
            Destroy(child.gameObject);

        int maxUnlocked = GameManager.Instance.GetMaxUnlockedLevel();

        for (int i = 1; i <= maxUnlocked; i++)
        {
            Button btn = Instantiate(nivelButtonPrefab, contentNiveles);

            // 🔹 Buscar hijo "NivelText" (TextMeshProUGUI)
            TMP_Text nivelText = btn.transform.Find("NivelText")?.GetComponent<TMP_Text>();
            if (nivelText != null)
                nivelText.text = "Nivel " + i;
            else
                Debug.LogWarning("Prefab del botón no tiene hijo 'NivelText' con TMP_Text");

            int nivelSeleccionado = i;
            btn.onClick.AddListener(() =>
            {
                string sceneName = "Level_" + nivelSeleccionado.ToString("00");
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                    SceneManager.LoadScene(sceneName);
                else
                    Debug.LogWarning("No existe la escena: " + sceneName);
            });

            // 🔹 Ocultar candado porque todos aquí están desbloqueados
            Transform lockIcon = btn.transform.Find("LockIcon");
            if (lockIcon != null) lockIcon.gameObject.SetActive(false);
        }
    }

    public void OnCerrarNivelesPasados()
    {
        if (panelNivelesPasados == null)
        {
            Debug.LogError("MainMenuUI: panelNivelesPasados no asignado.");
            return;
        }
        panelNivelesPasados.SetActive(false);
    }

    public void OnQuitButtonPressed()
    {
        Debug.Log("Salir del juego...");
        Application.Quit();
    }
}