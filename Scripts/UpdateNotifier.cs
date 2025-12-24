using UnityEngine;
using UnityEngine.UI;
using TMPro; // 👈 Importante

public class UpdateNotifier : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject updatePanel;
    public TextMeshProUGUI updateText;   // 👈 Cambiado a TMP
    public Button updateButton;

    [Header("Config")]
    public string updateMessage = "¡Nueva actualización disponible!";
    public string googlePlayUrl = "https://play.google.com/store/apps/details?id=com.tuempresa.tujuego";

    void Start()
    {
        ShowUpdateMessage();
        if (updateButton != null)
            updateButton.onClick.AddListener(OpenStorePage);
    }

    public void ShowUpdateMessage()
    {
        if (updatePanel != null)
        {
            updatePanel.SetActive(true);
            if (updateText != null)
                updateText.text = updateMessage;
        }
    }

    private void OpenStorePage()
    {
        Application.OpenURL(googlePlayUrl);
    }
}