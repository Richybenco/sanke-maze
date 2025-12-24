using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class SnakeCheckpointData
{
    public List<Vector3Int> cellHistory;
    public float headAngle;
    public Vector3Int direction;
    public Vector3Int pendingDirection;
}

public class SnakeController : MonoBehaviour
{
    [Header("Prefabs de segmentos")]
    public GameObject straightPrefab;
    public GameObject straightVerticalPrefab;

    [Header("Prefabs de curvas (8 direcciones)")]
    public GameObject curveRightUpPrefab;
    public GameObject curveRightDownPrefab;
    public GameObject curveLeftUpPrefab;
    public GameObject curveLeftDownPrefab;
    public GameObject curveUpRightPrefab;
    public GameObject curveUpLeftPrefab;
    public GameObject curveDownRightPrefab;
    public GameObject curveDownLeftPrefab;

    [Header("Cabeza")]
    public GameObject head;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip turnClip;

    [Header("Tilemap/Grid")]
    public Tilemap floorTilemap;
    public Grid grid;

    [Header("Configuración inicial")]
    public int initialBodyLength = 2;
    public Vector3Int initialDirection = Vector3Int.right;

    [Header("Velocidad")]
    public float moveInterval = 0.25f;
    public int applesPerSpeedUp = 3;
    public float speedIncreasePercent = 0.02f;

    [Header("Interpolación estilo Google Snake")]
    public float interpMultiplier = 1.0f;
    public AnimationCurve interpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Curva rápida para segmentos en curva")]
    public AnimationCurve fastCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.3f, 1),
        new Keyframe(1, 1)
    );

    private List<Vector3Int> cellHistory = new List<Vector3Int>();
    private List<GameObject> segments = new List<GameObject>();
    private List<bool> segmentIsCurve = new List<bool>();

    private Vector3Int direction;
    private Vector3Int pendingDirection;
    private bool growThisStep = false;

    private float moveTimer = 0f;
    private float moveProgress = 1f;
    private bool isMoving = false;

    private Vector3 headFrom, headTo;
    private readonly List<Vector3> segFrom = new List<Vector3>();
    private readonly List<Vector3> segTo = new List<Vector3>();

    private float headAngle = 0f;
    private float targetHeadAngle = 0f;

    private bool isPaused = false;
    private bool waitForFirstInput = false;

    private HashSet<Vector3Int> curveCells = new HashSet<Vector3Int>();

    private Sprite straightSprite;
    private Sprite straightVerticalSprite;
    private Sprite curveRU, curveRD, curveLU, curveLD, curveUR, curveUL, curveDR, curveDL;

    private Vector3Int initialCell; // 🔹 celda inicial guardada

    public static event System.Action SnakeMoved;
    public Vector3Int CurrentCell => cellHistory.Count > 0 ? cellHistory[0] : Vector3Int.zero;

    void Awake()
    {
        straightSprite = straightPrefab?.GetComponent<SpriteRenderer>().sprite;
        straightVerticalSprite = straightVerticalPrefab?.GetComponent<SpriteRenderer>().sprite;

        curveRU = curveRightUpPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveRD = curveRightDownPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveLU = curveLeftUpPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveLD = curveLeftDownPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveUR = curveUpRightPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveUL = curveUpLeftPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveDR = curveDownRightPrefab?.GetComponent<SpriteRenderer>().sprite;
        curveDL = curveDownLeftPrefab?.GetComponent<SpriteRenderer>().sprite;
    }

    void Start()
    {
        // 🔹 Guardar celda inicial del prefab ANTES de inicializar
        initialCell = floorTilemap.WorldToCell(head.transform.position);
        Debug.Log("InitialCell guardada en Start: " + initialCell);

        InicializarSnake();
        RebuildCurveCells();
        PrepareInterpolationBuffers();
        moveProgress = 1f;
        isMoving = false;
    }

    void Update()
    {
        if (isPaused)
        {
            if (waitForFirstInput)
            {
                // 🔹 Tap rápido con mouse
                if (Input.GetMouseButtonDown(0))
                {
                    ResumeGame();
                }
                // 🔹 Input táctil en móvil
                else if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        ResumeGame();
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        HandleSwipe(touch.deltaPosition);
                    }
                }
            }
            return;
        }

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Step();
        }
    }

    void LateUpdate()
    {
        if (!isMoving) return;

        moveProgress += Time.deltaTime * (interpMultiplier / moveInterval);
        float eased = interpCurve.Evaluate(Mathf.Clamp01(moveProgress));

        head.transform.position = Vector3.Lerp(headFrom, headTo, eased);
        headAngle = Mathf.LerpAngle(headAngle, targetHeadAngle, eased);
        head.transform.rotation = Quaternion.Euler(0, 0, headAngle);

        int segCount = Mathf.Min(segments.Count, segFrom.Count);
        for (int i = 0; i < segCount; i++)
        {
            float factor = segmentIsCurve[i] ? fastCurve.Evaluate(Mathf.Clamp01(moveProgress)) : eased;
            segments[i].transform.position = Vector3.Lerp(segFrom[i], segTo[i], factor);
        }

        if (moveProgress >= 1f)
        {
            head.transform.position = headTo;
            headAngle = targetHeadAngle;
            head.transform.rotation = Quaternion.Euler(0, 0, headAngle);

            for (int i = 0; i < segCount; i++)
                segments[i].transform.position = segTo[i];

            isMoving = false;
        }
    }

    // --- Control de input táctil ---
    private void ResumeGame()
    {
        isPaused = false;
        waitForFirstInput = false;
        ResetMoveTimer();
        ForceStep();
    }

    private void HandleSwipe(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 20)
                SetDirection(Vector3Int.right);
            else if (delta.x < -20)
                SetDirection(Vector3Int.left);
        }
        else
        {
            if (delta.y > 20)
                SetDirection(Vector3Int.up);
            else if (delta.y < -20)
                SetDirection(Vector3Int.down);
        }
    }

    // --- Inicializar Snake ---
    public void InicializarSnake()
    {
        direction = initialDirection;
        pendingDirection = initialDirection;
        targetHeadAngle = DirToAngle(initialDirection);
        headAngle = targetHeadAngle;

        cellHistory.Clear();
        segments.Clear();
        segmentIsCurve.Clear();
        curveCells.Clear();

        initialCell = floorTilemap.WorldToCell(head.transform.position);
        cellHistory.Add(initialCell);

        for (int i = 1; i <= initialBodyLength; i++)
        {
            Vector3Int bodyCell = initialCell - direction * i;
            cellHistory.Add(bodyCell);

            GameObject seg = Instantiate(straightPrefab, floorTilemap.GetCellCenterWorld(bodyCell), Quaternion.identity, transform);
            segments.Add(seg);
            segmentIsCurve.Add(false);

            var sr = seg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Vector3Int tailDir = ClampDir(cellHistory[cellHistory.Count - 2] - cellHistory[cellHistory.Count - 1]);
                ApplyStraight(sr, seg, tailDir, true);
            }
        }

        head.transform.rotation = Quaternion.Euler(0, 0, headAngle);
        head.transform.position = floorTilemap.GetCellCenterWorld(initialCell);
    }

    // --- Revivir desde anuncio ---
       public void ReviveFromAd()
    {
        if (initialCell == Vector3Int.zero)
        {
            initialCell = floorTilemap.WorldToCell(head.transform.position);
            Debug.LogWarning("InitialCell recalculado en ReviveFromAd: " + initialCell);
        }

        int currentLength = Mathf.Max(1, cellHistory.Count);
        int reviveLength = Mathf.Max(1, currentLength / 2);

        direction = initialDirection;
        pendingDirection = initialDirection;
        targetHeadAngle = DirToAngle(initialDirection);
        headAngle = targetHeadAngle;

        foreach (var seg in segments) Destroy(seg);
        segments.Clear();
        cellHistory.Clear();
        segmentIsCurve.Clear();
        curveCells.Clear();

        cellHistory.Add(initialCell);

        // 🔹 Recrear cuerpo reducido
        for (int i = 1; i <= reviveLength; i++)
        {
            Vector3Int bodyCell = initialCell - direction * i;
            cellHistory.Add(bodyCell);

            GameObject seg = Instantiate(straightPrefab, floorTilemap.GetCellCenterWorld(bodyCell), Quaternion.identity, transform);
            segments.Add(seg);
            segmentIsCurve.Add(false);

            var sr = seg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Vector3Int tailDir = ClampDir(cellHistory[cellHistory.Count - 2] - cellHistory[cellHistory.Count - 1]);
                ApplyStraight(sr, seg, tailDir, true);
            }
        }

        // 🔹 Colocar cabeza en la celda inicial
        head.transform.rotation = Quaternion.Euler(0, 0, headAngle);
        head.transform.position = floorTilemap.GetCellCenterWorld(initialCell);

        RebuildCurveCells();
        PrepareInterpolationBuffers();
        moveProgress = 1f;
        isMoving = false;

        // 🔹 Pausar hasta primer input (tap o swipe)
        isPaused = true;
        waitForFirstInput = true;
        Debug.Log("ReviveFromAd: Snake pausado, esperando primer input");
    }

            // --- Step y lógica de movimiento ---
    private void Step()
    {
        if (pendingDirection != direction && pendingDirection != -direction)
        {
            direction = pendingDirection;
            targetHeadAngle = DirToAngle(direction);
        }

        Vector3Int nextCell = cellHistory[0] + direction;
        cellHistory.Insert(0, nextCell);

        if (growThisStep)
        {
            growThisStep = false;
            Vector3Int newBodyCell = cellHistory[1];
            GameObject seg = Instantiate(straightPrefab, floorTilemap.GetCellCenterWorld(newBodyCell), Quaternion.identity, transform);
            segments.Insert(0, seg);
            segmentIsCurve.Insert(0, false);
        }
        else
        {
            cellHistory.RemoveAt(cellHistory.Count - 1);
        }

        RebuildCurveCells();

        int count = Mathf.Min(segments.Count, cellHistory.Count - 1);
        for (int i = 0; i < count; i++)
            DecideAndApplySegmentVisual(i);

        PrepareInterpolationBuffers();
        moveProgress = 0f;
        isMoving = true;

        SnakeMoved?.Invoke();
    }

    private void PrepareInterpolationBuffers()
    {
        headFrom = head.transform.position;
        headTo = floorTilemap.GetCellCenterWorld(cellHistory[0]);

        segFrom.Clear();
        segTo.Clear();

        int count = Mathf.Min(segments.Count, cellHistory.Count - 1);
        for (int i = 0; i < count; i++)
        {
            Vector3Int targetCell = cellHistory[i + 1];
            segFrom.Add(segments[i].transform.position);
            segTo.Add(floorTilemap.GetCellCenterWorld(targetCell));
        }
    }

    private void DecideAndApplySegmentVisual(int index)
    {
        if (index + 1 >= cellHistory.Count) return;

        Vector3Int prev = (index == 0) ? cellHistory[0] : cellHistory[index];
        Vector3Int current = cellHistory[index + 1];
        Vector3Int next = (index + 2 < cellHistory.Count) ? cellHistory[index + 2] : current;

        Vector3Int dir1 = ClampDir(current - prev);
        Vector3Int dir2 = ClampDir(next - current);

        var sr = segments[index].GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (curveCells.Contains(current))
        {
            ApplyCurve(sr, segments[index], dir1, dir2, current);
            segmentIsCurve[index] = true;
        }
        else
        {
            ApplyStraight(sr, segments[index], dir1, true);
            segmentIsCurve[index] = false;
        }
    }

    private void RebuildCurveCells()
    {
        curveCells.Clear();

        int max = cellHistory.Count - 2;
        for (int i = 0; i < max; i++)
        {
            Vector3Int prev = cellHistory[i];
            Vector3Int current = cellHistory[i + 1];
            Vector3Int next = cellHistory[i + 2];

            Vector3Int dir1 = ClampDir(current - prev);
            Vector3Int dir2 = ClampDir(next - current);

            if (dir1 != dir2)
                curveCells.Add(current);
        }

        // Caso especial cola
        if (cellHistory.Count >= 3)
        {
            Vector3Int tailPrev = cellHistory[cellHistory.Count - 3];
            Vector3Int tailCurrent = cellHistory[cellHistory.Count - 2];
            Vector3Int tailNext = cellHistory[cellHistory.Count - 1];

            Vector3Int tDir1 = ClampDir(tailCurrent - tailPrev);
            Vector3Int tDir2 = ClampDir(tailNext - tailCurrent);

            if (tDir1 != tDir2)
                curveCells.Add(tailCurrent);
        }
    }

    private void ApplyCurve(SpriteRenderer sr, GameObject segment, Vector3Int dir1, Vector3Int dir2, Vector3Int currentCell)
    {
        Sprite s = GetCurveSprite(dir1, dir2);
        if (s != null)
        {
            sr.sprite = s;
            segment.transform.rotation = Quaternion.identity;
            segment.transform.position = floorTilemap.GetCellCenterWorld(currentCell);
        }
        else
        {
            ApplyStraight(sr, segment, dir1, true);
        }
    }

    private Sprite GetCurveSprite(Vector3Int dir1, Vector3Int dir2)
    {
        if (dir1 == Vector3Int.right && dir2 == Vector3Int.up) return curveRU;
        if (dir1 == Vector3Int.right && dir2 == Vector3Int.down) return curveRD;
        if (dir1 == Vector3Int.left && dir2 == Vector3Int.up) return curveLU;
        if (dir1 == Vector3Int.left && dir2 == Vector3Int.down) return curveLD;
        if (dir1 == Vector3Int.up && dir2 == Vector3Int.right) return curveUR;
        if (dir1 == Vector3Int.up && dir2 == Vector3Int.left) return curveUL;
        if (dir1 == Vector3Int.down && dir2 == Vector3Int.right) return curveDR;
        if (dir1 == Vector3Int.down && dir2 == Vector3Int.left) return curveDL;
        return null;
    }

    private void ApplyStraight(SpriteRenderer sr, GameObject segment, Vector3Int dir, bool forceVerticalIfY)
    {
        if (forceVerticalIfY && dir.y != 0 && straightVerticalSprite != null)
        {
            sr.sprite = straightVerticalSprite;
            segment.transform.rotation = Quaternion.identity;
        }
        else
        {
            if (straightSprite != null) sr.sprite = straightSprite;
            segment.transform.rotation = (forceVerticalIfY && dir.y != 0 && straightVerticalSprite == null)
                ? Quaternion.Euler(0, 0, 90f)
                : Quaternion.identity;
        }
    }

    private Vector3Int ClampDir(Vector3Int d)
    {
        int x = d.x == 0 ? 0 : (d.x > 0 ? 1 : -1);
        int y = d.y == 0 ? 0 : (d.y > 0 ? 1 : -1);
        return new Vector3Int(x, y, 0);
    }

    private float DirToAngle(Vector3Int dir)
    {
        if (dir == Vector3Int.right) return 0f;
        if (dir == Vector3Int.up) return 90f;
        if (dir == Vector3Int.left) return 180f;
        if (dir == Vector3Int.down) return 270f;
        return headAngle;
    }

    // --- API pública ---
    public void ForceStep() => Step();

    public void SetDirection(Vector3Int newDir)
    {
        if (newDir == -direction) return;

        if (newDir != pendingDirection)
        {
            pendingDirection = newDir;
            targetHeadAngle = DirToAngle(pendingDirection);

            if (audioSource != null && turnClip != null)
                audioSource.PlayOneShot(turnClip);
        }
    }

    public void Grow() => growThisStep = true;
    public void PauseSnake() => isPaused = true;
    public void ResumeAfterInput() => isPaused = false;
    public void ResetMoveTimer() => moveTimer = moveInterval - 0.001f;

    public void ResetDirection()
    {
        direction = initialDirection;
        pendingDirection = initialDirection;
        targetHeadAngle = DirToAngle(initialDirection);
        headAngle = targetHeadAngle;
        head.transform.rotation = Quaternion.Euler(0, 0, headAngle);
    }

    public SnakeCheckpointData GetCheckpoint()
    {
        return new SnakeCheckpointData
        {
            cellHistory = new List<Vector3Int>(cellHistory),
            headAngle = headAngle,
            direction = direction,
            pendingDirection = pendingDirection
        };
    }

    public void RestoreCheckpoint(SnakeCheckpointData data)
    {
        if (data == null) return;

        cellHistory = new List<Vector3Int>(data.cellHistory);

        foreach (var seg in segments) Destroy(seg);
        segments.Clear();
        segmentIsCurve.Clear();
        curveCells.Clear();

        headAngle = data.headAngle;
        targetHeadAngle = DirToAngle(data.direction);
        head.transform.rotation = Quaternion.Euler(0, 0, headAngle);
        head.transform.position = floorTilemap.GetCellCenterWorld(cellHistory[0]);

        for (int i = 1; i < cellHistory.Count; i++)
        {
            Vector3 pos = floorTilemap.GetCellCenterWorld(cellHistory[i]);
            GameObject seg = Instantiate(straightPrefab, pos, Quaternion.identity, transform);
            segments.Add(seg);
            segmentIsCurve.Add(false);

            var sr = seg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Vector3Int dir = ClampDir(cellHistory[i - 1] - cellHistory[i]);
                ApplyStraight(sr, seg, dir, true);
            }
        }

        direction = data.direction;
        pendingDirection = data.pendingDirection;

        RebuildCurveCells();
        PrepareInterpolationBuffers();
        moveProgress = 1f;
        isMoving = false;

                // Pausar hasta que el jugador toque/clickee
        isPaused = true;
        waitForFirstInput = true;
    }

    public List<Vector3Int> GetCellHistory() => cellHistory;
    public List<GameObject> GetBodySegments() => segments;

    public void RevertLastStep()
    {
        if (cellHistory.Count == 0) return;

        cellHistory.RemoveAt(0);

        int count = Mathf.Min(segments.Count, cellHistory.Count - 1);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = floorTilemap.GetCellCenterWorld(cellHistory[i + 1]);
            segments[i].transform.position = pos;
        }

        RebuildCurveCells();
        PrepareInterpolationBuffers();
        moveProgress = 1f;
        isMoving = false;
    }

    public void ReduceGrowth(int segmentsToRemove)
    {
        for (int i = 0; i < segmentsToRemove && segments.Count > 0; i++)
        {
            int last = segments.Count - 1;
            GameObject seg = segments[last];
            segments.RemoveAt(last);
            segmentIsCurve.RemoveAt(last);
            Destroy(seg);

            if (cellHistory.Count > 1)
                cellHistory.RemoveAt(cellHistory.Count - 1);
        }
    }

    public void IncreaseSpeed()
    {
        moveInterval *= (1f - speedIncreasePercent);

        // 🔹 Evitar que la velocidad sea demasiado alta
        if (moveInterval < 0.05f)
            moveInterval = 0.05f;

        Debug.Log("Nueva velocidad del Snake (intervalo): " + moveInterval);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Apple"))
        {
            Grow();

            // 🔹 Aumentar velocidad cada cierto número de manzanas
            if (GameManager.Instance != null && GameManager.Instance.GetNivelActual() % applesPerSpeedUp == 0)
            {
                IncreaseSpeed();
            }

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Body"))
        {
            Debug.Log("Colisión: Game Over");
            var gm = GameManager.Instance;
            if (gm != null) gm.GameOver();
        }
    }
}