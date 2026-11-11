
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DeckStatusHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public string deckName = "Top Deck";
    public TMP_Text statusText;

    private bool isHovering = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered: " + deckName);

        isHovering = true;
        ShowStatus();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited: " + deckName);

        isHovering = false;

        if (statusText != null)
            statusText.enabled = false;
    }

    void Update()
    {
        if (isHovering)
            ShowStatus();
    }

    void ShowStatus()
    {
        if (statusText == null)
        {
            Debug.LogWarning(
                "Status Text is NOT assigned on " + gameObject.name);
            return;
        }

        if (DeckStatusManager.Instance == null)
        {
            Debug.LogWarning("DeckStatusManager is missing!");
            return;
        }

        bool working =
            DeckStatusManager.Instance.IsDeckWorking(deckName);

        bool monster =
            DeckStatusManager.Instance.IsMonsterOnDeck(deckName);

        string workingMessage = working
            ? "<color=green>WORKING</color>"
            : "<color=red>NOT WORKING</color>";

        string monsterMessage = monster
            ? "<color=red>WARNING: MONSTER DETECTED</color>"
            : "<color=green>NO MONSTER DETECTED</color>";

        statusText.text = deckName + "\n" +
                          workingMessage + "\n" +
                          monsterMessage;

        statusText.enabled = true;
    }
}
