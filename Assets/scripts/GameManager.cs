using System;
using System.Collections;
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
    public GameObject outOfCardsGraphic;
    public Coroutine flashCardCoroutine;
    public Coroutine flashDealCoroutine;
    public bool debug = false;
    List<Card> flashFrom = new List<Card>();
    List<Card> flashTo = new List<Card>();

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
        AudioManager.Play("deal", 0.25f);
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


    public void FlashDeckGraphic()
    {
        if (debug) return;

        AudioManager.Play("noMove", 0.25f);

        int numberOfActiveCards = 0;
        foreach (RectTransform card in DeckCardGraphics)
        {
            if (card.gameObject.activeSelf) numberOfActiveCards++;
        }

        if (numberOfActiveCards == 0) { outOfCardsGraphic.SetActive(true); return; }
        if (flashDealCoroutine != null)
        {
            StopCoroutine(flashDealCoroutine);
            flashDealCoroutine = null;
        }
        flashDealCoroutine = StartCoroutine(FlashDealCards());
    }

    IEnumerator FlashDealCards()
    {
        float timer = 0;
        while (timer <= 1)
        {
            timer += 0.2f;
            yield return new WaitForSeconds(0.1f);
            foreach (RectTransform card in DeckCardGraphics)
            {
                if (card == null) yield break;
                card.gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 1f, 0.7f);
            }


            yield return new WaitForSeconds(0.1f);
            foreach (RectTransform card in DeckCardGraphics)
            {
                if (card == null) yield break;
                card.gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            }

        }
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
        if (columnRect == null) { columnRect = (RectTransform)columns[0].transform; }
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
                vec2 = new Vector3(col.top.position.x, col.top.position.y / 2, 0);
            else
                vec2 = new Vector3(col.cards.Last().transform.position.x, col.cards.Last().transform.position.y / 2, 0);

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
        AudioManager.Play("put", 0.25f);
        //Debug.Log("Good");
    }

    public void OptimalPlace()
    {
        if (moveCards == null || moveCards.Count == 0 || moveCards[0] == null) return;
        if (columnRect == null) { columnRect = columns[0].GetComponent<RectTransform>(); }

        Colimn bestColumn = null;
        float columnScore = float.MinValue;


        (bestColumn, columnScore) = CalculateBestMove(moveCards[0].column, moveCards[0].number, moveCards.Count);

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

        int i = moveCards.Count;
        foreach (Card card in moveCards)
        {
            card.playSound = true;
            card.movedCards = i;
            i--;
        }

        bool clearReveal = false;
        List<CardDummy> completedSequence = bestColumn.AddCards(moveCards, true, ref clearReveal, true);
        moveStack.Push(new CardMove(homeColNumber, bestColumn.ID, to, revealed, completedSequence, clearReveal));

        StopDragging(false);
    }

    public (Colimn, float) CalculateBestMove(int fromColumn, int numberOnFirstCard, int numberOfCards)
    {

        Colimn bestColumn = null;
        float bestScore = float.MinValue;

        foreach (Colimn column in columns)
        {
            if (column.ID == fromColumn) { /*Debug.Log(col.ID + " skip root column");*/ continue; }
            if (column.cards.Count != 0 && column.cards[column.cards.Count - 1].number != numberOnFirstCard + 1) { /*Debug.Log(col.ID + " Column not empty and Card can't be placed here");*/ continue; }

            if (numberOnFirstCard == 13)
            {
                if (column.cards.Count == 0) { bestScore = 0 + numberOfCards; bestColumn = column; /*Debug.Log(col.ID + " King and empty");*/ break; }
                else { /*Debug.Log(col.ID + " King and not empty");*/ continue; }
            }

            if (column.cards.Count == 0) if (bestScore < 0) { bestScore = 0 + numberOfCards; bestColumn = column; /*Debug.Log(col.ID + " ZERO and free");*/ continue; } else continue;

            float localScore = 100;

            localScore += math.abs(fromColumn - column.ID) * 5; // distance to from column

            int i = column.cards.Count - 1; //number of cards in selectedChecking column
                                            // Debug.Log(column.ID + " Start counting from: " + i);
            int number = numberOnFirstCard + 1; //this is for checking how many cards are in a chain
            float b = localScore;
            while (i >= 0 && column.cards[i].number == number && column.cards[i].IsVisible() && column.cards[i].IsMovable())
            {
                //Debug.Log(i);
                localScore += 100;
                number++;
                i--;
            }
            Debug.Log(localScore - b + " -- " + numberOfCards * 100);
            localScore += numberOfCards * 100;


            //Debug.Log(col.ID + " Score " + localScore);
            if (localScore > bestScore)
            {
                bestColumn = column;
                bestScore = localScore;
            }

        }

        return (bestColumn, bestScore);
    }

    public void GiveHint()
    {
        if (moveCards.Count != 0) return;
        Colimn fromBestColumn = null;
        Colimn toBestColumn = null;
        int formIndex = 52;
        float bestColumnScore = float.MinValue;

        foreach (Colimn column in columns)
        {
            for (int i = 0; i < column.cards.Count; i++)
            {
                if (!column.cards[i].IsVisible() || !column.cards[i].IsMovable()) continue;
                Colimn calculatedColumn = null;
                float calculatedColumnScore = float.MinValue;
                (calculatedColumn, calculatedColumnScore) = CalculateBestMove(column.cards[i].column, column.cards[i].number, column.cards.Count - 1 - column.cards[i].indexInColumn);
                if (calculatedColumn != null)
                    Debug.Log("=============\n" + column.cards[i].number + " : " + calculatedColumnScore + "\nfrom: " + column.cards[i].column + "\nto:" + calculatedColumn.ID);
                else
                    Debug.Log("nem");

                if (calculatedColumnScore == float.MinValue) continue;//if no move can be calculated for this card, then skip it;

                if (calculatedColumn.cards.Count == 0 && i == 0
                || i != 0 && calculatedColumn.cards.Count == 0 && column.cards[i].number + 1 == column.cards[i - 1].number) continue;//if the hint would involve moving all the cards taht were already in seriael to an empty column, just dont do it. this is to stop endless loop of hints moving the cards back and fourth.

                if (calculatedColumn.cards.Count != 0)
                {
                    //subtract if cards were already in series
                    int numberOfCardsInSeiesDifference = 0;
                    int j = i - 1; //number of cards in selectedChecking column
                    int number = -52;
                    if (j >= 0)
                        number = column.cards[j].number; //this is for checking how many cards are in a chain

                    while (j >= 0 && (column.cards[j].number == number) && column.cards[j].IsVisible() && column.cards[j].IsMovable())
                    {
                        numberOfCardsInSeiesDifference--;
                        number++;
                        j--;
                    }

                    j = calculatedColumn.cards.Count - 1;
                    if (j >= 0)
                        number = calculatedColumn.cards[j].number;
                    while (j >= 0 && (calculatedColumn.cards[j].number == number) && calculatedColumn.cards[j].IsVisible() && calculatedColumn.cards[j].IsMovable())
                    {
                        numberOfCardsInSeiesDifference++;
                        number++;
                        j--;
                    }

                    if (numberOfCardsInSeiesDifference <= 0) continue;
                    calculatedColumnScore -= numberOfCardsInSeiesDifference * 100;
                }

                if (calculatedColumnScore > bestColumnScore)
                {
                    fromBestColumn = column;
                    toBestColumn = calculatedColumn;
                    formIndex = i;
                    bestColumnScore = calculatedColumnScore;
                }

            }
        }
        if (fromBestColumn == null)
        {
            FlashDeckGraphic();
            return;
            /*int index = FirstEmptyColumn();

            if (AllCardsMovable())
            {
                FlashDeckGraphic();
                return;
            }


            if (index >= 0)
            {

                toBestColumn = columns[index];
                fromBestColumn = null;

                formIndex = 52;
                bestColumnScore = float.MinValue;
                foreach (Colimn column in columns)
                {
                    int currentScore = 0;
                    for (int i = 0; i < column.cards.Count; i++)
                    {
                        if (!column.cards[i].IsVisible() || !column.cards[i].IsMovable()) continue;
                        if (i == 0 && column.cards[i].IsVisible()) break;
                        currentScore++;

                    }
                    if (bestColumnScore < currentScore)
                    {
                        bestColumnScore = currentScore;
                        formIndex = column.cards.Count - currentScore;
                        fromBestColumn = column;

                    }
                }
            }
            else
            {
                FlashDeckGraphic();
                return;
            }*/

        }

        AudioManager.Play("hint", 0.25f);

        if (flashCardCoroutine != null)
        {
            StopCoroutine(flashCardCoroutine);
            foreach (Card card in flashFrom)
            {
                if (card != null)
                    card.background.color = Color.white;
            }
            foreach (Card card in flashTo)
            {
                if (card != null)
                    card.background.color = Color.white;
            }
            flashFrom.Clear();
            flashTo.Clear();
        }
        for (int i = fromBestColumn.cards.Count - 1; i >= formIndex; i--)
            flashFrom.Add(fromBestColumn.cards[i]);
        if (toBestColumn.cards.Count != 0)
            flashTo.Add(toBestColumn.cards[toBestColumn.cards.Count - 1]);
        flashCardCoroutine = StartCoroutine(FlashCards());
        Debug.Log(fromBestColumn.ID + " " + toBestColumn.ID + " " + formIndex);
    }

    bool AllCardsMovable()
    {
        foreach (Colimn column in columns)
        {

            for (int i = 0; i < column.cards.Count; i++)
            {
                if (!column.cards[i].IsVisible()) continue;
                if (!column.cards[i].IsMovable()) return false;

            }
        }
        return true;
    }

    int FirstEmptyColumn()
    {
        int i = 0;
        foreach (Colimn column in columns)
        {
            if (column.cards.Count == 0) return i;
            i++;
        }
        return -52;
    }

    IEnumerator FlashCards()
    {
        float timer = 0;
        while (timer <= 1)
        {
            timer += 0.2f;
            yield return new WaitForSeconds(0.1f);
            foreach (Card card in flashFrom)
            {
                if (card == null) yield break;
                card.background.color = new Color(0.7f, 1f, 0.7f);
            }
            // if (flashTo.Count != 0)
            foreach (Card card in flashTo)
            {
                card.background.color = new Color(1, 0.7f, 0.7f);
            }

            yield return new WaitForSeconds(0.1f);
            foreach (Card card in flashFrom)
            {
                if (card == null) yield break;
                card.background.color = Color.white;
            }
            foreach (Card card in flashTo)
            {
                card.background.color = Color.white;
            }
        }
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

        AudioManager.Play("winApplause", 0.5f);

    }
}
