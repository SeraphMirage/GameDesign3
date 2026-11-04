using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ComputerRepairBehaviour : MonoBehaviour
{
    // Screen Text
    public TMP_Text computerScreenText;

    // Command Buttons
    public Button diagnosticsButton;
    public Button restartButton;
    public Button databaseButton;

    // Reboot
    public Slider rebootProgress;
    public float rebootDuration = 5f;

    private int repairStep = 0;
    private bool isRebooting = false;
    private bool isRepaired = false;

    void Start()
    {
        repairStep = 0;

        computerScreenText.text =
            "RESEARCH TERMINAL 04\n" +
            "CRITICAL ERROR\n\n" +
            "SYSTEM FAILURE\n" +
            "ERROR CODE: RST-042\n\n" +
            "DATABASE CONNECTION LOST\n\n" +
            "CORE PROCESS: UNRESPONSIVE\n\n" +
            "REBOOT REQUIRED.";

        diagnosticsButton.interactable = true;
        restartButton.interactable = false;
        databaseButton.interactable = false;

        rebootProgress.gameObject.SetActive(false);
        rebootProgress.value = 0;
    }

    public void RunDiagnostics()
    {
        if (repairStep != 0 || isRebooting || isRepaired)
            return;

        repairStep = 1;

        computerScreenText.text =
            "RESEARCH TERMINAL 04\n" +
            "DIAGNOSTICS COMPLETE\n\n" +
            "ERROR DETECTED\n" +
            "ERROR CODE: RST-042\n\n" +
            "DATABASE CONNECTION LOST\n\n" +
            "CORE PROCESS: UNRESPONSIVE\n\n" +
            "MANUAL RESTART REQUIRED.";

        diagnosticsButton.interactable = false;
        restartButton.interactable = true;
    }

    public void RestartCore()
    {
        if (repairStep != 1 || isRebooting || isRepaired)
            return;

        repairStep = 2;

        computerScreenText.text =
            "RESEARCH TERMINAL 04\n" +
            "CORE RESTARTED\n\n" +
            "CORE PROCESS: RUNNING\n\n" +
            "DATABASE CONNECTION: LOST\n\n" +
            "DATABASE RESTORE REQUIRED.";

        restartButton.interactable = false;
        databaseButton.interactable = true;
    }

    public void RestoreDatabase()
    {
        if (repairStep != 2 || isRebooting || isRepaired)
            return;

        repairStep = 3;

        databaseButton.interactable = false;

        StartCoroutine(RebootComputer());
    }

    private IEnumerator RebootComputer()
    {
        isRebooting = true;

        computerScreenText.text =
            "RESEARCH TERMINAL 04\n" +
            "REBOOTING...\n\n" +
            "RESTORING RESEARCH DATABASE...\n\n" +
            "PLEASE WAIT.";

        rebootProgress.gameObject.SetActive(true);
        rebootProgress.value = 0;

        float elapsed = 0f;

        while (elapsed < rebootDuration)
        {
            elapsed += Time.deltaTime;

            rebootProgress.value =
                Mathf.Clamp01(elapsed / rebootDuration) * 100f;

            yield return null;
        }

        rebootProgress.value = 100f;

        isRebooting = false;
        isRepaired = true;

        computerScreenText.text =
            "RESEARCH TERMINAL 04\n" +
            "SYSTEM RESTORED\n\n" +
            "DATABASE CONNECTION: ACTIVE\n\n" +
            "CORE PROCESS: RUNNING\n\n" +
            "RESEARCH DATA: RECOVERED\n\n" +
            "ALL SYSTEMS OPERATIONAL.";
    }
}
