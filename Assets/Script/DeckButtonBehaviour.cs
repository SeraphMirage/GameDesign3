using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class DeckButtonBehaviour : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Deck")]
    public string deckName;
    public string sceneName;

    [Header("Display")]
    public TMP_Text statusText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (statusText == null)
            return;

        DeckStatusManager manager = DeckStatusManager.Instance;

        bool working = true;
        bool monsterHere = false;

        if (manager != null)
        {
            working = manager.IsDeckWorking(deckName);
            monsterHere = manager.IsMonsterOnDeck(deckName);
        }

        string condition = working
            ? "<color=green>WORKING</color>"
            : "<color=red>NEEDS REPAIR</color>";

        string monsterWarning = monsterHere
            ? "\n<color=red>WARNING: MONSTER DETECTED</color>"
            : "\n<color=green>NO MONSTER DETECTED</color>";

        statusText.richText = true;
        statusText.color = Color.white;

        statusText.text =
            deckName +
            "\n" +
            condition +
            monsterWarning;

        statusText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }

    public void EnterDeck()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("No scene assigned to " + deckName);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning(
                sceneName + " is not included in the Build Profile!"
            );

            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}