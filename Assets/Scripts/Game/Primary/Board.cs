using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    // Peripherals
#pragma warning disable 0649
    [SerializeField] private Spawner spawner;
    [SerializeField] private Holder hold;
    [SerializeField] private Graphics graphics;
    [SerializeField] private Score score;

    // UI Menus
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject gameOptionsMenu;
    [SerializeField] private GameObject gameOverMenu;

    // Board Dimensions
    private static uint WIDTH = 10;
    private static uint HEIGHT = 20;
    private static uint EXTRA_HEIGHT = 4;

    // Game management objects
    private Transform[,] boardGrid = new Transform[WIDTH, HEIGHT + EXTRA_HEIGHT];
    private Tetromino activePiece = null;
    private bool gameOver = false;
    private bool paused = false;

    /*
     * Game parameters
     */

    // Player input
    public enum PlayerInput {
        RIGHT,
        LEFT,
        ROTATE_RIGHT,
        ROTATE_LEFT,
        DROP,
        SWAP,
        SOFTDROP,
        PAUSE,
        NONE,
    }
    private PlayerInput input = PlayerInput.NONE;
    private Dictionary<PlayerInput, KeyCode> keyBinds = Settings.keyBinds;
    

    // Falling piece
    private float fallDelay = 0.8f;
    private ContinuousTimer fallTimer = new ContinuousTimer();
    private bool softDropping = false;
    //private uint minY;

    // Movement
    private bool kick = Settings.kick;
    private bool successfulMovement = false;
    private PlayerInput lastSuccessfulMove = PlayerInput.NONE;
    private ContinuousTimer moveTimer = new ContinuousTimer();
    private DiscreteTimer moveRepeatTimer = new DiscreteTimer();

    // Line Clearing
    private List<uint> linesToClear = new List<uint>();
    private float clearTime = Settings.clearTime;
    private float postClearFallDelay = Settings.postClearFallDelay;
    private float postFallSpawnDelay = Settings.postFallSpawnDelay;
    private uint topEmptyRow = 0;

    // Locking
    private uint lockResetCap = Settings.lockResetCap;
    private uint lockResetCount = 0;
    private float lockDelay = Settings.lockDelay;
    private DiscreteTimer lockTimer = new DiscreteTimer();

    // Scoring
    uint softDrop = 0;
    uint hardDrop = 0;
    uint tspin = 0;


    // Startup code
    void Start() {
        fallDelay = calculateFallRate(score.level);
        Time.timeScale = 1;
    }

    // Main game loop
    void Update() {
        if (paused && !Input.GetKeyDown(keyBinds[PlayerInput.PAUSE]) || gameOptionsMenu.active || controlsMenu.active) {
            return;
        }
        if (linesToClear.Count == 0 && !gameOver) {
            // Spawn and check GameOver if last piece landed
            if (activePiece == null) {

                if (lockDelay <= 0.1f) {
                    if (fallTimer.time < fallDelay) {
                        return;
                    }
                }

                // Check GameOver
                gameOver = checkGameOver();
                if (gameOver) {
                    Debug.Log("Game Over");
                    StartCoroutine(gameOverRoutine());
                    return;
                }

                // Spawn next piece
                activePiece = spawner.nextTetromino();
                if (!Settings.showGhost) {
                    activePiece.setGhostVisible(false);
                }
                resetPiece();
            }

            // Player input
            successfulMovement = false;
            input = getPlayerInput();

            switch (input) {
                case PlayerInput.NONE:
                    moveRepeatTimer.reset();
                    break;
                case PlayerInput.DROP:
                    Vector3 prev = activePiece.centerPosition();
                    activePiece.moveToGhostPosition();
                    Vector3 cur = activePiece.centerPosition();
                    hardDrop = (uint)Mathf.Round(prev.y - cur.y);
                    graphics.tetrominoBeam(prev, cur, activePiece); 
                    lastSuccessfulMove = PlayerInput.DROP;
                    moveRepeatTimer.reset();
                    break;
                case PlayerInput.SWAP:
                    activePiece = hold.swap(activePiece);
                    if (activePiece == null) {
                        return;
                    }
                    resetPiece();
                    moveRepeatTimer.reset();
                    break;
                default:
                    if (moveRepeatTimer.time == 0) {
                        // Just move, first press
                        successfulMovement = handleMovement(input);
                        if (successfulMovement) {
                            lastSuccessfulMove = input;
                            moveTimer.reset();
                        }
                        moveRepeatTimer.update();
                    } else if (moveRepeatTimer.time >= Settings.moveDelay) {
                        // Only move if delay has been passed and only every so often
                        if (moveTimer.time > Settings.moveSpeed) {
                            successfulMovement = handleMovement(input);
                            if (successfulMovement) {
                                lastSuccessfulMove = input;
                                moveTimer.reset();
                            }
                        }
                    } else {
                        moveRepeatTimer.update();
                    }
                    break;
            }

            // Move piece down or decrement lock timer
            if (!pieceLanded()) {
                if (fallTimer.time > (softDropping ? Settings.moveSpeed / 4 : fallDelay)) {
                    // Down
                    activePiece.translateVertical(-1);
                    lastSuccessfulMove = PlayerInput.NONE;
                    fallTimer.reset();

                    softDrop = softDropping ? softDrop + 1 : 0;

                } else {
                    softDrop = softDropping ? softDrop : 0;
                }
            } else {

                // Reset timers on successful move if appropriate
                if (successfulMovement) {
                    if (lockResetCount < lockResetCap) {
                        lockTimer.reset();
                        lockResetCount++;
                    } else {
                        lockTimer.update();
                    }
                    
                } else {
                    lockTimer.update();
                }
                
            }


            // Lock piece, clear lines, update score
            if ((pieceLanded() && lockTimer.time > lockDelay) || input == PlayerInput.DROP) {

                // Note tSpin. Must be done here before piece container is destroyed.
                tspin = isTspin(lastSuccessfulMove);

                // Lock piece, release blocks, any held piece
                lockPiece();
                hold.reset();
                activePiece.pulseDetachMinosAndDestroy();
                activePiece = null;

                // Clear lines
                checkLineClears();

                // Update Score and Level
                bool levelUp = score.updateScore((uint)linesToClear.Count, softDrop, hardDrop, tspin);
                if (levelUp) {
                    if (Settings.dynamicColor) {
                        graphics.fadeToNextColorPallet();
                    }
                    
                    fallDelay = calculateFallRate(score.level);
                }

                if (linesToClear.Count > 0) {
                    clearLines();
                }
            }
        }
    }

    /*
     * Game Management Functions
     */

     public void togglePause() {
        if (paused) {
            Time.timeScale = 1;
            foreach (Transform t in transform) {
                t.gameObject.SetActive(true);
            }
            GetComponent<SpriteRenderer>().enabled = true;
            pauseMenu.SetActive(false);
            paused = false;
        } else {
            Time.timeScale = 0;
            BlockUpdater.activeMinos = GameObject.FindGameObjectsWithTag("Mino");
            BlockUpdater.activeGhosts = GameObject.FindGameObjectsWithTag("GhostMino");
            foreach (Transform t in transform) {
                t.gameObject.SetActive(false);
            }
            GetComponent<SpriteRenderer>().enabled = false;
            pauseMenu.SetActive(true);
            paused = true;
        } 
    }

    public void toggleControls() {
        if (pauseMenu.gameObject.active) {
            pauseMenu.gameObject.SetActive(false);
            controlsMenu.gameObject.SetActive(true);
        } else {
            pauseMenu.gameObject.SetActive(true);
            controlsMenu.gameObject.SetActive(false);
        }
    }

    public void toggleGameOptions() {
        if (pauseMenu.gameObject.active) {
            pauseMenu.gameObject.SetActive(false);
            gameOptionsMenu.gameObject.SetActive(true);
        } else {
            pauseMenu.gameObject.SetActive(true);
            gameOptionsMenu.gameObject.SetActive(false);
        }
    }

    public void restart() {
        PageLoader.playGame();
    }

    public void goToGameMode() {
        PageLoader.goToGameModeScreen();
    }

    public void exit() {
        PageLoader.exitGame();
    }


    private void resetPiece() {
        softDrop = 0; hardDrop = 0;
        fallTimer.reset();
        lockTimer.reset();
        lockResetCount = 0;
        lastSuccessfulMove = PlayerInput.NONE;
        while (!validPosition(activePiece.getPiece())) {
            activePiece.translateVertical(1);

        }
        updateGhost();
    }

    private bool checkGameOver() {
        for (uint row = HEIGHT; row < HEIGHT + EXTRA_HEIGHT; row++) {
            if (checkRow(row) != 0) {
                return true;
            }
        }

        return false;
    }

    private IEnumerator gameOverRoutine() {
        foreach (Tetromino t in spawner.transform.GetComponentsInChildren<Tetromino>()) {
            t.GetComponent<Tetromino>().setMinoBaseColor(Color.gray);
        }
        foreach (Tetromino t in hold.transform.GetComponentsInChildren<Tetromino>()) {
            t.GetComponent<Tetromino>().setMinoBaseColor(Color.gray);
        }
        int j = -1, dir = 1;
        for (int i = 0; i < HEIGHT + EXTRA_HEIGHT; i++) {
            for (j += dir; (j < WIDTH && j >= 0); j += dir) {
                Transform minoTransform = boardGrid[j, i];
                if (minoTransform != null) {
                    minoTransform.GetComponent<SpriteRenderer>().color = Color.gray;
                    yield return new WaitForSeconds(0.01f);

                }
            }
            dir = -dir;
        }
        yield return new WaitForSeconds(1);
        gameOverMenu.active = true;
    }

    private float calculateFallRate(uint level) {
        uint framesPerCell = 0;
        if (level < 9) {
            framesPerCell = 48 - 5 * level;
        }
        else if (level < 10) {
            framesPerCell = 6;
        }
        else if (level < 13) {
            framesPerCell = 5;
        }
        else if (level < 16) {
            framesPerCell = 4;
        }
        else if (level < 19) {
            framesPerCell = 3;
        }
        else if (level < 29) {
            framesPerCell = 2;
        }
        else {
            framesPerCell = 1;
        }
        return framesPerCell / 60.0f;
    }

    private uint isTspin(PlayerInput move) {
        if (activePiece.getShape() != Tetromino.Shape.T || !(move.Equals(PlayerInput.ROTATE_LEFT) || move.Equals(PlayerInput.ROTATE_RIGHT))) {
            return 0;
        }
        uint major = 0, minor = 0;
        foreach (Vector3 position in activePiece.tSpinMajorPositions()) {
            if (!emptyCell(position)) {
                major++;
            }
        }
        foreach (Vector3 position in activePiece.tSpinMinorPositions()) {
            if (!emptyCell(position)) {
                minor++;
            }
        }
        if (major + minor >= 3) {
            return major;

        }
        return 0;
    }

    /*
     * Player Input Functions
     */

    private PlayerInput getPlayerInput() {
        softDropping = Input.GetKey(keyBinds[PlayerInput.SOFTDROP]);
        if (Input.GetKey(keyBinds[PlayerInput.LEFT])) {
            return PlayerInput.LEFT;
        }
        else if (Input.GetKey(keyBinds[PlayerInput.RIGHT])) {
            return PlayerInput.RIGHT;
        }
        else if (Input.GetKey(keyBinds[PlayerInput.ROTATE_LEFT])) {
            return PlayerInput.ROTATE_LEFT;
        }
        else if (Input.GetKey(keyBinds[PlayerInput.ROTATE_RIGHT])) {
            return PlayerInput.ROTATE_RIGHT;
        }
        else if (Input.GetKeyDown(keyBinds[PlayerInput.DROP])) {
            return PlayerInput.DROP;
        }
        else if (Input.GetKeyDown(keyBinds[PlayerInput.SWAP])) {
            return PlayerInput.SWAP;
        }
        else if (Input.GetKeyDown(keyBinds[PlayerInput.PAUSE])) {
            togglePause();
            return PlayerInput.PAUSE;
        } else {
            return PlayerInput.NONE;
        }
    }

    private bool handleMovement(PlayerInput move) {
        bool success = false;
        switch (move) {
            case PlayerInput.RIGHT:
                success = translate(1);
                break;
            case PlayerInput.LEFT:
                success = translate(-1);
                break;
            case PlayerInput.ROTATE_RIGHT:
                success = rotate(-1);
                break;
            case PlayerInput.ROTATE_LEFT:
                success = rotate(1);
                break;
        }
        updateGhost();
        return success;
    }

    /*
     * Piece Movevement Functions
     */
    private bool translate(int dir) {
        activePiece.translateHorizontal(dir);
        if (!validPosition(activePiece.getPiece())) {
            activePiece.translateHorizontal(-dir);
            return false;
        }
        return true;
    }

    private bool rotate(int dir) {
        activePiece.rotate(dir);
        if (!validPosition(activePiece.getPiece())) {
            if (kick) {
                Vector3 originalPos = activePiece.transform.position;
                foreach (Vector3 correction in activePiece.getKickOptions()) {
                    activePiece.moveTo(originalPos + correction);
                    if (validPosition(activePiece.getPiece())) {
                        return true;
                    }
                }
                activePiece.moveTo(originalPos);
            }
            activePiece.rotate(-dir);
            return false;
        }
        return true;
    }

    public void updateGhost() {
        if (activePiece != null) {

            GameObject ghost = activePiece.getGhost();
            if (ghost.active) {
                activePiece.moveGhostToCurrentPosition();
                while (validPosition(ghost)) {
                    ghost.transform.Translate(0, -1, 0, Space.World);
                }
                if (!ghost.transform.position.Equals(activePiece.transform.position)) {
                    ghost.transform.Translate(0, 1, 0, Space.World);
                }
            }

        }
    }

    public bool validPosition(GameObject piece) {
        foreach (Transform minoTransform in piece.transform) {
            if (!emptyCell(minoTransform.position)) {
                return false;
            }
        }
        return true;
    }

    public bool emptyCell(Vector3 pos) {
        int x = (int)Mathf.Round(pos.x - transform.position.x);
        int y = (int)Mathf.Round(pos.y - transform.position.y);
        return x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT + EXTRA_HEIGHT && boardGrid[x, y] == null;
    }

    /*
     * Piece Landing and Locking Functions
     */

    public void lockPiece() {
        foreach (Transform t in activePiece.getPiece().transform) {
            Vector3 p = t.position - transform.position;
            p.x = Mathf.Round(p.x);
            p.y = Mathf.Round(p.y);
            boardGrid[(int)p.x, (int)p.y] = t;
        }
        graphics.createSparkle(activePiece.centerPosition(), activePiece.getColor());
    }

    private bool pieceLanded() {
        int diff = (int)Mathf.Round(activePiece.transform.position.y - activePiece.getGhost().transform.position.y);
        if (diff == 0) {
            return true;
        } else if (diff > 0) {
            return false;
        } else {
            throw new System.Exception("Piece Has Fallen Below Ghost Position!");
        }
    }

    /*
     * Line Clearing Functions
     */

    private int checkRow(uint row) {
        /*
         * Checks if a row is completely full, completely empty, or mixed
         * 
         * Returns 1 if completely full
         * Returns 0 if completely empty
         * Returns -1 if mixed
         * 
         */
        int firstVal = boardGrid[0, row] == null ? 0 : 1;
        for (int i = 1; i < WIDTH; i++) {
            if ((boardGrid[i, row] == null ? 0 : 1) != firstVal) {
                return -1;
            }
        }
        return firstVal;
    }

    public int checkLineClears() {
        for (uint row = 0; row < HEIGHT + EXTRA_HEIGHT; row++) {
            switch (checkRow(row)) {
                case 0:
                    topEmptyRow = row;
                    return linesToClear.Count;
                case 1:
                    linesToClear.Add(row);
                    break;
            }
        }

        return linesToClear.Count;
    }

    private void clearLines() {
        StartCoroutine(clearLinesRoutine());
    }

    public IEnumerator clearLinesRoutine() {
        if (linesToClear.Count >= 4 || (tspin > 0 && linesToClear.Count > 0)) {
            graphics.flash();
        }
        /*
         * Line Clearing Block
         * 
         * Note:
         * Mino.explode() Destroys the mino object, so no need to destroy objects or set the grid space to null 
         */
        for (int i = 0; i < WIDTH / 2; i++) {
            foreach (uint line in linesToClear) {
                Transform t1 = boardGrid[WIDTH / 2 + i, line];
                Transform t2 = boardGrid[WIDTH / 2 - i - 1, line];
                graphics.createExplosion(t1.position, t1.gameObject.GetComponent<SpriteRenderer>().material.color);
                graphics.createExplosion(t2.position, t2.gameObject.GetComponent<SpriteRenderer>().material.color);
                Destroy(t1.gameObject);
                Destroy(t2.gameObject);
            }
            yield return new WaitForSeconds(clearTime / 5);
        }

        yield return new WaitForSeconds(postClearFallDelay);

        /*
         * Line Falling Block
         */
        uint fallDistance = 0;
        for (uint row = 0; row < topEmptyRow; row++) {
            if (linesToClear.Contains(row)) {
                fallDistance++;
            }
            else {
                for (uint col = 0; col < WIDTH; col++) {
                    Transform t = boardGrid[col, row];
                    if (t != null) {
                        t.Translate(0, -fallDistance, 0, Space.World);
                        boardGrid[col, row] = null;
                        boardGrid[col, row - fallDistance] = t;
                    }
                }
            }
        }
        yield return new WaitForSeconds(postFallSpawnDelay);

        /*
         * Inform main loop that line clearing has finished.
         */
        linesToClear.Clear();
    }

    /* 
     * Utility classes
     */

    private class ContinuousTimer {
        /*
         * A basic timer that continuously updates its measured time
         */
        private float startTime = 0;
        public float time { get { return Time.time - startTime; } private set { } }
        public void reset() {
            startTime = Time.time;
        }
    }

    private class DiscreteTimer {
        /*
         * A timer that only updates its measured time when requested
         */
        public float time { get; private set; }
        public void reset() {
            time = 0;
        }
        public void update() {
            time += Time.deltaTime;
        }
    }
}


