using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOptionsMenu : MonoBehaviour {

    // UI elements
    [SerializeField] private Toggle dynamicColor;
    [SerializeField] private Slider moveDelay;
    [SerializeField] private TextMeshProUGUI moveDelayText;
    [SerializeField] private Slider moveSpeed;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private Toggle fullScreen;

    // Update display blocks and load previous settings
    void Start() {
        BlockUpdater.updateSprites();
        BlockUpdater.updateColors();
        dynamicColor.isOn = Settings.dynamicColor;
        moveDelay.value = Settings.moveDelay * 100;
        moveSpeed.value = Settings.moveSpeed * 100;
        fullScreen.isOn = Screen.fullScreen;
    }

    public void toggleDynamicColor() {
        Settings.dynamicColor = dynamicColor.isOn;
    }

    public void updateRepeat(Slider slider) {
        if (slider == moveDelay) {
            Settings.moveDelay = slider.value / 100;
            moveDelayText.SetText((slider.value * 10).ToString() + " ms");
        }
        else {
            Settings.moveSpeed = slider.value / 100;
            moveSpeedText.SetText((slider.value * 10).ToString() + " ms");
        }
    }

    public void toggleFullscreen() {
        Settings.fullScreen = fullScreen.isOn;
        Screen.fullScreen = Settings.fullScreen;
    }

    public void changeBlockColor(bool next) {
        if (next) {
            BlockUpdater.nextColorPallet();
        } else {
            BlockUpdater.prevColorPallet();
        }
        BlockUpdater.updateColors();
    }

    public void changeBlockSprite(bool next) {
        if (next) {
            BlockUpdater.nextSprite();
        }
        else {
            BlockUpdater.prevSprite();
        }
        BlockUpdater.updateSprites();
    }

    
}
