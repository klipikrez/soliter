using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;



public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Canvas canvas;
    public RectTransform canvasRect;
    public Sprite[] symbols;
    public Sprite[] imageBg;
    public Color[] SymbolColors;
    public Colimn[] columns;
    public float spacing = 50;
    CardDummy[] playingCards = new CardDummy[104];
    [SerializeField]
    public List<int> playingCardsIndex = new List<int>();
    public List<Card> moveCards = new List<Card>();
    [SerializeField]
    public Stack<CardDummy> nonRandomCardsToDeal = new Stack<CardDummy>() { };
    public GameObject moveObj;
    public static GameManager instance;
    public Stack<Move> moveStack = new Stack<Move>();
    public RectTransform SpawnCardsHere;
    public PlayerInput input;
    InputAction touch;
    public TMP_Text[] timer;
    public TMP_Text[] DecksLeft;
    public List<Transform> DeckCardGraphics = new List<Transform>();
    Vector2 mousePos = Vector2.zero;
    public GameObject[] WinActiate;
    public TMP_Text[] timeUi;
    public float posmultiply = 100;
    public bool debug = false;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        instance = this;
        touch = input.actions["Point"];
        canvasRect = canvasRect.GetComponent<RectTransform>();

    }

    private void OnEnable()
    {
        touch.performed += Touch;
    }
    void Touch(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i].ID = i;
        }
        GenerateRandomBoard();
    }

    public void Restart()
    {
        if (moveCards.Count != 0) return;
        Timer = 0;
        foreach (GameObject obj in WinActiate)
        {
            obj.SetActive(false);
        }
        foreach (Colimn col in columns)
        {
            col.RemoveCards(0);
        }
        GenerateRandomBoard();
        ResetDeckGraphics();
    }
    private float Timer;
    public void Update()
    {
        if (!WinActiate[0].activeSelf)
            Timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(Timer / 60F);
        int seconds = Mathf.FloorToInt(Timer % 60F);

        timer[0].text = minutes.ToString("00") + ":" + seconds.ToString("00");
        timer[1].text = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (moveCards.Count > 0)
        {
            int i = 0;
            foreach (Card card in moveCards)
            {
                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)canvas.transform,
                    mousePos,
                    canvas.worldCamera,
                    out localPosition
                );

                card.SetPosition(new Vector2(0, -GameManager.instance.spacing * i++) + localPosition);
                card.SetSpeed((moveCards.Count - i) / 2f + 1);
            }
        }
    }

    public void StopDragging(bool returnToHome = true)
    {
        foreach (Card c in moveCards)
        {
            c.background.raycastTarget = true;
            c.dragging = false;

        }
        if (returnToHome)
            columns[moveCards[0].column].AddCards(moveCards, true);
        moveCards.Clear();
    }

    public void StartDraging(List<Card> crds)
    {
        moveCards = crds;
        foreach (Card c in GameManager.instance.moveCards)
        {
            c.background.raycastTarget = false;
            c.dragging = true;
        }
    }

    public void GenerateRandomBoard()
    {
        moveStack.Clear();
        nonRandomCardsToDeal.Clear();
        playingCardsIndex.Clear();

        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    playingCards[j + i * 13 + k * 52] = new CardDummy(j + 1, i);
                    playingCardsIndex.Add(j + i * 13 + k * 52);
                }
            }
        }

        int spawnInColumnIndex = 0;
        int count = 0;
        while (count < (!debug ? 54 : 26))
        {
            if (spawnInColumnIndex >= 10) spawnInColumnIndex = 0;


            SpawnRandomCard(spawnInColumnIndex, count > (!debug ? 43 : 15) ? true : false);

            spawnInColumnIndex++;
            count++;

        }


    }

    public List<CardDummy> SpawnRandomCard(int column, bool visible)
    {
        CardDummy info = GetRandomCard();
        if (info == null) return null;
        return SpawnCard(info.number, info.symbol, column, visible);
    }

    public List<CardDummy> SpawnCard(int number, int symbol, int column, bool visible)
    {
        GameObject obj = Instantiate(cardPrefab);
        Card card = obj.GetComponent<Card>();

        card.Inicialize(number, symbol);

        List<CardDummy> completedSequence = columns[column].AddCards(new List<Card>() { card }, visible ? true : false);
        card.ResetSize();
        card.SetPositionImeniate(SpawnCardsHere.transform.position);
        return completedSequence;
    }

    CardDummy GetRandomCard()
    {
        CardDummy card = null;
        if (playingCardsIndex.Count + nonRandomCardsToDeal.Count == 0) return null; //no cards?? error
        if (nonRandomCardsToDeal.Count == 0)
        {
            int index = !debug ? UnityEngine.Random.Range(0, playingCardsIndex.Count) : playingCardsIndex.Count - 1;

            card = playingCards[playingCardsIndex[index]];
            playingCardsIndex.RemoveAt(index);
            return card;
        }
        card = nonRandomCardsToDeal.Pop();
        return card;
    }

    public void UndoMove()
    {
        if (moveCards.Count != 0) return;
        if (moveStack.Count == 0) return;
        Move move = moveStack.Pop();

        move.Undo(this);
    }

    public void DealCards(int number)
    {
        if (moveCards.Count != 0) return;
        if (playingCardsIndex.Count + nonRandomCardsToDeal.Count == 0) return;

        Dictionary<int, List<CardDummy>> completedSequences = new Dictionary<int, List<CardDummy>>();
        int spawnInColumnIndex = 0;
        int count = 0;

        while (count < number)
        {
            if (spawnInColumnIndex >= 10) spawnInColumnIndex = 0;

            List<CardDummy> completedSequence = SpawnRandomCard(spawnInColumnIndex, true);

            if (completedSequence != null)
            {
                completedSequences.Add(spawnInColumnIndex, completedSequence);
            }

            count++;
            spawnInColumnIndex++;

        }
        moveStack.Push(new DealMove(completedSequences));

        UpdateDeckGraphics();
    }

    public void CheckWin()
    {

        if (playingCardsIndex.Count + nonRandomCardsToDeal.Count == 0)
        {
            int countCards = 0;
            foreach (Colimn c in columns)
            {
                countCards += c.cards.Count;
            }
            if (countCards == 0) Win();
        }
    }

    public void UpdateDeckGraphics()
    {
        UpdateDeckGraphic(0);
        UpdateDeckGraphic(1);
    }

    public void UpdateDeckGraphic(int index)
    {
        if (debug) return;
        int num = (playingCardsIndex.Count + nonRandomCardsToDeal.Count) / 10;

        DecksLeft[index].text = (num).ToString();
        if (num != 0)
        {
            DecksLeft[index].transform.SetParent(DeckCardGraphics[num - 1 + index * 5], false);
        }
        else
        {
            DecksLeft[index].transform.SetParent(DeckCardGraphics[0 + index * 5].parent, false);
        }



        if (num != 5) DeckCardGraphics[num + index * 5].gameObject.SetActive(false);
        if (num != 0) DeckCardGraphics[num - 1 + index * 5].gameObject.SetActive(true);

        DecksLeft[index].rectTransform.anchoredPosition *= new Vector2(0, 1);

    }

    public void ResetDeckGraphics()
    {
        ResetDeckGraphic(0);
        ResetDeckGraphic(1);
    }

    public void ResetDeckGraphic(int index)
    {
        DecksLeft[index].text = (5).ToString();

        DecksLeft[index].transform.SetParent(DeckCardGraphics[4 + index * 5], false);

        foreach (Transform graphic in DeckCardGraphics)
        {
            graphic.gameObject.SetActive(true);
        }

        DecksLeft[index].rectTransform.anchoredPosition *= new Vector2(0, 1);

    }


    RectTransform columnRect = null;
    public void CheckClosestToPlace()
    {
        if (moveCards == null || moveCards.Count == 0 || moveCards[0] == null) return;
        if (columnRect == null) { columnRect = columns[0].GetComponent<RectTransform>(); }
        float maxPlaceDistance = columnRect.sizeDelta.x * 0.5f;

        Colimn closestColumn = null;
        float cloasestDistance = float.MaxValue;

        foreach (Colimn col in columns)
        {
            if (col == columns[moveCards.Last().column]) { /*Debug.Log(col.ID + " skip root column");*/ continue; }
            Vector3 v3 = mousePos;
            v3.z = 5.0f;
            v3 = Camera.main.ScreenToWorldPoint(v3);
            v3.z = 0f;

            Vector3 vec1 = new Vector3(v3.x, v3.y / 2, 0);  //mouse position vector
            Vector3 vec2 = Vector3.one;                     //column top card position vector
            if (col.cards.Count == 0)//if no cards, take root position
            {
                vec2 = new Vector3(col.top.position.x, col.top.position.y / 2, 0);
            }
            else
            {
                vec2 = new Vector3(col.cards.Last().transform.position.x, col.cards.Last().transform.position.y / 2, col.cards.Last().transform.position.z);

            }
            float dist = Vector3.Distance(vec1, vec2);

            if (dist < maxPlaceDistance && dist < cloasestDistance)
            {
                cloasestDistance = dist;
                closestColumn = col;
            }
        }
        if (closestColumn == null || closestColumn == columns[moveCards.Last().column]) { /*Debug.Log("NoColumn");*/ StopDragging(); return; }

        if (closestColumn.cards.Count != 0 && closestColumn.cards.Last().number != moveCards[0].number + 1) { /*Debug.Log("notSame");*/ StopDragging(); return; }

        bool moveReveal = false;
        if (columns[moveCards.Last().column].cards.Count > 0)
            moveReveal = columns[moveCards.Last().column].cards.Last().SetVisible(true);

        int homeColNumber = moveCards[0].column;
        int index = moveCards[0].indexInColumn;
        int to = closestColumn.cards.Count;


        bool clearReveal = false;
        List<CardDummy> completedSequence = closestColumn.AddCards(moveCards, true, ref clearReveal, false);
        moveStack.Push(new CardMove(homeColNumber, closestColumn.ID, to, moveReveal, completedSequence, clearReveal));
        StopDragging(false);
        //Debug.Log("Good");
    }

    public void OptimalPlace()
    {
        if (moveCards == null || moveCards.Count == 0 || moveCards[0] == null) return;
        if (columnRect == null) { columnRect = columns[0].GetComponent<RectTransform>(); }

        Colimn bestColumn = null;
        float columnScore = float.MinValue;
        foreach (Colimn col in columns)
        {
            if (col == columns[moveCards.Last().column]) { /*Debug.Log(col.ID + " skip root column");*/ continue; }
            if (col.cards.Count != 0 && col.cards.Last().number != moveCards[0].number + 1) { /*Debug.Log(col.ID + " Column not empty and Card can be placed here");*/ continue; }
            if (moveCards.Last().number == 13)
            {
                if (col.cards.Count == 0) { columnScore = 0; bestColumn = col; /*Debug.Log(col.ID + " King and empty");*/ break; }
                else { /*Debug.Log(col.ID + " King and not empty");*/ continue; }
            }
            if (col.cards.Count == 0) if (columnScore < 0) { columnScore = 0; bestColumn = col; /*Debug.Log(col.ID + " ZERO and free");*/ continue; } else continue;
            float localScore = 100;

            localScore += math.abs(moveCards.Last().column - col.ID) * 5;

            int i = col.cards.Count - 2;
            int number = col.cards.Last().number;
            while (i >= 0 && (col.cards[i].number - 1) == number)
            {
                localScore += 100;
                number++;
                i--;
            }

            //Debug.Log(col.ID + " Score " + localScore);
            if (localScore > columnScore)
            {
                bestColumn = col;
                columnScore = localScore;
            }

        }
        if (bestColumn == null) { /*Debug.Log("Now not able to place");*/ StopDragging(); return; }
        //Debug.Log(bestColumn.ID + " FINAL SCORE " + columnScore);
        bool revealed = false;
        if (columns[moveCards.Last().column].cards.Count > 0)
            revealed = columns[moveCards.Last().column].cards.Last().SetVisible(true);

        int homeColNumber = moveCards[0].column;
        int to = bestColumn.cards.Count;
        int j = 0;
        foreach (Card card in moveCards)
        {
            card.SetSpeed((moveCards.Count - j++) / 10f + 1f);
        }

        bool clearReveal = false;
        List<CardDummy> completedSequence = bestColumn.AddCards(moveCards, true, ref clearReveal, true);
        moveStack.Push(new CardMove(homeColNumber, bestColumn.ID, to, revealed, completedSequence, clearReveal));

        StopDragging(false);
    }

    public void Win()
    {
        foreach (GameObject obj in WinActiate)
        {
            obj.SetActive(true);
        }
        int minutes = Mathf.FloorToInt(Timer / 60F);
        int seconds = Mathf.FloorToInt(Timer % 60F);

        timeUi[0].text = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (PlayerPrefs.HasKey("HighScore"))
        {
            if (PlayerPrefs.GetFloat("HighScore") > Timer)
            {
                if (!Application.isEditor)
                    PlayerPrefs.SetFloat("HighScore", Timer);
            }
        }
        else
        {
            if (!Application.isEditor)
                PlayerPrefs.SetFloat("HighScore", Timer);
        }

        if (!Application.isEditor)
        {
            minutes = Mathf.FloorToInt(PlayerPrefs.GetFloat("HighScore") / 60F);
            seconds = Mathf.FloorToInt(PlayerPrefs.GetFloat("HighScore") % 60F);
        }
        timeUi[1].text = minutes.ToString("00") + ":" + seconds.ToString("00");

    }
}
