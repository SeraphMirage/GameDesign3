
using UnityEngine;
using System.Collections.Generic;

public class DeckStatusManager : MonoBehaviour
{
    public static DeckStatusManager Instance;

    [Header("Monster Tracking")]
    public string monsterDeck = "Top Deck";
    public float deckChangeInterval = 25f;

    [Header("Deck Change Status")]
    public bool deckChangePending = false;

    private float timer;

    private Dictionary<string, bool> deckStatus =
        new Dictionary<string, bool>();

    private readonly string[] availableDecks =
    {
        "Top Deck",
        "Bottom Deck"
    };

    // Remember previous spawn locations.
    private Dictionary<string, int> lastSpawnIndices =
        new Dictionary<string, int>();

    // Track monster instances in loaded scenes.
    private Dictionary<string, MonsterBehaviour>
        loadedMonsters =
        new Dictionary<string, MonsterBehaviour>();

    // ------------------------------------
    // AWAKE
    // ------------------------------------

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

    // ------------------------------------
    // START
    // ------------------------------------

    void Start()
    {
        Debug.Log(
            "Monster currently on: " + monsterDeck
        );
    }

    // ------------------------------------
    // UPDATE
    // ------------------------------------

    void Update()
    {
        // Don't restart the countdown while
        // a deck change is already pending.
        if (deckChangePending)
        {
            // If the monster's deck scene is
            // not loaded, switch offscreen.
            if (!HasLoadedMonsterOnCurrentDeck())
            {
                CompleteDeckChange();
            }

            return;
        }

        timer += Time.deltaTime;

        if (timer >= deckChangeInterval)
        {
            RequestDeckChange();
        }
    }

    // ------------------------------------
    // MONSTER REGISTRATION
    // ------------------------------------

    public void RegisterMonster(
        string deckName,
        MonsterBehaviour monster)
    {
        if (monster == null)
            return;

        loadedMonsters[deckName] = monster;
    }

    public void UnregisterMonster(
        string deckName,
        MonsterBehaviour monster)
    {
        if (loadedMonsters.TryGetValue(
            deckName,
            out MonsterBehaviour registered))
        {
            if (registered == monster)
            {
                loadedMonsters.Remove(deckName);
            }
        }
    }

    bool HasLoadedMonsterOnCurrentDeck()
    {
        if (loadedMonsters.TryGetValue(
            monsterDeck,
            out MonsterBehaviour monster))
        {
            return monster != null &&
                   monster.isActiveAndEnabled;
        }

        return false;
    }

    // ------------------------------------
    // REQUEST DECK CHANGE
    // ------------------------------------

    public void RequestDeckChange()
    {
        if (deckChangePending)
            return;

        deckChangePending = true;

        Debug.Log(
            "Deck change pending. " +
            "Monster will finish its activity."
        );

        // If the monster is on an unloaded
        // deck, there's no visible patrol
        // or chase to interrupt.
        if (!HasLoadedMonsterOnCurrentDeck())
        {
            CompleteDeckChange();
        }
    }

    public bool IsDeckChangePending()
    {
        return deckChangePending;
    }

    // ------------------------------------
    // COMPLETE DECK CHANGE
    // ------------------------------------

    public void CompleteDeckChange()
    {
        if (!deckChangePending)
            return;

        string oldDeck = monsterDeck;

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
        {
            deckChangePending = false;
            timer = 0f;
            return;
        }

        monsterDeck =
            choices[Random.Range(0, choices.Count)];

        deckChangePending = false;
        timer = 0f;

        Debug.Log(
            "Monster finished activity and moved " +
            "from " + oldDeck +
            " to " + monsterDeck
        );
    }

    // Keep compatibility with other scripts.
    public void MoveMonsterToAnotherDeck()
    {
        RequestDeckChange();
    }

    // ------------------------------------
    // MONSTER LOCATION
    // ------------------------------------

    public bool IsMonsterOnDeck(string deckName)
    {
        return monsterDeck == deckName;
    }

    public string GetMonsterDeck()
    {
        return monsterDeck;
    }

    // ------------------------------------
    // RANDOM SPAWN TRACKING
    // ------------------------------------

    public int GetRandomSpawnIndex(
        string deckName,
        int spawnCount)
    {
        if (spawnCount <= 0)
            return -1;

        int lastIndex = -1;

        lastSpawnIndices.TryGetValue(
            deckName,
            out lastIndex
        );

        int randomIndex =
            Random.Range(0, spawnCount);

        // Avoid repeating the last spawn.
        if (spawnCount > 1 &&
            randomIndex == lastIndex)
        {
            randomIndex =
                (randomIndex + Random.Range(
                    1,
                    spawnCount
                )) % spawnCount;
        }

        lastSpawnIndices[deckName] =
            randomIndex;

        return randomIndex;
    }

    // ------------------------------------
    // DECK STATUS
    // ------------------------------------

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
