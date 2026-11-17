using TMPro;
using UnityEngine;

public class ReserchLogBehaviour : MonoBehaviour
{
    public TMP_Text logText;

    [TextArea(5, 15)]
    public string[] researchLogs;

    private int currentLog = 0;

    private void Start()
    {
        ShowLog();
    }

    public void NextLog()
    {
        if (currentLog < researchLogs.Length - 1)
        {
            currentLog++;
            ShowLog();
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
        if (logText == null || researchLogs == null ||
            researchLogs.Length == 0)
            return;

        logText.text = researchLogs[currentLog];
    }
}
