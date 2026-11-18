using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SonarBehaviour : MonoBehaviour
{
    [Header("Sonar Display")]
    public GameObject sonarDisplay;
    public RectTransform sonarSweep;
    public GameObject sonarContact;
    public TMP_Text sonarStatusText;

    [Header("Sonar Settings")]
    public float sweepSpeed = 60f;
    public float contactDelay = 8f;

    private bool isPowered = false;

    private void Start()
    {
        isPowered = false;

        if (sonarDisplay != null)
            sonarDisplay.SetActive(false);

        if (sonarContact != null)
            sonarContact.SetActive(false);
    }

    private void Update()
    {
        if (!isPowered || sonarSweep == null)
            return;

        sonarSweep.Rotate(
            0f,
            0f,
            -sweepSpeed * Time.deltaTime
        );
    }

    public void RestoreSonarPower()
    {
        if (isPowered)
            return;

        isPowered = true;

        if (sonarDisplay != null)
            sonarDisplay.SetActive(true);

        if (sonarContact != null)
            sonarContact.SetActive(false);

        if (sonarStatusText != null)
            sonarStatusText.text = "SONAR ONLINE\nSCANNING...";

        StartCoroutine(DetectContact());
    }

    private IEnumerator DetectContact()
    {
        yield return new WaitForSeconds(contactDelay);

        if (!isPowered)
            yield break;

        if (sonarContact != null)
            sonarContact.SetActive(true);

        if (sonarStatusText != null)
            sonarStatusText.text =
                "WARNING\nUNIDENTIFIED CONTACT DETECTED";
    }
}
