using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupText : MonoBehaviour {

    // Object references
    private Animation anim;
    private TextMeshPro textMesh;

    // Initializes the popup text and disables it for later use
    void Awake() {
        anim = GetComponent<Animation>();
        textMesh = GetComponent<TextMeshPro>();
        gameObject.SetActive(false);
        
    }

    // Sets the text of the popup text
    public void setText(string text) {
        textMesh.SetText(text);
    }

    // Activates the popup text and plays its animation
    public void createPopup() {
        gameObject.SetActive(true);
        anim.Play();
    }

    // Deactivates the popup text
    private void deactivate() {
        gameObject.SetActive(false);
    }
}
