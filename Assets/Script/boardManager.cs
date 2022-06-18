using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class boardManager : MonoBehaviour
{
    public static boardManager Instance { set; get; }

    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePrefab;
    public GameObject blackPrefab;

    public GameObject highlights;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;

    private Vector3 boardOff = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOff = new Vector3(0.5f, 0.125f, 0.5f);

    public bool gameIsOver;
    public float overTime;

    public bool isPlayerWhite;
    private bool isWhiteTurn;
    private bool hasKilled;

    private Piece selected;
    private List<Piece> forcedPieces;

    private Vector2 mousePos;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private Client client;

    void Start()
    {
        Instance = this;
        client = FindObjectOfType<Client>();

        foreach (Transform transform in highlights.transform)
        {
            transform.position = Vector3.down * 50;
        }

        if (client)
        {
            isPlayerWhite = client.isHost;
            Alert(client.players[0].name + " VS " + client.players[1].name);
        }
        else
        {
            Alert("White player's turn");
        }
        rotateCam(isPlayerWhite);
        isWhiteTurn = true;
        forcedPieces = new List<Piece>();
        genBoard();
    }

    private void Update()
    {
        if (gameIsOver)
        {
            gameOver(overTime);
        }

        foreach (Transform transform in highlights.transform) //spin highlight
        {
            transform.Rotate(Vector3.up * 90 * Time.deltaTime);
        }

        updateAlert();
        updateMousePos();

        if ((isPlayerWhite) ? isWhiteTurn : !isWhiteTurn)
        {
            int x = (int)mousePos.x;
            int y = (int)mousePos.y;

            if (selected != null)
                animPiece(selected);

            if (Input.GetMouseButtonDown(0)) //click LMB
                selectPiece(x, y);

            if (Input.GetMouseButtonUp(0))
                tryMove((int)startDrag.x, (int)startDrag.y, x, y);
        }
    }

    private void rotateCam(bool isPlayerWhite)
    {
        Camera cam = Camera.main;
        if (!isPlayerWhite)
        {
            cam.transform.localEulerAngles = new Vector3(90, 0, 180);
        }
    }

    void gameOver(float sinceOver)
    {
        if (Time.time - sinceOver > 3.0f)
        {
            Server server = FindObjectOfType<Server>();
            Client client = FindObjectOfType<Client>();

            if (server)
                Destroy(server.gameObject);

            if (client)
                Destroy(client.gameObject);

            SceneManager.LoadScene("Menu");
        }
    }

    private void updateMousePos()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mousePos.x = (int)(hit.point.x - boardOff.x);
            mousePos.y = (int)(hit.point.z - boardOff.z); // use z since board on ground
        }
        else
        {
            mousePos.x = -1;
            mousePos.y = -1;
        }
    }

    void animPiece(Piece piece)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            piece.transform.position = hit.point + Vector3.up;
        }
    }

    void selectPiece(int x, int y)
    {

        if (isOutOfBounds(x, y))
            return;

        Piece piece = pieces[x, y];
        if (piece != null && piece.isWhite == isPlayerWhite)
        {
            if (forcedPieces.Count == 0)
            {
                selected = piece;
                startDrag = mousePos;
            }
            else
            {
                if (forcedPieces.Find(fp => fp == piece) == null)
                    return;

                selected = piece;
                startDrag = mousePos;

            }
        }
    }

    bool isOutOfBounds(int i, int j)
    {
        return (i < 0 || i >= 8 || j < 0 || j >= 8);
    }

    public void tryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = checkPossibleMove();

        //redefine it for the other player on multiplayer
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selected = pieces[x1, y1];

        if (isOutOfBounds(x2, y2))
        {
            if (selected != null) //cancel the move and reset the selected piece pos
                movePiece(selected, x1, y1);

            startDrag = Vector2.zero;
            selected = null;
            highlight();
        }

        if (selected != null)
        {
            if (endDrag == startDrag) // cancel if not moved
            {
                movePiece(selected, x1, y1);
                startDrag = Vector2.zero;
                selected = null;
                highlight();
            }

            if (selected.validMove(pieces, x1, y1, x2, y2)) // is move valid
            {
                if (Mathf.Abs(x2 - x1) == 2) // check if its a killing move
                {
                    Piece piece = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (piece != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        DestroyImmediate(piece.gameObject);
                        hasKilled = true;
                    }
                }

                if (forcedPieces.Count != 0 && !hasKilled)
                {
                    movePiece(selected, x1, y1);
                    startDrag = Vector2.zero;
                    selected = null;
                    highlight();
                }

                //move the piece
                pieces[x2, y2] = selected;
                pieces[x1, y1] = null;
                movePiece(selected, x2, y2);

                endTurn();
            }
            else // if move invalid reset piece pos
            {
                movePiece(selected, x1, y1);
                startDrag = Vector2.zero;
                selected = null;
                highlight();
            }
        }
    }

    void endTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        if (selected != null)
        {
            if (selected.isWhite && !selected.isKing && y == 7)
            {
                selected.isKing = true;
                selected.transform.Rotate(Vector3.right * 180);
            }
            else if (!selected.isWhite && !selected.isKing && y == 0)
            {
                selected.isKing = true;
                selected.transform.Rotate(Vector3.right * 180);
            }
        }

        if (client)
        {
            string message = "Cmove|";
            message += startDrag.x.ToString() + '|';
            message += startDrag.y.ToString() + '|';
            message += endDrag.x.ToString() + '|';
            message += endDrag.y.ToString();

            client.Send(message);
        }

        selected = null;
        startDrag = Vector2.zero;

        if (checkPossibleMove(selected, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        hasKilled = false;
        checkWin();

        if (!gameIsOver)
        {
            if (!client)
            {
                isPlayerWhite = !isPlayerWhite;
                if (isPlayerWhite)
                    Alert("White player's turn");
                else
                    Alert("Black player's turn");
            }
            else
            {
                if (isWhiteTurn)
                    Alert(client.players[0].name + "'s turn");
                else
                    Alert(client.players[1].name + "'s turn");
            }
        }

        checkPossibleMove();
    }

    void checkWin()
    {
        var pieces = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }

        if (!hasWhite)
            win(false);
        if (!hasBlack)
            win(true);
    }

    void win(bool whiteWin)
    {
        overTime = Time.time;

        if (whiteWin)
            Alert("White Team Win");
        else
        {
            Alert("Black Team Win");
        }

        gameIsOver = true;
    }

    List<Piece> checkPossibleMove(Piece piece, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x, y].isCanKill(pieces, x, y))
            forcedPieces.Add(pieces[x, y]);

        highlight();
        return forcedPieces;
    }

    private List<Piece> checkPossibleMove()
    {
        forcedPieces = new List<Piece>();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                    if (pieces[i, j].isCanKill(pieces, i, j))
                        forcedPieces.Add(pieces[i, j]);

        highlight();
        return forcedPieces;
    }

    private void genBoard()
    {
        //Gen White
        for (int y = 0; y < 3; y++) //y coordinate 3 rows
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2) //x coordinate skips 1 
            {
                if (oddRow)
                    genPiece(x, y);
                else
                    genPiece(x + 1, y);
            }
        }

        //Gen Black
        for (int y = 7; y > 4; y--) //y coordinate 3 rows
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2) //x coordinate skips 1 
            {
                if (oddRow)
                    genPiece(x, y);
                else
                    genPiece(x + 1, y);
            }
        }
    }

    private void genPiece(int x, int y)
    {
        GameObject obj;

        if (y > 3)
        {
            obj = Instantiate(blackPrefab) as GameObject;
        }
        else
        {
            obj = Instantiate(whitePrefab) as GameObject;
        }
        obj.transform.SetParent(transform);
        Piece piece = obj.GetComponent<Piece>();
        pieces[x, y] = piece;
        movePiece(piece, x, y);
    }

    private void movePiece(Piece piece, int x, int y)
    {
        piece.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOff + pieceOff;
    }

    void highlight()
    {
        foreach (Transform transform in highlights.transform)
        {
            transform.position = Vector3.down * 50;
        }

        if (forcedPieces.Count > 0)
            highlights.transform.GetChild(0).transform.position = forcedPieces[0].transform.position + Vector3.down * 0.1f;

        if (forcedPieces.Count > 1)
            highlights.transform.GetChild(1).transform.position = forcedPieces[1].transform.position + Vector3.down * 0.1f;
    }

    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<TextMeshProUGUI>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;
    }
    public void updateAlert()
    {
        if (alertActive)
        {
            if (Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if (Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
    }
}
