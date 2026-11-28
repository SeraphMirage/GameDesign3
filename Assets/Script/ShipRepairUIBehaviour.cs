
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShipRepairUIBehaviour : MonoBehaviour
{
    [Header("Ship Status Display")]
    public TMP_Text deckStatusText;

    [Header("Deck Buttons")]
    public Button topDeckButton;
    public Button bottomDeckButton;

    [Header("Top Deck Repairs")]
    public string[] topDeckRepairIds;
    public string[] topDeckRepairNames;

    [Header("Bottom Deck Repairs")]
    public string[] bottomDeckRepairIds;
    public string[] bottomDeckRepairNames;

    private string hoveredDeck = "";

    // ------------------------------------
    // START
    // ------------------------------------

    void Start()
    {
        SetupHoverButtons();
        UpdateShipStatus();
    }

    // ------------------------------------
    // UPDATE
    // ------------------------------------

    void Update()
    {
        UpdateShipStatus();
    }

    // ------------------------------------
    // SETUP BUTTON HOVER
    // ------------------------------------

    void SetupHoverButtons()
    {
        SetupButton(topDeckButton, "Top Deck");
        SetupButton(bottomDeckButton, "Bottom Deck");
    }

    void SetupButton(Button button, string deck)
    {
        if (button == null)
            return;

        ShipRepairHoverTarget hover =
            button.GetComponent<ShipRepairHoverTarget>();

        if (hover == null)
        {
            hover = button.gameObject
                .AddComponent<ShipRepairHoverTarget>();
        }

        hover.Setup(this, deck);
    }

    // ------------------------------------
    // MOUSE ENTER / EXIT
    // ------------------------------------

    public void SetHoveredDeck(string deck)
    {
        hoveredDeck = deck;
        UpdateShipStatus();
    }

    public void ClearHoveredDeck(string deck)
    {
        if (hoveredDeck == deck)
        {
            hoveredDeck = "";
            UpdateShipStatus();
        }
    }

    // ------------------------------------
    // MAIN STATUS DISPLAY
    // ------------------------------------

    public void UpdateShipStatus()
    {
        if (deckStatusText == null)
            return;

        if (hoveredDeck == "Top Deck")
        {
            ShowDeckDetails(
                "TOP DECK",
                "Top Deck",
                topDeckRepairIds,
                topDeckRepairNames
            );
        }
        else if (hoveredDeck == "Bottom Deck")
        {
            ShowDeckDetails(
                "BOTTOM DECK",
                "Bottom Deck",
                bottomDeckRepairIds,
                bottomDeckRepairNames
            );
        }
        else
        {
            ShowOverview();
        }
    }

    // ------------------------------------
    // DECK REPAIR DETAILS
    // ------------------------------------

    void ShowDeckDetails(
        string title,
        string deckName,
        string[] repairIds,
        string[] repairNames)
    {
        int brokenCount =
            ShipRepairState.CountBroken(repairIds);

        string result =
            "<b>" + title + "</b>\n\n";

        if (brokenCount == 0)
        {
            result +=
                "<color=green>ALL SYSTEMS " +
                "OPERATIONAL</color>\n";
        }
        else
        {
            result +=
                "<color=red>WARNING: " +
                brokenCount +
                " SYSTEMS BROKEN</color>\n\n";

            for (int i = 0;
                 i < repairIds.Length;
                 i++)
            {
                string id = repairIds[i];

                if (string.IsNullOrEmpty(id))
                    continue;

                if (!ShipRepairState.IsFixed(id))
                {
                    string repairName =
                        GetRepairName(
                            id,
                            repairNames,
                            i
                        );

                    result +=
                        "<color=yellow>• </color>" +
                        repairName + "\n";
                }
            }
        }

        result += "\n" +
            GetMonsterStatus(deckName);

        deckStatusText.text = result;
    }

    // ------------------------------------
    // REPAIR NAMES
    // ------------------------------------

    string GetRepairName(
        string repairId,
        string[] names,
        int index)
    {
        if (names != null &&
            index < names.Length &&
            !string.IsNullOrEmpty(names[index]))
        {
            return names[index];
        }

        // If no display name is assigned,
        // show a readable version of the ID.
        return repairId.Replace("_", " ");
    }

    // ------------------------------------
    // DEFAULT OVERVIEW
    // ------------------------------------

    void ShowOverview()
    {
        int topBroken =
            ShipRepairState.CountBroken(
                topDeckRepairIds
            );

        int bottomBroken =
            ShipRepairState.CountBroken(
                bottomDeckRepairIds
            );

        string result =
            "<b>SHIP SYSTEM STATUS</b>\n\n" +
            "<b>TOP DECK</b>\n" +
            GetShortStatus(topBroken) +
            "\n\n" +
            "<b>BOTTOM DECK</b>\n" +
            GetShortStatus(bottomBroken) +
            "\n\n" +
            "<color=yellow>Hover over a deck " +
            "to view repairs.</color>";

        deckStatusText.text = result;
    }

    // ------------------------------------
    // SHORT STATUS
    // ------------------------------------

    string GetShortStatus(int broken)
    {
        if (broken == 0)
        {
            return
                "<color=green>WORKING</color>";
        }

        return
            "<color=red>" +
            broken +
            " SYSTEMS BROKEN</color>";
    }

    // ------------------------------------
    // MONSTER WARNING
    // ------------------------------------

    string GetMonsterStatus(string deckName)
    {
        if (DeckStatusManager.Instance == null)
        {
            return
                "<color=yellow>MONSTER LOCATION " +
                "UNKNOWN</color>";
        }

        if (DeckStatusManager.Instance
            .IsMonsterOnDeck(deckName))
        {
            return
                "<color=red>WARNING: " +
                "MONSTER DETECTED</color>";
        }

        return
            "<color=green>NO MONSTER " +
            "DETECTED</color>";
    }
}

// ------------------------------------
// BUTTON HOVER DETECTION
// ------------------------------------

public class ShipRepairHoverTarget :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private ShipRepairUIBehaviour repairUI;
    private string deckName;

    public void Setup(
        ShipRepairUIBehaviour ui,
        string deck)
    {
        repairUI = ui;
        deckName = deck;
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (repairUI != null)
        {
            repairUI.SetHoveredDeck(deckName);
        }
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (repairUI != null)
        {
            repairUI.ClearHoveredDeck(deckName);
        }
    }
}
