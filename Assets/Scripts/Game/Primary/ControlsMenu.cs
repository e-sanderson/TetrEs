using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsMenu : MonoBehaviour {

    public ControlsButton selectedButton = null;
    public ControlsButton[] allButtons;


    // Main loop for updating keybinds
    void Update() {
        if (selectedButton != null) {
            // Iterate through all possible keys
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                // Key found, update binds
                if (Input.GetKeyDown(vKey)) { 
                    // Check if the new key is a key thats already bound
                    foreach (ControlsButton button in allButtons) {
                        if (vKey == Settings.keyBinds[button.getInput()]) {
                            Settings.keyBinds[button.getInput()] = KeyCode.None;
                            button.setText("");
                        }
                    }
                    bindKey(vKey, selectedButton);
                    selectedButton = null;
                }
            }
        }
    }

    public void updateKeybind(ControlsButton button) {
        selectedButton = button;
    }

    private void bindKey(KeyCode key, ControlsButton button) {
        Settings.keyBinds[button.getInput()] = key;
        button.setText(key.ToString());

    }

    public void resetKeybinds() {
        foreach (ControlsButton button in allButtons) {
            bindKey(Settings.defaultKeybinds[button.getInput()], button);
        }
    }
}
