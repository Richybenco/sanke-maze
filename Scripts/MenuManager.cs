using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelNivelesPasados;
    public Transform contentNiveles; // el Content del Scroll View
    public Button nivelButtonPrefab; // prefab de botón para cada nivel

    void Start()
    {
        panelNivelesPasados.SetActive(false);
    }

    // Método que se llama al presionar el botón
    public void MostrarNivelesPasados()
    {
        panelNivelesPasados.SetActive(true);

        // limpiar contenido previo
        foreach (Transform child in contentNiveles)
            Destroy(child.gameObject);

        // obtener nivel actual desde GameManager
        int nivelActual = GameManager.Instance.GetNivelActual();

        // generar botones para niveles pasados
        for (int i = 1; i <= nivelActual; i++)
        {
            Button btn = Instantiate(nivelButtonPrefab, contentNiveles);
            btn.GetComponentInChildren<Text>().text = "Nivel " + i;

            int nivelSeleccionado = i;
            btn.onClick.AddListener(() =>
            {
                Debug.Log("Seleccionaste nivel " + nivelSeleccionado);
                // Aquí puedes cargar la escena del nivel seleccionado
                // Ejemplo: SceneManager.LoadScene("Nivel" + nivelSeleccionado);
            });
        }
    }

    public void CerrarPanelNiveles()
    {
        panelNivelesPasados.SetActive(false);
    }
}