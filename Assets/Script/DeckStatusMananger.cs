using UnityEngine;
using System.Collections.Generic;

public class DeckStatusManager : MonoBehaviour
{
    public static DeckStatusManager Instance;

    [Header("Monster Tracking")]
    public string monsterDeck = "Bottom Deck";
    public float deckChangeInterval = 25f;

    private float timer;

    private Dictionary<string, bool> deckStatus =
        new Dictionary<string, bool>();

    private readonly string[] availableDecks =
    {
        "Top Deck",
        "Bottom Deck"
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        deckStatus["Top Deck"] = true;
        deckStatus["Bottom Deck"] = true;
    }

    void Start()
    {
        Debug.Log(
            "Monster currently on: " + monsterDeck
        );
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= deckChangeInterval)
        {
            timer = 0f;
            MoveMonsterToAnotherDeck();
        }
    }

    void MoveMonsterToAnotherDeck()
    {
        List<string> choices =
            new List<string>();

        foreach (string deck in availableDecks)
        {
            if (deck != monsterDeck)
            {
                choices.Add(deck);
            }
        }

        if (choices.Count == 0)
            return;

        string oldDeck = monsterDeck;

        monsterDeck =
            choices[Random.Range(0, choices.Count)];

        Debug.Log(
            "Monster moved from " +
            oldDeck +
            " to " +
            monsterDeck
        );
    }

    public bool IsMonsterOnDeck(string deckName)
    {
        return monsterDeck == deckName;
    }

    public string GetMonsterDeck()
    {
        return monsterDeck;
    }

    public bool IsDeckWorking(string deckName)
    {
        if (deckStatus.ContainsKey(deckName))
            return deckStatus[deckName];

        return true;
    }

    public void SetDeckStatus(
        string deckName,
        bool working)
    {
        deckStatus[deckName] = working;
    }
}