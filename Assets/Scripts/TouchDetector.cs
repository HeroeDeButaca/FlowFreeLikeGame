using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TouchDetector : MonoBehaviour
{
    [SerializeField] private LineRenderer _activeLineRenderer;
    private Node _actualNode;

    public bool CanTouch = true;

    [Tooltip("Lista de LineRenderers existentes en el tablero")]
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    [SerializeField]
    [Tooltip("Prefab del LineRenderer")]
    private GameObject _prefabLineRenderer;

    [SerializeField]
    [Tooltip("Padre del tablero")]
    private Transform _fatherBoard;

    [SerializeField]
    [Tooltip("Almacena las celdas por las que pasa el LineRenderer actual pero en formato de GameObject")]
    private List<GameObject> _cellsPath = new List<GameObject>();

    [HideInInspector]
    [Tooltip("Evento que se ejecutará cuando el tablero este lleno")]
    public UnityEvent OnTableFilled;

    public static TouchDetector Instance;

    void Awake()
    {
        if (Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameController.Instance.OnReset.AddListener(OnReset);
        NodesGenerator.Instance.OnBoardReady.AddListener(delegate
        {
            CanTouch = true;
        });
    }

    void Update()
    {
        if (!CanTouch)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            _cellsPath.Clear();

            GameObject clickedGO = GetMouseGameObjectCell();

            if (clickedGO != null)
            {
                int posIndex;

                if (IsAnActualLineRenderer(clickedGO, out _activeLineRenderer, out posIndex)) // Es un LineRenderer que ya existe
                {
                    // Si hay casillas pintadas las despintamos
                    for (int i = 0; i < _activeLineRenderer.positionCount; i++)
                    {
                        GameObject cell = GetCellGOByPosition(_activeLineRenderer.GetPosition(i));

                        if (cell.transform.GetChild(1).gameObject.activeSelf)
                        {
                            cell.transform.GetChild(1).gameObject.SetActive(false);
                        }
                    }

                    _activeLineRenderer.positionCount = posIndex + 1;

                    // Recoger _cellsPath del lineRenderer
                    for (int i = 0; i < _activeLineRenderer.positionCount; i++)
                    {
                        GameObject cell = GetCellGOByPosition(_activeLineRenderer.GetPosition(i));
                        _cellsPath.Add(cell);
                    }

                    // Averiguar el '_actualNode'
                    Node[] nodes = NodesGenerator.Instance.Nodes.ToArray();
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (nodes[i].AssociedCell.AssociedGO.GetHashCode() == _cellsPath[0].GetHashCode())
                        {
                            _actualNode = nodes[i];
                        }
                    }
                }
                else if (IsThereNode(clickedGO, out _actualNode)) // Crea un nuevo LineRenderer
                {
                    if (_actualNode.NodePair.AssociedLineRenderer != null)
                    {
                        _lineRenderers.Remove(_actualNode.NodePair.AssociedLineRenderer);
                        Destroy(_actualNode.NodePair.AssociedLineRenderer.gameObject);
                    }

                    _activeLineRenderer = Instantiate(_prefabLineRenderer, Vector3.zero,
                        Quaternion.identity, _fatherBoard).GetComponent<LineRenderer>();

                    _activeLineRenderer.SetPosition(0, clickedGO.transform.position);
                    _activeLineRenderer.colorGradient = CreateSimpleColorGradient(_actualNode.NodeColor);

                    _actualNode.AssociedLineRenderer = _activeLineRenderer;

                    _lineRenderers.Add(_activeLineRenderer);
                    _cellsPath.Add(clickedGO);
                }

            }

        }
        else if (Input.GetMouseButton(0))
        {
            if (_cellsPath.Count <= 0)
                return;

            GameObject clickedGO = GetMouseGameObjectCell();

            if (clickedGO != null)
            {
                if (clickedGO.tag == "Estimulo")
                {
                    if (_cellsPath.Last().GetHashCode() != clickedGO.GetHashCode())
                    {
                        int oldPosIndex = -1;

                        if (IsANewCell(clickedGO) && !ItBreaksSomeRule(clickedGO))
                        {
                            _activeLineRenderer.positionCount++;
                            _activeLineRenderer.SetPosition((_activeLineRenderer.positionCount - 1), clickedGO.transform.position);

                            _cellsPath.Add(clickedGO);

                            CorrectLineRendererPath();
                            return;
                        }
                        else if (IsAnOldPos(clickedGO, out oldPosIndex))
                        {
                            GoToOldPosition(oldPosIndex);

                            CorrectLineRendererPath();
                            return;
                        }

                    }

                }

            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_actualNode != null && _activeLineRenderer != null)
            {
                if (IsLineRendererPathCorrect())
                {
                    // Le decimos al nodo y a su pareja que ha hecho un camino correcto
                    _actualNode.IsCorrectPath = true;
                    _actualNode.NodePair.IsCorrectPath = true;

                    _actualNode.ShowAnim();
                    _actualNode.NodePair.ShowAnim();

                    // Poner todo el camino con el color del nodo en el background checker
                    foreach (GameObject cellGO in _cellsPath)
                    {
                        Color nodeColor = _actualNode.NodeColor;
                        nodeColor.a = 0.33f;

                        cellGO.transform.GetChild(1).GetComponent<SpriteRenderer>().color = nodeColor;
                        cellGO.transform.GetChild(1).gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Le decimos al nodo y a su pareja que ha hecho un camino incorrecto
                    _actualNode.IsCorrectPath = false;
                    _actualNode.NodePair.IsCorrectPath = false;

                    if (_activeLineRenderer.positionCount <= 1)
                    {
                        _lineRenderers.Remove(_activeLineRenderer);
                        Destroy(_activeLineRenderer.gameObject);
                    }
                }

                // Desreferenciamos variables
                _actualNode = null;
                _activeLineRenderer = null;
            }

            int tableSquares = BoardGenerator.Instance.GridBoard.Width * BoardGenerator.Instance.GridBoard.Height;

            if (GetTotalPositionsFilled() >= tableSquares && IsAllCorrect())
            {
                Debug.Log("Tablero completado");
                CanTouch = false;
                OnTableFilled?.Invoke();
            }

        }

    }

    /// <summary>
    /// Lanza un raycast y te pasa en forma de GameObject que celda ha tocado
    /// </summary>
    /// <returns>La celda tocada en forma de GameObject</returns>
    private GameObject GetMouseGameObjectCell()
    {
        /*
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("ZonaJuego"))
            {
                Debug.Log("FueraJuego");
            }

            if (hit.collider.CompareTag("Estimulo"))
            {
                //Debug.Log("Target Position: " + hit.collider.gameObject.transform.parent.gameObject.name);
                return hit.collider.gameObject.transform.parent.gameObject;
            }
        }

        return null;
        */
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        Collider2D topCollider = null;

        if (hits.Length > 0)
        {
            RaycastHit2D bestHit = hits[0];
            int bestOrder = int.MinValue;

            foreach (var h in hits)
            {
                var sr = h.collider.GetComponent<SpriteRenderer>();
                int order = sr != null ? sr.sortingOrder : 0;

                if (order > bestOrder)
                {
                    bestOrder = order;
                    bestHit = h;
                }
            }

            topCollider = bestHit.collider;
        }

        //Debug.Log($"Top collider tag: {topCollider.tag}");

        if (topCollider != null)
        {
            if (topCollider.CompareTag("Estimulo"))
            {
                //Debug.Log("Target Position: " + hit.collider.gameObject.transform.parent.gameObject.name);
                return topCollider.gameObject.transform.parent.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Pasando una posición te pasa una celda
    /// </summary>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    private GameObject GetCellGOByPosition(Vector3 cellPos)
    {
        return GameObject.Find($"Cell({cellPos.x},{cellPos.y})");
    }

    /// <summary>
    /// Detecta si lo clickado es un LineRenderer, y devuelve si es así,
    /// el LineRenderer, y la posición del LineRenderer tocada
    /// </summary>
    /// <param name="cellGO"></param>
    /// <param name="actualLineRenderer"></param>
    /// <param name="posIndex"></param>
    /// <returns></returns>
    private bool IsAnActualLineRenderer(GameObject cellGO, out LineRenderer actualLineRenderer, out int posIndex)
    {
        bool isActualLineRenderer = false;
        actualLineRenderer = null;
        posIndex = -1;

        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            for (int j = 0; j < _lineRenderers[i].positionCount; j++)
            {
                if (cellGO.transform.position == _lineRenderers[i].GetPosition(j))
                {
                    posIndex = j;
                    isActualLineRenderer = true;
                    actualLineRenderer = _lineRenderers[i];
                    break;
                }
            }

        }

        if (actualLineRenderer != null)
        {
            GameObject lastCell = GetCellGOByPosition(actualLineRenderer.GetPosition(actualLineRenderer.positionCount - 1));

            if (IsThereNode(lastCell, out Node node))
            {
                // Si hay casillas pintadas las despintamos
                for (int i = 0; i < actualLineRenderer.positionCount; i++)
                {
                    GameObject cell = GetCellGOByPosition(actualLineRenderer.GetPosition(i));

                    if (cell.transform.GetChild(1).gameObject.activeSelf)
                    {
                        cell.transform.GetChild(1).gameObject.SetActive(false);
                    }
                }

                _lineRenderers.Remove(actualLineRenderer);
                Destroy(actualLineRenderer.gameObject);

                isActualLineRenderer = false;
                actualLineRenderer = null;
                posIndex = -1;
            }
        }

        return isActualLineRenderer;
    }

    /// <summary>
    /// Te dice si ha tocado un nodo o no, y te devuelve que nodo ha tocado
    /// </summary>
    /// <param name="cellGO"></param>
    /// <param name="refNode"></param>
    /// <returns></returns>
    private bool IsThereNode(GameObject cellGO, out Node refNode)
    {
        List<Node> nodes = NodesGenerator.Instance.Nodes;
        Node detectedNode = null;

        bool isThereNode = false;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].AssociedCell.AssociedGO.GetHashCode() == cellGO.GetHashCode())
            {
                detectedNode = nodes[i];
                isThereNode = true;
                break;
            }
        }

        refNode = detectedNode;
        return isThereNode;
    }

    /// <summary>
    /// Averigua si ha roto alguna regla
    /// </summary>
    /// <param name="cellGO"></param>
    /// <returns></returns>
    private bool ItBreaksSomeRule(GameObject cellGO)
    {
        bool itBreaksRule = false;
        Vector3 lastCellPos = _cellsPath.Last().transform.position;
        Vector3 actualCellPos = cellGO.transform.position;

        // Averiguamos si ha hecho un movimiento en diagonal
        if (lastCellPos + new Vector3(1, 1, 0) == actualCellPos) { itBreaksRule = true; goto FinalBreakSomeRule; }
        else if (lastCellPos + new Vector3(1, -1, 0) == actualCellPos) { itBreaksRule = true; goto FinalBreakSomeRule; }
        else if (lastCellPos + new Vector3(-1, 1, 0) == actualCellPos) { itBreaksRule = true; goto FinalBreakSomeRule; }
        else if (lastCellPos + new Vector3(-1, -1, 0) == actualCellPos) { itBreaksRule = true; goto FinalBreakSomeRule; }

        // Averiguamos si ha atravesado un nodo
        if (IsThereNode(GetCellGOByPosition(lastCellPos), out Node node) && _cellsPath.Count > 1)
        {
            itBreaksRule = true;
            goto FinalBreakSomeRule;
        }

        // Averiguamos si ha atravesado una linea
        foreach (LineRenderer lr in _lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);

            foreach (Vector3 position in positions)
            {
                if (position == actualCellPos)
                {
                    itBreaksRule = true;
                    goto FinalBreakSomeRule;
                }

            }
        }


    // Esto es una etiqueta. Si se hace un 'goto FinalBreakSomeRule'
    // se ira aquí directamente
    FinalBreakSomeRule:
        return itBreaksRule;
    }

    /// <summary>
    /// Pasando un Color hace un Gradiante que sea como un color plano
    /// </summary>
    /// <param name="basicColor"></param>
    /// <returns></returns>
    private Gradient CreateSimpleColorGradient(Color basicColor)
    {
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        GradientAlphaKey[] gak = new GradientAlphaKey[2];

        gck[0].color = basicColor;
        gck[0].time = 0.0f;
        gck[1].color = basicColor;
        gck[1].time = 1.0f;

        gak[0].alpha = 1.0f;
        gak[0].time = 0.0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 1.0f;

        g.SetKeys(gck, gak);

        return g;
    }

    /// <summary>
    /// Averigua si se ha hecho bien el camino
    /// </summary>
    /// <returns></returns>
    private bool IsLineRendererPathCorrect()
    {
        bool isCorrectPath = false;

        if (_activeLineRenderer.positionCount > 0)
            isCorrectPath = _actualNode.NodePair.AssociedCell.AssociedGO.GetHashCode() == _cellsPath.Last().GetHashCode();

        return isCorrectPath;
    }

    /// <summary>
    /// Averigua si el ratón se ha movido a una celda nueva
    /// que no estuviera en el camino de celdas recorridas
    /// </summary>
    /// <param name="cellGO"></param>
    /// <returns></returns>
    private bool IsANewCell(GameObject cellGO)
    {
        bool isANewCell = true;

        for (int i = 0; i < _cellsPath.Count; i++)
        {
            if (cellGO.GetHashCode() == _cellsPath[i].GetHashCode())
            {
                isANewCell = false;
                break;
            }
        }

        return isANewCell;
    }

    /// <summary>
    /// Averigua si el ratón se ha movido a una celda antigua,
    /// y además devuelve la posición respecto al array de posiciones
    /// que tiene el LineRenderer
    /// </summary>
    /// <param name="cellGO"></param>
    /// <param name="oldPosIndex"></param>
    /// <returns></returns>
    private bool IsAnOldPos(GameObject cellGO, out int oldPosIndex)
    {
        bool isAnOldPos = false;
        oldPosIndex = -1;

        for (int i = 0; i < _activeLineRenderer.positionCount - 1; i++)
        {
            if (_activeLineRenderer.GetPosition(i) == cellGO.transform.position)
            {
                isAnOldPos = true;
                oldPosIndex = i;
                break;
            }
        }

        return isAnOldPos;
    }

    private void CorrectLineRendererPath()
    {
        List<Vector3> correctPositions = new List<Vector3>();

        if (_activeLineRenderer.positionCount <= 0)
            return;

        correctPositions.Add(_activeLineRenderer.GetPosition(0));

        for (int i = 1; i < _activeLineRenderer.positionCount; i++)
        {
            Vector3 lastLRPos = _activeLineRenderer.GetPosition(i - 1);
            Vector3 actualLRPos = _activeLineRenderer.GetPosition(i);

            int xDif = Mathf.RoundToInt(actualLRPos.x - lastLRPos.x);
            int yDif = Mathf.RoundToInt(actualLRPos.y - lastLRPos.y);

            if (xDif > 1) // X positivo
            {
                int initialX = Mathf.RoundToInt(lastLRPos.x + 1);
                int finalX = Mathf.RoundToInt(actualLRPos.x);

                for (int j = initialX; j < finalX; j++)
                {
                    Vector3 phantomPos = new Vector3(j, actualLRPos.y, 0f);
                    correctPositions.Add(phantomPos);
                }

            }
            else if (xDif < -1) // X negativo
            {
                int initialX = Mathf.RoundToInt(lastLRPos.x - 1);
                int finalX = Mathf.RoundToInt(actualLRPos.x);

                for (int j = initialX; j > finalX; j--)
                {
                    Vector3 phantomPos = new Vector3(j, actualLRPos.y, 0f);
                    correctPositions.Add(phantomPos);
                }

            }
            else if (yDif > 1) // Y positivo
            {
                int initialY = Mathf.RoundToInt(lastLRPos.y + 1);
                int finalY = Mathf.RoundToInt(actualLRPos.y);

                for (int j = initialY; j < finalY; j++)
                {
                    Vector3 phantomPos = new Vector3(actualLRPos.x, j, 0f);
                    correctPositions.Add(phantomPos);
                }
            }
            else if (yDif < -1) // Y negativo
            {
                int initialY = Mathf.RoundToInt(lastLRPos.y - 1);
                int finalY = Mathf.RoundToInt(actualLRPos.y);

                for (int j = initialY; j > finalY; j--)
                {
                    Vector3 phantomPos = new Vector3(actualLRPos.x, j, 0f);
                    correctPositions.Add(phantomPos);
                }

            }

            correctPositions.Add(actualLRPos);
        }

        if (_activeLineRenderer.positionCount < correctPositions.Count)
        {
            _cellsPath.Clear();

            for (int i = 0; i < correctPositions.Count; i++)
            {
                GameObject cell = GetCellGOByPosition(correctPositions[i]);
                _cellsPath.Add(cell);
            }

            _activeLineRenderer.positionCount = correctPositions.Count;
            _activeLineRenderer.SetPositions(correctPositions.ToArray());
        }
    }

    /// <summary>
    /// Quita varias partes de camino de golpe para así retroceder
    /// varias celdas
    /// </summary>
    /// <param name="index"></param>
    private void GoToOldPosition(int index)
    {
        //_activeLineRenderer.positionCount = (index+1);
        _cellsPath.RemoveRange(index, _cellsPath.Count - index);
        Vector3[] newLineRendererPath = new Vector3[_cellsPath.Count];

        for (int i = 0; i < _cellsPath.Count; i++)
        {
            newLineRendererPath[i] = _cellsPath[i].transform.position;
        }

        _activeLineRenderer.positionCount = newLineRendererPath.Length;
        _activeLineRenderer.SetPositions(newLineRendererPath);
    }

    private int GetTotalPositionsFilled()
    {
        int totalPositionsFilled = 0;

        foreach (LineRenderer lr in _lineRenderers)
        {
            totalPositionsFilled += lr.positionCount;
        }

        return totalPositionsFilled;
    }

    private bool IsAllCorrect()
    {
        Node[] nodes = NodesGenerator.Instance.Nodes.ToArray();
        foreach (Node node in nodes)
        {
            if (!node.IsCorrectPath)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLineRenderers()
    {
        _lineRenderers.Clear();
    }

    public void AddLineRendererToList(LineRenderer lr)
    {
        _lineRenderers.Add(lr);
    }

    private void OnReset()
    {
        CanTouch = false;
        _activeLineRenderer = null;
        _actualNode = null;

        foreach (LineRenderer lr in _lineRenderers)
        {
            Destroy(lr.gameObject);
        }

        _lineRenderers.Clear();
        _cellsPath.Clear();
    }
}
