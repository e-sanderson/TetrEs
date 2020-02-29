using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeMenu : MonoBehaviour {

    // UI elements
    [SerializeField] private ToggleGroup gameMode;
    [SerializeField] private Toggle classicMode;
    [SerializeField] private Toggle modernMode;
    [SerializeField] private Toggle customMode;
    [SerializeField] private Slider pieceLockDelay;
    [SerializeField] private TextMeshProUGUI lockDelayText;
    [SerializeField] private TMP_Dropdown pieceLockResetCap;
    [SerializeField] private TMP_Dropdown previews;
    [SerializeField] private Toggle showGhost;
    [SerializeField] private ToggleGroup pieceGeneration;
    [SerializeField] private Toggle classicGeneration;
    [SerializeField] private Toggle modernGeneration;
    [SerializeField] private Toggle hold;
    [SerializeField] private ToggleGroup score;
    [SerializeField] private Toggle classicScore;
    [SerializeField] private Toggle modernScore;
    [SerializeField] private TMP_Dropdown level;
    [SerializeField] private Toggle kick;

    public void Start() {
        loadSettings(true);
    }

    // Loads the current settings into the UI
    public void loadSettings(bool loadMode) {
        if (loadMode) { 
            // Game Mode
            switch (Settings.gameMode) {
                case Settings.GameMode.CLASSIC:
                    classicMode.isOn = true;
                    modernMode.isOn = false;
                    customMode.isOn = false;
                    break;
                case Settings.GameMode.MODERN:
                    modernMode.isOn = true;
                    classicMode.isOn = false;
                    customMode.isOn = false;
                    break;
                case Settings.GameMode.CUSTOM:
                    customMode.isOn = true;
                    classicMode.isOn = false;
                    modernMode.isOn = false;
                    break;
            }
        }

        // Lock Delay
        pieceLockDelay.value = Settings.lockDelay * 10;
        lockDelayText.SetText((Settings.lockDelay).ToString() + " s");

        // Lock Delay Cap
        if (Settings.lockResetCap == uint.MaxValue) {
            pieceLockResetCap.value = 0;
        } else {
            pieceLockResetCap.value = (int)Settings.lockResetCap;
        }
        

        // Previews
        previews.value = Settings.numPreviews;

        switch (Settings.generationPolicy) {
            case Spawner.PieceGenerationPolicy.RANDOMBAG:
                modernGeneration.isOn = true;
                classicGeneration.isOn = false;
                break;
            case Spawner.PieceGenerationPolicy.REROLL:
                classicGeneration.isOn = true;
                modernGeneration.isOn = false;
                break;
        }

        // Show Ghost
        showGhost.isOn = Settings.showGhost;

        // Hold
        hold.isOn = Settings.holderActive;

        switch (Settings.scorePolicy) {
            case Score.ScorePolicy.CLASSIC:
                classicScore.isOn = true;
                modernScore.isOn = false;
                break;
            case Score.ScorePolicy.MODERN:
                modernScore.isOn = true;
                classicScore.isOn = false;
                break;
        }

        level.value = (int) Settings.startLevel;

        kick.isOn = Settings.kick;
    }

    // Allows custom game modes
    public void toggleCustomization(bool active) {
        pieceLockDelay.enabled = active;
        pieceLockResetCap.enabled = active;
        previews.enabled = active;
        pieceGeneration.enabled = active;
        hold.enabled = active;
        score.enabled = active;
        kick.enabled = active;
        showGhost.enabled = active;
        classicGeneration.enabled = active;
        modernGeneration.enabled = active;
        classicScore.enabled = active;
        modernScore.enabled = active;
    }

    // Applyie the "Classic" game mode template
    public void applyClassicMode() {
        Settings.applyClassicSettings();
        loadSettings(false);
        toggleCustomization(false);
    }

    // Applies the "Modern" game mode template
    public void applyModernMode() {
        Settings.applyModernSettings(); 
        loadSettings(false);
        toggleCustomization(false);
    }

    // Allows a custom game mode to be configured
    public void applyCustomMode() {
        Settings.gameMode = Settings.GameMode.CUSTOM;
        toggleCustomization(true);
    }

    public void updatePieceLockDelay() {
        Settings.lockDelay = pieceLockDelay.value / 10;
        lockDelayText.SetText((pieceLockDelay.value / 10).ToString() + " s");
    }

    public void updateLockResetCap() {
        Settings.lockResetCap = pieceLockResetCap.value == 0 ? uint.MaxValue : (uint) pieceLockResetCap.value;
    }

    public void updateShowGhost() {
        Settings.showGhost = showGhost.isOn;
    }

    public void updatePreviewSelect() {
        Settings.numPreviews = previews.value;
    }

    public void updateScorePolicy(Toggle selection) {
        if (selection.isOn) {
            if (selection == classicScore) {
                Settings.scorePolicy = Score.ScorePolicy.CLASSIC;
            }
            else if (selection == modernScore) {
                Settings.scorePolicy = Score.ScorePolicy.MODERN;
            }
        }
    }

    public void updateGenerationPolicy(Toggle selection) {
        if (selection.isOn) {
            if (selection == classicGeneration) {
                Settings.scorePolicy = Score.ScorePolicy.CLASSIC;
            }
            else if (selection == modernGeneration) {
                Settings.scorePolicy = Score.ScorePolicy.MODERN;
            }
        }
    }

    public void updateHold() {
        Settings.holderActive = hold.isOn;
    }

    public void updateKick() {
        Settings.kick = kick.isOn;
    }

    public void updateLevel() {
        Settings.startLevel = (uint) level.value;
    }

    // Toggles the given menu on or off
    public void toggleMenu(GameObject menu) {
        toggleSelf();
        menu.gameObject.SetActive(!menu.gameObject.active);
    }

    // Toggles this menu on or off
    public void toggleSelf() {
        GetComponent<Image>().enabled = !GetComponent<Image>().enabled;
        foreach (Transform t in GetComponentInChildren<Transform>()) {
            if (t != transform) {
                t.gameObject.active = !t.gameObject.active;
            }
        }
    }

    public void title() {
        PageLoader.goToTitleScreen();
    }

    public void play() {
        PageLoader.playGame();
    }

}
