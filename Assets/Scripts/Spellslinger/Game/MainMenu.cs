using Spellslinger.Game.Control;
using Spellslinger.Game.Manager;
using Spellslinger.Game.XR;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    private XRInputManager input;

    [Header("Menu Panels")]
    [SerializeField] private GameObject startGamePanel;
    [SerializeField] private GameObject confirmNewGamePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Outline Objects")]
    [SerializeField] private GameObject[] startGameOutlineObjects;
    [SerializeField] private GameObject[] optionsOutlineObjects;
    [SerializeField] private GameObject[] creditsOutlineObjects;
    [SerializeField] private GameObject[] endGameOutlineObjects;

    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;

    private GameObject selectedObject;
    private bool hasSavedGame = false;

    private void Start() {
        // find dependencies in scene
        this.input = GameObject.Find("-- XR --").GetComponent<XRInputManager>();

        // initialize eventlisteners
        this.input.OnControllerTrigger += this.SelectMenuItem;
        this.input.OnPreferredControllerChanged += this.input.SetUIMode;

        // Check if there is a save file
        this.hasSavedGame = SaveGameManager.SaveFileExists();
        if (!hasSavedGame) {
            this.startGamePanel.transform.Find("Continue").gameObject.GetComponent<Button>().interactable = false;
        }
    }

    private void Update() {
        RaycastHit hit = this.input.GetWandSelection();

        if (this.selectedObject != null && hit.collider != null && this.selectedObject != hit.collider.gameObject) {
            GameManager.Instance.PlayAudioClip(this.hoverSound, 0.5f);
        }

        this.selectedObject = hit.collider != null ? hit.collider.gameObject : null;

        if (this.selectedObject != null) {
            switch (this.selectedObject.name) {
                case "START":
                    this.ShowOutline(this.startGameOutlineObjects);
                    this.HideOutline(this.optionsOutlineObjects);
                    this.HideOutline(this.creditsOutlineObjects);
                    this.HideOutline(this.endGameOutlineObjects);

                    break;
                case "OPTIONS":
                    this.HideOutline(this.startGameOutlineObjects);
                    this.ShowOutline(this.optionsOutlineObjects);
                    this.HideOutline(this.creditsOutlineObjects);
                    this.HideOutline(this.endGameOutlineObjects);

                    break;
                case "CREDITS":
                    this.HideOutline(this.startGameOutlineObjects);
                    this.HideOutline(this.optionsOutlineObjects);
                    this.ShowOutline(this.creditsOutlineObjects);
                    this.HideOutline(this.endGameOutlineObjects);

                    break;
                case "END GAME":
                    this.HideOutline(this.startGameOutlineObjects);
                    this.HideOutline(this.optionsOutlineObjects);
                    this.HideOutline(this.creditsOutlineObjects);
                    this.ShowOutline(this.endGameOutlineObjects);

                    break;

                default:
                    this.HideOutline(this.startGameOutlineObjects);
                    this.HideOutline(this.optionsOutlineObjects);
                    this.HideOutline(this.creditsOutlineObjects);
                    this.HideOutline(this.endGameOutlineObjects);

                    break;
            }
        } else {
            this.HideOutline(this.startGameOutlineObjects);
            this.HideOutline(this.optionsOutlineObjects);
            this.HideOutline(this.creditsOutlineObjects);
            this.HideOutline(this.endGameOutlineObjects);
        }
    }

    /// <summary>
    /// Select a menu depending on the highlighted object.
    /// </summary>
    private void SelectMenuItem(bool triggerPressed, XRInputManager.Controller controller) {
        if (!triggerPressed || this.selectedObject == null) {
            return;
        }

        switch (this.selectedObject.name) {
            case "START":
                this.startGamePanel.SetActive(true);
                this.confirmNewGamePanel.SetActive(false);
                this.optionsPanel.SetActive(false);
                this.creditsPanel.SetActive(false);

                break;
            case "OPTIONS":
                this.startGamePanel.SetActive(false);
                this.confirmNewGamePanel.SetActive(false);
                this.optionsPanel.SetActive(true);
                this.creditsPanel.SetActive(false);

                break;
            case "CREDITS":
                this.startGamePanel.SetActive(false);
                this.confirmNewGamePanel.SetActive(false);
                this.optionsPanel.SetActive(false);
                this.creditsPanel.SetActive(true);

                break;
            case "END GAME":
                Application.Quit();

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Show the outline for the given outline objects.
    /// </summary>
    /// <param name="outlineObjects">The objects where the ouline shoould be enabled.</param>
    private void ShowOutline(GameObject[] outlineObjects) {
        foreach (GameObject outlineObject in outlineObjects) {
            Outline outline = outlineObject.GetComponent<Outline>();

            if (outline != null) {
                outline.enabled = true;
            }
        }
    }

    /// <summary>
    /// Hide the outline for the given outline objects.
    /// </summary>
    /// <param name="outlineObjects">The objects where the ouline shoould be disabled.</param>
    private void HideOutline(GameObject[] outlineObjects) {
        foreach (GameObject outlineObject in outlineObjects) {
            Outline outline = outlineObject.GetComponent<Outline>();

            if (outline != null) {
                outline.enabled = false;
            }
        }
    }

    /// <summary>
    /// Continue the game from the last save point.
    /// </summary>
    public void ContinueGame() {
        GameManager.Instance.PlayAudioClip(this.selectSound);
        GameManager.Instance.LoadLevel(SaveGameManager.Instance.GetSaveData().currentLevel);
    }

    /// <summary>
    /// Start a new game. Ask for confirmation if there is a save file.
    /// </summary>
    /// <param name="confirmation">If true, a new game will be started even if there is a save file.</param>
    public void StartNewGame(bool confirmation = false) {
        GameManager.Instance.PlayAudioClip(this.selectSound);

        if (!this.hasSavedGame || confirmation) {
            GameManager.Instance.LoadLevel(1);
        } else {
            this.confirmNewGamePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Reset the menu panels to the default state.
    /// </summary>
    public void ResetMenu() {
        GameManager.Instance.PlayAudioClip(this.selectSound);

        this.startGamePanel.SetActive(false);
        this.confirmNewGamePanel.SetActive(false);
        this.optionsPanel.SetActive(false);
        this.creditsPanel.SetActive(false);
    }
}
