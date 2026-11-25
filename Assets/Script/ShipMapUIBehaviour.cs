
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShipMapUIBehaviour : MonoBehaviour
{
    [Header("Deck Buttons")]
    public Button topDeckButton;
    public Button bottomDeckButton;

    [Header("Button Colors")]
    public Color currentDeckColor = Color.yellow;
    public Color hoverColor = Color.blue;
    public Color otherDeckColor = Color.white;

    [Header("Player Location")]
    public string currentPlayerDeck = "";

    // ------------------------------------
    // AWAKE
    // ------------------------------------

    void Awake()
    {
        ShowMouse();
    }

    // ------------------------------------
    // START
    // ------------------------------------

    void Start()
    {
        ShowMouse();

        // Find which scene the player came from.
        UpdatePlayerDeck();

        // Apply colors to the deck buttons.
        UpdateDeckButtonColors();
    }

    // ------------------------------------
    // UPDATE
    // ------------------------------------

    void Update()
    {
        ShowMouse();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMap();
        }
    }

    void LateUpdate()
    {
        ShowMouse();
    }

    // ------------------------------------
    // MOUSE
    // ------------------------------------

    private void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ------------------------------------
    // PLAYER DECK TRACKING
    // ------------------------------------

    void UpdatePlayerDeck()
    {
        currentPlayerDeck = PlayerPrefs.GetString(
            "MapReturnScene",
            "Top Deck"
        );

        Debug.Log(
            "Player currently on: " +
            currentPlayerDeck
        );
    }

    // ------------------------------------
    // BUTTON COLORS
    // ------------------------------------

    void UpdateDeckButtonColors()
    {
        bool playerOnTop =
            currentPlayerDeck == "Top Deck";

        bool playerOnBottom =
            currentPlayerDeck == "LabDeck2";

        SetButtonColors(
            topDeckButton,
            playerOnTop
        );

        SetButtonColors(
            bottomDeckButton,
            playerOnBottom
        );
    }

    void SetButtonColors(
        Button button,
        bool isCurrentDeck)
    {
        if (button == null)
            return;

        Color normalColor =
            isCurrentDeck
            ? currentDeckColor
            : otherDeckColor;

        // Use Unity's built-in button
        // color transition system.
        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;

        colors.normalColor = normalColor;

        // Blue when the mouse hovers.
        colors.highlightedColor = hoverColor;

        // Keep the button color after
        // keyboard selection.
        colors.selectedColor = normalColor;

        // Blue when clicked.
        colors.pressedColor = hoverColor;

        colors.disabledColor = normalColor;

        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;

        button.colors = colors;
    }

    // ------------------------------------
    // TOP DECK
    // ------------------------------------

    public void GoToTopDeck()
    {
        SceneManager.LoadScene("Top Deck");
    }

    // ------------------------------------
    // BOTTOM DECK
    // ------------------------------------

    public void GoToBottomDeck()
    {
        SceneManager.LoadScene("LabDeck2");
    }

    // ------------------------------------
    // EXIT MAP
    // ------------------------------------

    public void ExitMap()
    {
        string returnScene =
            PlayerPrefs.GetString(
                "MapReturnScene",
                "Top Deck"
            );

        SceneManager.LoadScene(returnScene);
    }
}
