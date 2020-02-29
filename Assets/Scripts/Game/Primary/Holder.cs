using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holder : MonoBehaviour {

    // The tetromino in reserve
    private Tetromino heldTetromino = null;

    // True if a swap made with the previous piece
    private bool prevSwap = false;

    // True if rotation be preserved when swapping
    private bool preserveRotation = Settings.preserveRotation;

    // True if the holder allowed in the current game mode
    private bool active = Settings.holderActive;

    // Diable holder if it is not to be used
    void Start() {
        if (!active) {
            gameObject.SetActive(false);
        }
    }

    // Stores a piece and retures what was in reserve
    public Tetromino swap(Tetromino t) {
        if (prevSwap || t == null || !active) {
            return t;
        }
        else {
            prevSwap = true;
            if (!preserveRotation) {
                t.resetRotation();
            }
            Tetromino returnTetromino = heldTetromino;
            t.transform.SetParent(transform);
            t.moveCenterTo(transform.position);
            t.setMinoBaseColor(Color.gray);
            t.setGhostActive(false);
            heldTetromino = t;
            if (returnTetromino != null) {
                returnTetromino.transform.SetParent(transform.parent);
                returnTetromino.moveToStartPosition(transform.parent);
                returnTetromino.setGhostActive(true);
                
            }
            return returnTetromino;
        }
    }
    
    // Allows a piece to be swapped again after being locked upon initially being swapped
    public void reset() {
        if (heldTetromino != null) {
            heldTetromino.setMinoBaseColor(Color.white);
        }
        prevSwap = false;
    }

}
