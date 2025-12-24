using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SafeAreaApplier : MonoBehaviour
{
    [Tooltip("Opcional: si tu UI principal está dentro de otro contenedor, ponlo aquí.")]
    public RectTransform target; // El contenedor que ajustaremos (si no se asigna, usa el propio RectTransform)

    [Header("Opcional")]
    public bool debugDrawGizmo = false;

    private RectTransform _rt;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private ScreenOrientation _lastOrientation;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (target == null) target = _rt;
        ApplySafeArea();
    }

    void OnEnable()
    {
        ApplySafeArea();
    }

    void Update()
    {
        // Reaplicar si cambian: safe area, tamaño de pantalla u orientación
        Rect safe = Screen.safeArea;
        Vector2Int screen = new Vector2Int(Screen.width, Screen.height);
        ScreenOrientation orient = Screen.orientation;

        if (safe != _lastSafeArea || screen != _lastScreenSize || orient != _lastOrientation)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safe = Screen.safeArea;
        _lastSafeArea = safe;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastOrientation = Screen.orientation;

        // Convertir de píxeles a anchors 0..1
        var parent = target.parent as RectTransform;
        if (parent == null)
        {
            Debug.LogWarning("SafeAreaApplier: El target no tiene RectTransform padre. Asegúrate de que esté dentro de un Canvas/RectTransform.");
            return;
        }

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        // Normalizar por tamaño del padre
        anchorMin.x /= parent.rect.width;
        anchorMin.y /= parent.rect.height;
        anchorMax.x /= parent.rect.width;
        anchorMax.y /= parent.rect.height;

        target.anchorMin = anchorMin;
        target.anchorMax = anchorMax;
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (!debugDrawGizmo) return;
        Gizmos.color = Color.green;
        Rect s = Screen.safeArea;

        // Dibuja un marco aproximado (en espacio de pantalla, solo útil en editor)
        Gizmos.DrawWireCube(new Vector3(s.center.x, s.center.y, 0), new Vector3(s.size.x, s.size.y, 0));
    }
}