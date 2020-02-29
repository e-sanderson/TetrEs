using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlsButton : MonoBehaviour {

    [SerializeField] private Board.PlayerInput input;
    [SerializeField] private TextMeshProUGUI text;

    // For loading keybinda
    public void Start() {
        setText(Settings.keyBinds[input].ToString());
    }

    // To update text
    public void setText(string newText) {
        text.SetText(newText);
    }

    // Retrieves the input associated with the button for reading purposes.
    public Board.PlayerInput getInput() {
        return input;
    }

}
