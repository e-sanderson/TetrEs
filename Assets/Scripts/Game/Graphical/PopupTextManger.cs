using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupTextManger : MonoBehaviour {
    // Object References
    public PopupText tetris;
    public PopupText tspin;
    public PopupText backtoback;
    public PopupText combo;

    // Creates the "Tetris!" popup
    public void createTetrisPopup() {
        tetris.createPopup();
    }

    // Creates a "T-Spin!" pop up. Text depends on the type of T-psin and number of lines cleared with it
    public void createTSpinPopup(bool mini, uint lines) {
        string ts = mini ? "T-Spin Mini" : "T-Spin";
        string ln;
        switch (lines) {
            case 1:
                ln = "\nSingle!";
                break;
            case 2:
                ln = "\nDouble!";
                break;
            case 3:
                ln = "\nTriple!";
                break;
            default:
                ln = "!";
                break;
        }
        tspin.setText(string.Concat(ts, ln));
        tspin.createPopup();
    }

    // Creates the "Back to back!" popup text
    public void createBackToBackPopup() {
        backtoback.createPopup();
    }

    // Creates the "Combo!" popup. Text depends on combo length
    public void createComboPopup(int comboSize) {
        combo.setText(string.Concat("Combo!\n", comboSize.ToString()));
        combo.createPopup();
           
    }
}
