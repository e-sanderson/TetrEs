using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    // General variables and settings
    private static Tetromino[] tetrominoList;
    private Queue<Tetromino> tetrominoQueue = new Queue<Tetromino>();
    private static int maxQueueSize = 5;
    private int queueSize = Settings.numPreviews;
    private float shrink = 0.5f;

    // REROLL policy settings and variables
    private int reroll = 0;
    private int maxReroll = Settings.maxReroll;
    private Tetromino previous;

    // RANDOMBAG variables
    private List<Tetromino> bag = new List<Tetromino>();

    // Reroll selects a mino at random uniformly
    // RandomBag places the 7 different pieces in a bag and removes 1 at a time until empty, then refills.
    public enum PieceGenerationPolicy {
        REROLL,
        RANDOMBAG,
    }
    private PieceGenerationPolicy generationPolicy = Settings.generationPolicy;

    // Initialize the piece preview window according to its settings
    void Start() {
        tetrominoList = Resources.LoadAll<Tetromino>("Prefabs/Tetrominos");
        if (queueSize == 0) {
            foreach (Transform t in transform) {
                Destroy(t.gameObject);
            }
        }
        else {
            for (int i = 0; i < maxQueueSize; i++) {
                if (i < queueSize) {
                    enqueueTetromino();
                }
                else {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
        updatePreviewWindows();
    }


    // Returns the next Tetromino
    public Tetromino nextTetromino() {
        Tetromino t = queueSize > 0 ? enqueueTetromino() : generateTetromino();
        updatePreviewWindows();
        t.transform.SetParent(transform.parent);
        t.moveToStartPosition(transform.parent);
        t.setGhostActive(true);
        return t;
    }

    // Adds a new Tetromino to the queue and returns the next Tetromino in the queue
    private Tetromino enqueueTetromino() {
        Tetromino t = generateTetromino();
        while (t.currentRotationState != Tetromino.initialRotationState) {
            t.rotate(1);
        }
        tetrominoQueue.Enqueue(t);
        Tetromino returnTetromino = tetrominoQueue.Count > queueSize ? tetrominoQueue.Dequeue() : null;
        return returnTetromino;
    }

    // Updates the preview windows
    private void updatePreviewWindows() {
        int idx = 0;
        foreach (Tetromino t in tetrominoQueue) {
            moveToWindow(t, idx);
            idx++;
            
        }
    }

    // Moves a piece to a particular window to be previewed
    private void moveToWindow(Tetromino t, int idx) {
        Transform window = transform.GetChild(idx);
        float scl = idx == 0 ? 1.0f : shrink;
        t.setScale(scl);
        
        t.transform.SetParent(window);
        t.moveCenterTo(window.position);

    }

    // Generates a random Tetromino for the queue
    private Tetromino generateTetromino() {
        Tetromino t;
        switch (generationPolicy) {
            case PieceGenerationPolicy.REROLL:
                t = randomTetromino();
                while(reroll < maxReroll && (t.Equals(previous) || tetrominoQueue.Contains(t))) {
                    t.Destroy();
                    t = randomTetromino();
                    reroll++;
                }
                previous = t;
                reroll = 0;
                break;
            case PieceGenerationPolicy.RANDOMBAG:
                if (bag.Count == 0) {
                    refillBag();
                }
                t = pullFromBag();
                break;
            default:
                t = randomTetromino();
                break;
        }
        t.setGhostActive(true);
        return t;
    }

    // Returns a Tetromino at random
    private Tetromino randomTetromino() {
        return Instantiate(tetrominoList[Random.Range(0, tetrominoList.Length)]);
    }

    // Refills the "bag" for the RANDOMBAG policy
    private void refillBag() {
        bag = new List<Tetromino>(tetrominoList);
    }

    // Retreives a piece at random from the "bag" when using the RANDOMBAG policy
    private Tetromino pullFromBag() {
        int idx = Random.Range(0, bag.Count);
        Tetromino t = Instantiate(bag[idx]);
        bag.RemoveAt(idx);
        return t;
    }
}
