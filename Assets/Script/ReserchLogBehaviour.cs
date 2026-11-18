using TMPro;
using UnityEngine;
using System.Collections;

public class ReserchLogBehaviour : MonoBehaviour
{
    [Header("Research Logs")]
    public TMP_Text logText;

    [TextArea(5, 15)]
    public string[] researchLogs;

    [Header("Player Dialogue")]
    public TMP_Text playerDialogueText;
    public GameObject dialogueBackground;

    [TextArea(2, 5)]
    public string finalLogDialogue =
        "I told them to put that thing back. " +
        "But no, what do I know? " +
        "I'm just the one keeping this damn ship afloat.";

    public float dialogueDuration = 5f;

    private int currentLog = 0;
    private bool hasPlayedDialogue = false;

    private void Start()
    {
        // Hide the dialogue when the game starts
        if (playerDialogueText != null)
        {
            playerDialogueText.text = "";
        }

        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(false);
        }

        ShowLog();
    }

    public void NextLog()
    {
        if (researchLogs == null ||
            researchLogs.Length == 0)
            return;

        if (currentLog < researchLogs.Length - 1)
        {
            currentLog++;
            ShowLog();

            // Trigger dialogue on the final entry
            if (currentLog == researchLogs.Length - 1 &&
                !hasPlayedDialogue)
            {
                hasPlayedDialogue = true;
                StartCoroutine(ShowPlayerDialogue());
            }
        }
    }

    public void PreviousLog()
    {
        if (currentLog > 0)
        {
            currentLog--;
            ShowLog();
        }
    }

    private void ShowLog()
    {
        if (logText == null ||
            researchLogs == null ||
            researchLogs.Length == 0)
            return;

        logText.text = researchLogs[currentLog];
    }

    private IEnumerator ShowPlayerDialogue()
    {
        if (playerDialogueText == null)
            yield break;

        // Show the dialogue background
        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(true);
        }

        playerDialogueText.text = finalLogDialogue;

        yield return new WaitForSecondsRealtime(
            dialogueDuration
        );

        playerDialogueText.text = "";

        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(false);
        }
    }
}
