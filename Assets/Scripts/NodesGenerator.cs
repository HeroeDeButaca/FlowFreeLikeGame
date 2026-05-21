using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;

public class NodesGenerator : MonoBehaviour
{
    public List<Node> Nodes = new List<Node>();

    [Header("Seed system")]
    private System.Random _rng;

    [SerializeField]
    private int _seedSize = 6;

    [SerializeField]
    private TMP_Text _seedText;

    [Space]
    [Header("Node Colors")]

    [SerializeField]
    private Color[] _possibleColors;

    [Space]
    [Header("Iterator values")]

    [SerializeField]
    private int _minIterations = 75;
    private int _maxIterations = 200;

    [SerializeField]
    private Image _bgrLoadingImage;

    [Space]
    [Header("Prefabs")]

    [SerializeField]
    [Tooltip("Prefab de los nodos")]
    private GameObject _prefabNode;

    [SerializeField]
    [Tooltip("Prefab del LineRenderer")]
    private GameObject _prefabLineRenderer;

    [SerializeField]
    [Tooltip("Padre del tablero")]
    private Transform _fatherBoard;

    private bool _boardReady = false;

    [HideInInspector]
    public UnityEvent OnBoardReady;

    public static NodesGenerator Instance;

    void Awake()
    {
        if (Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        //GameControllerCPR.Instance.OnReset.AddListener(OnReset);
    }

    public void CreateCorrectBoard(int nodes)
    {
        // Obtenemos una seed
        _rng = new System.Random(GetSeed());

        // Paso 2: Crea una solución predeterminada (todas filas verticales o horizontales)
        Grid grid = BoardGenerator.Instance.GridBoard;

        bool verticalInstance = grid.Height >= grid.Width;
        int nodesToCreate = 0;

        if (verticalInstance)
            nodesToCreate = Mathf.Min(nodes, grid.Height);
        else
            nodesToCreate = Mathf.Min(nodes, grid.Width);

        HashSet<Color> usedColors = new HashSet<Color>();

        for (int i = 0; i < nodesToCreate; i++)
        {
            GameObject headGO = Instantiate(_prefabNode);
            headGO.name = $"Head Node({i})";

            GameObject tailGO = Instantiate(_prefabNode);
            tailGO.name = $"Tail Node({i})";

            Node tailNode = null;
            Node headNode = null;

            Color selectedColor = Color.black;

            do
            {
                int rand = _rng.Next(0, _possibleColors.Length);
                selectedColor = _possibleColors[rand];
            } while (!usedColors.Add(selectedColor));

            if (verticalInstance)
            {
                tailGO.transform.position = new Vector2(i, 0);
                tailGO.transform.parent = grid.Cells[i, 0].AssociedGO.transform;
                grid.Cells[i, 0].IsFilled = true;

                headGO.transform.position = new Vector2(i, (grid.Height - 1));
                headGO.transform.parent = grid.Cells[i, (grid.Height - 1)].AssociedGO.transform;
                grid.Cells[i, (grid.Height - 1)].IsFilled = true;

                SpriteRenderer tailSprRend = tailGO.GetComponentInChildren<SpriteRenderer>();
                tailSprRend.color = selectedColor;

                tailNode = new Node(i, selectedColor, tailSprRend, grid.Cells[i, 0]);

                for (int j = 0; j < grid.Height; j++)
                {
                    tailNode.SolutionPath.Add(grid.Cells[i, j]);
                }

                SpriteRenderer headSprRend = headGO.GetComponentInChildren<SpriteRenderer>();
                headSprRend.color = selectedColor;

                headNode = new Node(i, selectedColor, headSprRend, grid.Cells[i, (grid.Height - 1)]);

                for (int j = (grid.Height - 1); j >= 0; j--)
                {
                    headNode.SolutionPath.Add(grid.Cells[i, j]);
                }

            }
            else
            {
                tailGO.transform.position = new Vector2(0, i);
                tailGO.transform.parent = grid.Cells[0, i].AssociedGO.transform;
                grid.Cells[0, i].IsFilled = true;

                headGO.transform.position = new Vector2((grid.Width - 1), i);
                headGO.transform.parent = grid.Cells[(grid.Width - 1), i].AssociedGO.transform;
                grid.Cells[(grid.Width - 1), i].IsFilled = true;

                SpriteRenderer tailSprRend = tailGO.GetComponentInChildren<SpriteRenderer>();
                tailSprRend.color = selectedColor;

                tailNode = new Node(i, Color.black, tailSprRend, grid.Cells[0, i]);

                for (int j = 0; j < grid.Width; j++)
                {
                    tailNode.SolutionPath.Add(grid.Cells[j, i]);
                }

                SpriteRenderer headSprRend = headGO.GetComponentInChildren<SpriteRenderer>();
                headSprRend.color = selectedColor;

                headNode = new Node(i, Color.black, headSprRend, grid.Cells[(grid.Width - 1), i]);

                for (int j = (grid.Width - 1); j >= 0; j--)
                {
                    headNode.SolutionPath.Add(grid.Cells[j, i]);
                }

            }

            tailNode.NodePair = headNode;
            headNode.NodePair = tailNode;

            tailNode.NodeAnim = tailGO.GetComponent<Animator>();
            tailGO.transform.GetChild(1).GetComponent<SpriteRenderer>().color = selectedColor;

            headNode.NodeAnim = headGO.GetComponent<Animator>();
            headGO.transform.GetChild(1).GetComponent<SpriteRenderer>().color = selectedColor;

            Nodes.Add(tailNode);
            Nodes.Add(headNode);

        }

        StartCoroutine(WaitUntilBoardReady());
    }

    private IEnumerator WaitUntilBoardReady()
    {
        _bgrLoadingImage.fillAmount = 1f;
        _bgrLoadingImage.raycastTarget = true;

        BoardGenerator.Instance.StartChangeColor();

        //StartCoroutine(IteratorBoard());
        //yield return new WaitUntil(() => _boardReady);
        yield return IteratorBoard();

        BoardGenerator.Instance.StopChangeColor();

        OnBoardReady?.Invoke();

        _bgrLoadingImage.fillAmount = 0f;
        _bgrLoadingImage.raycastTarget = false;


    }

    /// <summary>
    /// Se encarga de hacer que los nodos iteren todo el rato entre ellos para generar un tablero
    /// </summary>
    /// <returns></returns>
    private IEnumerator IteratorBoard()
    {
        int totalIterations = _rng.Next(_minIterations, _maxIterations);
        float exponent = Random.Range(0.23f, 0.35f);

        for (int i = 0; i < totalIterations; i++)
        {
            // Paso 4.1 cogemos uno de los extremos (cabeza o cola)
            List<Node> neighborNodesPositions = new List<Node>();
            Node randomNode = null;

            do
            {
                neighborNodesPositions.Clear();

                int randomNodeAttemps = 0;
                do
                {
                    randomNode = Nodes[_rng.Next(0, Nodes.Count)];
                    randomNodeAttemps++;
                } while (randomNode.SolutionPath.Count <= 3 && randomNodeAttemps < 100);

                if (randomNode.AssociedCell.IsFilled)
                {
                    foreach (Cell neighbor in randomNode.AssociedCell.Neighbors)
                    {
                        if (neighbor.IsFilled)
                        {
                            for (int j = 0; j < Nodes.Count; j++)
                            {
                                if (neighbor.Position == Nodes[j].AssociedCell.Position)
                                {
                                    neighborNodesPositions.Add(Nodes[j]);
                                }
                            }
                        }
                    }
                }

            } while (neighborNodesPositions.Count <= 0); // Hasta que coja una cola/cabeza que ha su alrededor tenga otro nodo con cola/cabeza

            yield return null;

            // Paso 4.2: Empequeñecemos esa parte y actualizamos la pareja de ese mismo nodo
            Cell previousCell = randomNode.AssociedCell;

            previousCell.IsFilled = false;
            /*
            Debug.Log($"Pre-Remove:\n" +
                $"SolutionPath Node {randomNode.SpriteRend.transform.parent.gameObject.name} Length: {randomNode.SolutionPath.Count}");
            */
            randomNode.AssociedCell = randomNode.SolutionPath[1];
            randomNode.AssociedCell.IsFilled = true;

            randomNode.SolutionPath.RemoveAt(0);
            //randomNode.NodePair.SolutionPath.RemoveAt((randomNode.NodePair.SolutionPath.Count-1));
            randomNode.NodePair.SolutionPath.Remove(randomNode.NodePair.SolutionPath.Last());

            Transform nodeTransf = randomNode.SpriteRend.gameObject.transform.parent.transform;
            Transform cellTransf = randomNode.AssociedCell.AssociedGO.transform;

            nodeTransf.parent = cellTransf;
            nodeTransf.localPosition = Vector3.zero;

            /*
            Debug.Log($"Post-Remove:\n" +
                $"SolutionPath Node {randomNode.SpriteRend.transform.parent.gameObject.name} Length: {randomNode.SolutionPath.Count}");

            for(int j = 0; j < randomNode.SolutionPath.Count; j++)
            {
                Debug.Log($"RandNode SolutionPath {j}: {randomNode.SolutionPath[j].AssociedGO.name}");
            }
            */

            // Paso 5: Eligimos uno de sus antiguos vecinos y hacemos que ocupe su espacio
            int randNeighbor = _rng.Next(0, neighborNodesPositions.Count);

            Node selectedNeighbor = neighborNodesPositions[randNeighbor];

            selectedNeighbor.AssociedCell.IsFilled = false;
            nodeTransf = selectedNeighbor.SpriteRend.gameObject.transform.parent.transform;

            selectedNeighbor.AssociedCell = previousCell;
            selectedNeighbor.AssociedCell.IsFilled = true;

            nodeTransf.parent = selectedNeighbor.AssociedCell.AssociedGO.transform;
            nodeTransf.localPosition = Vector3.zero;

            /*
            Debug.Log($"Pre-Remove:\n" +
                $"SolutionPath Neighbor Node {selectedNeighbor.SpriteRend.transform.parent.gameObject.name} Length: {selectedNeighbor.SolutionPath.Count}");
            */

            // Paso 6: Rehacemos la solución de la pareja elegida y actualizamos la pareja de ese mismo nodo
            selectedNeighbor.SolutionPath.Insert(0, previousCell);
            selectedNeighbor.NodePair.SolutionPath.Add(previousCell);

            /*
            Debug.Log($"Post-Remove:\n" +
                $"SolutionPath Neighbor Node {selectedNeighbor.SpriteRend.transform.parent.gameObject.name} Length: {selectedNeighbor.SolutionPath.Count}");

            for (int j = 0; j < selectedNeighbor.SolutionPath.Count; j++)
            {
                Debug.Log($"Neighbor node SolutionPath {j}: {selectedNeighbor.SolutionPath[j].AssociedGO.name}");
            }
            */

            // Paso 7: Repetimos el proceso hasta que no tenga que hacer más iteraciones
            float t = (float)i / totalIterations;
            float eased = 1f - Mathf.Pow(t, exponent);
            _bgrLoadingImage.fillAmount = eased;
            yield return null;
        }

        // Decimos que ha terminado de generar el tablero
        _boardReady = true;
    }

    /// <summary>
    /// Función que sirve para obtener una semilla de caracteres que muestra por
    /// pantalla si estas en Editor o en la DevBuild, y lo devuelve en Int 
    /// convirtiendolo en un HashCode.
    /// </summary>
    /// <returns></returns>
    private int GetSeed()
    {
        //string seedStr = "U4SSJY";
        string seedStr = SeedGenerator.GenerateSeed(_seedSize);

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        _seedText.SetText(seedStr);
#endif

        Debug.Log($"Seed: {seedStr}");

        int seed = seedStr.GetHashCode();
        return seed;
    }

    private void ClearNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Destroy(Nodes[i].SpriteRend.transform.parent.gameObject);
        }

        Nodes.Clear();
    }

    public void ShowCorrection()
    {
        StartCoroutine(CorrectionCoroutine());
    }

    private IEnumerator CorrectionCoroutine()
    {
        // Tapamos todas las celdas para que no quede feo
        yield return BoardGenerator.Instance.ShowLoadingColor(true);

        // Si hay alguno con camino ya creado lo borramos
        for (int i = 0; i < Nodes.Count; i++)
        {
            // Si tiene LineRenderer lo borramos
            if (Nodes[i].AssociedLineRenderer != null)
            {
                LineRenderer nodeLr = Nodes[i].AssociedLineRenderer;

                // Si su camino era correcto borramos todo el color de su path
                if (Nodes[i].IsCorrectPath)
                {
                    for (int j = 0; j < nodeLr.positionCount; j++)
                    {
                        Vector3 cellPos = nodeLr.GetPosition(j);
                        GameObject cellGO = GetCellGOByPosition(cellPos);

                        if (cellGO.transform.GetChild(1).gameObject.activeSelf)
                        {
                            cellGO.transform.GetChild(1).gameObject.SetActive(false);
                        }

                    }

                }

                Nodes[i].AssociedLineRenderer = null;
                Destroy(nodeLr.gameObject);
            }

        }

        TouchDetector.Instance.ClearLineRenderers();
        yield return null;

        List<GameObject> lrsGO = new List<GameObject>();

        // Creamos el camino "correcto" de cada nodo (su SolutionPath)
        for (int i = 0; i < Nodes.Count; i += 2)
        {
            Node actualNode = Nodes[i];
            LineRenderer lr = Instantiate(_prefabLineRenderer, Vector3.zero, Quaternion.identity, _fatherBoard).GetComponent<LineRenderer>();
            actualNode.AssociedLineRenderer = lr;
            TouchDetector.Instance.AddLineRendererToList(lr);

            lr.colorGradient = CreateSimpleColorGradient(actualNode.NodeColor);

            List<Vector3> lrPositions = new List<Vector3>();

            for (int j = 0; j < actualNode.SolutionPath.Count; j++)
            {
                Vector3 cellPos = new Vector3(actualNode.SolutionPath[j].Position.x, actualNode.SolutionPath[j].Position.y, 0f);
                lrPositions.Add(cellPos);
            }

            lr.positionCount = lrPositions.Count;
            lr.SetPositions(lrPositions.ToArray());
        }

        // Volvemos a mostrar todas las celdas
        yield return BoardGenerator.Instance.ShowLoadingColor(false);
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

    private void OnReset()
    {
        ClearNodes();
        _boardReady = false;
    }
}
