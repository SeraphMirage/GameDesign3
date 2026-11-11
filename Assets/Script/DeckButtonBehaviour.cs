using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class DeckButtonBehaviour :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Deck")]
    public string deckName;

    [Header("Scene")]
    public string sceneName;

    [Header("Status Text")]
    public TMP_Text statusText;

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (statusText == null)
        {
            Debug.LogWarning(
                "No Status Text assigned to " + deckName
            );

            return;
        }

        bool working = true;
        bool monsterHere = false;

        if (DeckStatusManager.Instance != null)
        {
            working =
                DeckStatusManager.Instance
                    .IsDeckWorking(deckName);

            monsterHere =
                DeckStatusManager.Instance
                    .IsMonsterOnDeck(deckName);
        }

        string repairStatus;

        if (working)
        {
            repairStatus =
                "<color=green>WORKING</color>";
        }
        else
        {
            repairStatus =
                "<color=red>NEEDS REPAIR</color>";
        }

        string monsterStatus;

        if (monsterHere)
        {
            monsterStatus =
                "<color=red>MONSTER DETECTED</color>";
        }
        else
        {
            monsterStatus =
                "<color=green>NO MONSTER DETECTED</color>";
        }

        statusText.richText = true;

        statusText.text =
            deckName +
            "\n" +
            repairStatus +
            "\n" +
            monsterStatus;

        statusText.gameObject.SetActive(true);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }

    public void EnterDeck()
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        SceneManager.LoadScene(sceneName);
    }
}