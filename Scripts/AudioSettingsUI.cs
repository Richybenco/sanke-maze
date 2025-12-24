using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI Toggles")]
    public Toggle toggleMove;
    public Toggle toggleEat;
    public Toggle toggleCrash;

    [Header("Audio Manager Reference")]
    public SnakeAudioManager audioManager;

    void Start()
    {
        // Inicializa los toggles según el estado actual
        toggleMove.isOn = audioManager.enableMove;
        toggleEat.isOn = audioManager.enableEat;
        toggleCrash.isOn = audioManager.enableCrash;

        // Suscribir eventos
        toggleMove.onValueChanged.AddListener(OnToggleMove);
        toggleEat.onValueChanged.AddListener(OnToggleEat);
        toggleCrash.onValueChanged.AddListener(OnToggleCrash);
    }

    void OnToggleMove(bool value) => audioManager.enableMove = value;
    void OnToggleEat(bool value) => audioManager.enableEat = value;
    void OnToggleCrash(bool value) => audioManager.enableCrash = value;
}