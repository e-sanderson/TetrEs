using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetromino : MonoBehaviour {

    // Shape ID order: I L O R S T Z

    #pragma warning disable 0649

    // General references
    [SerializeField] private GameObject piece;
    [SerializeField] private GameObject ghost;
    [SerializeField] private Vector3 startPosition;
    [SerializeField]
    private Shape shape;

    // Rotation information
    public static int initialRotationState = 0;
    public int currentRotationState = 0;
    private int previousRotationState = 3;

    // Data for computing appropriate wall kicks
    private static readonly Vector3[][] kickOptions = new Vector3[][] { new Vector3[] { new Vector3(-1, 0, 0), new Vector3(-1, 1, 0), new Vector3(0, -2, 0), new Vector3(-1, -2, 0) }, // RLSTZ 0->1 || 2->1
                                                                        new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, -1, 0), new Vector3(0, 2, 0), new Vector3(1, 2, 0) },     // RLSTZ 1->0 || 1->2
                                                                        new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, -2, 0), new Vector3(1, -2, 0) },    // RLSTZ 2->3 || 0->3
                                                                        new Vector3[] { new Vector3(-1, 0, 0), new Vector3(-1, -1, 0), new Vector3(0, 2, 0), new Vector3(-1, 2, 0) },  // RLSTZ 3->2 || 3->0
                                                                        new Vector3[] { new Vector3(-2, 0, 0), new Vector3(1, 0, 0), new Vector3(-2, -1, 0), new Vector3(1, 2, 0) },   //     I 0->1 || 3->2
                                                                        new Vector3[] { new Vector3(2, 0, 0), new Vector3(-1, 0, 0), new Vector3(2, 1, 0), new Vector3(-1, -2, 0) },   //     I 1->0 || 2->3
                                                                        new Vector3[] { new Vector3(-1, 0, 0), new Vector3(2, 0, 0), new Vector3(-1, 2, 0), new Vector3(2, 1, 0) },    //     I 1->2 || 0->3
                                                                        new Vector3[] { new Vector3(-1, 0, 0), new Vector3(2, 0, 0), new Vector3(-1, 2, 0), new Vector3(2, -1, 0) },   //     I 2->1 || 3->0
                                                               }; 

    // Shape enum
    public enum Shape {
        I, L, O, R, S, T, Z
    }

    // Returns the movement options available for a given piece when performing a wall kick
    public Vector3[] getKickOptions() {
        /*
         * O piece will never call this fucntion so the two cases are I block and not I block
         */
        if (shape != Shape.I) {
            if (((previousRotationState == 0 && currentRotationState == 1) || (previousRotationState == 2 && currentRotationState == 1))) {
                return kickOptions[0];
            } else if (((previousRotationState == 1 && currentRotationState == 0) || (previousRotationState == 1 && currentRotationState == 2))) {
                return kickOptions[1];
            } else if (((previousRotationState == 2 && currentRotationState == 3) || (previousRotationState == 0 && currentRotationState == 3))) {
                return kickOptions[2];
            } else if (((previousRotationState == 3 && currentRotationState == 2) || (previousRotationState == 3 && currentRotationState == 0))) {
                return kickOptions[3];
            }
        } else if (shape == Shape.O) {
            throw new System.Exception("O-Block should not request kick options");
        } else {
            if (((previousRotationState == 0 && currentRotationState == 1) || (previousRotationState == 3 && currentRotationState == 2))) {
                return kickOptions[4];
            }
            else if (((previousRotationState == 1 && currentRotationState == 0) || (previousRotationState == 2 && currentRotationState == 3))) {
                return kickOptions[5];
            }
            else if (((previousRotationState == 1 && currentRotationState == 2) || (previousRotationState == 0 && currentRotationState == 3))) {
                return kickOptions[6];
            }
            else if (((previousRotationState == 2 && currentRotationState == 1) || (previousRotationState == 3 && currentRotationState == 0))) {
                return kickOptions[7];
            }
        }
        throw new System.Exception("Unhandled wall kick condition.");
    }

    // Translates the Tetromino vertically
    public void translateVertical(int x) {
        transform.Translate(0, x, 0, Space.World);
        ghost.transform.Translate(0, -x, 0, Space.World);
    }

    // Moves the Tetromino down
    public void down() {
        translateVertical(-1);
    }

    // Translates the Tetromino horizontally
    public void translateHorizontal(int x) {
        transform.Translate(x, 0, 0, Space.World);
    }

    // Rotates the Tetromino
    public void rotate(int a) {
        transform.Rotate(0, 0, a * 90);
        ghost.transform.Rotate(0, 0, a * 90);
        ghost.transform.RotateAround(transform.position, Vector3.forward, -90 * a);
        previousRotationState = currentRotationState;
        currentRotationState = (currentRotationState - a + 4) % 4;
        foreach (Transform minoTransform in piece.transform) {
            minoTransform.Rotate(0, 0, -a * 90);
        }
        foreach (Transform minoTransform in ghost.transform) {
            minoTransform.Rotate(0, 0, -a * 90);
        }
        
    }

    // Moves the Tetromino to a desired position
    public void moveTo(Vector3 p) {
        transform.position = p;
        moveGhostToCurrentPosition();
    }

    // Moves the tetromino to the same position as its ghost piece
    public void moveToGhostPosition() {
        moveTo(ghost.transform.position);
    }

    // Moves the ghost piece to the Tetrominos current position
    public void moveGhostToCurrentPosition() {
        ghost.transform.position = transform.position;
    }
    

    // Moves the center of the Tetromino to the desired position (For display purposes)
    public void moveCenterTo(Vector3 p) {
        moveTo(p - centerOffset());
    }

    // Moves the Tetromino to its start position
    public void moveToStartPosition(Transform relativeTo) {
        moveTo(startPosition + relativeTo.position);
    }

    // Resets the rotation of the tetromino to its initial state
    public void resetRotation() {
        while (currentRotationState != initialRotationState) {
            rotate(1);
        }
    }
    
    // Saves the current rotation state of the tetromino as its "intial" rotation state
    public void saveRotationState() {
        initialRotationState = currentRotationState;
    }

    // Retrieves the positions of the minos in the piece
    public IEnumerable<Vector3> getMinoPositions() {
        foreach (Transform t in piece.transform) {
            yield return t.localPosition + transform.localPosition;
        }
    }

    // Changes the scale of the tetromino (For display purposes)
    public void setScale(float scl) {
        transform.localScale = scl * Vector3.one;
    }

    // Returns the center position of the Tetromino (For display purposes)
    public Vector3 centerPosition() {
        return transform.position + centerOffset();
    }

    // Returns the offset from the center of the piece of the Tetrominos position (For computing center position)
    public Vector3 centerOffset() {
        Vector3 min, max;
        minoExtrema(out min, out max);
        return (max + min) / 2;
    }

    // Returns the dimensions of a piece (I is 1x4, L is 2x3, etc...)
    public Vector2 dimensions() {
        Vector3 min, max;
        minoExtrema(out min, out max);
        return max - min + Vector3.one;
    }

    // Returns the smallest and largest positions of the minos in each direction (For computation purposes)
    private void minoExtrema(out Vector3 min, out Vector3 max) {
        max = Vector3.negativeInfinity;
        min = Vector3.positiveInfinity;
        Vector3 p;
        foreach (Transform t in piece.transform) {
            p = t.position - transform.position;
            max = Vector3.Max(max, p);
            min = Vector3.Min(min, p);
        }
    }

    // Destroys the Tetromino and its ghost piece
    public void Destroy() {
        Destroy(ghost.gameObject);
        Destroy(gameObject);
    }

    // Releases the Minos from the Tetromino to be placed in the Board and destroys the container of the piece
    public void pulseDetachMinosAndDestroy() {
        Destroy(ghost.gameObject);
        Animation a = GetComponent<Animation>();
        a.Play(); // This animation will detach the minos once finished
    }

    // Detaches the minos and destroys the container object
    public void detachMinosAndDestroy() {
        for (int i = piece.transform.childCount - 1; i >= 0; i--) {
            piece.transform.GetChild(i).parent = null;
        }
        Destroy(gameObject);
    }

    // Sets the base colors of each mino (For display purposes)
    public void setMinoBaseColor(Color32 color) {
        foreach (Transform minoTransform in piece.transform) {
            minoTransform.GetComponent<SpriteRenderer>().color = color;
        }
    }

    // Returns the container of the Minos within the Tetromino
    public GameObject getPiece() {
        return piece;
    }

    // Returns the container of the ghost Minos within the Tetrominos
    public GameObject getGhost() {
        return ghost;
    }

    // Activates/Deactivates the ghost piece
    public void setGhostActive(bool a) {
        ghost.gameObject.SetActive(a);
    }

    // Makes the ghost piece visible/invisible
    public void setGhostVisible(bool b) {
        foreach (SpriteRenderer sr in ghost.transform.GetComponentsInChildren<SpriteRenderer>()) {
            sr.enabled = b;
        }
    }

    // Returns the color of the minos in the piece
    public Color32 getColor() {
        return piece.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sharedMaterial.color;
    }

    // Returns the "Major" positions for use in "T-spin" checks
    public Vector3[] tSpinMajorPositions() {
        return new Vector3[] { transform.position + transform.TransformVector(new Vector3(1, 1)),
                               transform.position + transform.TransformVector(new Vector3( -1, 1))};
    }

    // Returns the "minor" positions for use in "T-spin" checks
    public Vector3[] tSpinMinorPositions() {
        return new Vector3[] { transform.position + transform.TransformVector(new Vector3( -1, -1)),
                               transform.position + transform.TransformVector(new Vector3(1, -1))};
    }
    
    // Returns the shape of the Tetromino
    public Shape getShape() {
        return shape;
    }

    // Equality check for Tetrominos, They are equal if they have the same shape
    public override bool Equals(object other) {
        Tetromino t = other as Tetromino;
        if (Object.ReferenceEquals(t, null)) {
            return false;
        }
        
        return t.shape == shape;
    }
}
