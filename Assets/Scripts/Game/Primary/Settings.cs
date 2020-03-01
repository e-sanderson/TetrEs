using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings {

    // Game modes
    public enum GameMode {
        CUSTOM,
        CLASSIC,
        MODERN,
        
    }

    // Keybinds
    public static readonly Dictionary<Board.PlayerInput, KeyCode> defaultKeybinds = new Dictionary<Board.PlayerInput, KeyCode>() {
        { Board.PlayerInput.RIGHT, KeyCode.D },
        { Board.PlayerInput.LEFT, KeyCode.A },
        { Board.PlayerInput.ROTATE_RIGHT, KeyCode.E },
        { Board.PlayerInput.ROTATE_LEFT, KeyCode.Q },
        { Board.PlayerInput.DROP, KeyCode.W },
        { Board.PlayerInput.SWAP, KeyCode.R },
        { Board.PlayerInput.SOFTDROP, KeyCode.S },
        { Board.PlayerInput.PAUSE, KeyCode.Space }
    };

    public static Dictionary<Board.PlayerInput, KeyCode> keyBinds;

    public static GameMode gameMode = GameMode.CUSTOM;
    
    // Line Clearing
    public static readonly float clearTime = 0.25f;
    public static readonly float postClearFallDelay = 0.5f;
    public static readonly float postFallSpawnDelay = 0.25f;

    // Movement
    public static bool kick = true;
    public static uint lockResetCap = 15;
    public static float lockDelay = 0.5f;

    /*
     * Spawner Settings
     */

    [Range(0, 5)]
    public static int numPreviews = 5;
    public static int maxReroll = 1;
    public static Spawner.PieceGenerationPolicy generationPolicy = Spawner.PieceGenerationPolicy.RANDOMBAG;

    /*
     * Holder Settings
     */

    public static bool preserveRotation = false;
    public static bool holderActive = true;

    /*
     * Score Settings
     */

    public static uint startLevel = 0;
    public static Score.ScorePolicy scorePolicy = Score.ScorePolicy.MODERN;

    /*
     * Misc Settings
     */

    public static bool showGhost = true;

    /*
     * Preferences 
     */

    public static float moveSpeed = 0.08f;

    public static float moveDelay = 0.17f;

    public static bool dynamicColor = true;

    public static bool fullScreen = false;

    // Applies the "Classic" settngs template
    public static void applyClassicSettings() {
        gameMode = GameMode.CLASSIC;
        kick = false;
        lockResetCap = uint.MaxValue;
        lockDelay = 0;
        numPreviews = 1;
        generationPolicy = Spawner.PieceGenerationPolicy.REROLL;
        holderActive = false;
        scorePolicy = Score.ScorePolicy.CLASSIC;
        showGhost = false;
    }

    // Applies the "Modern" settings template
    public static void applyModernSettings() {
        gameMode = GameMode.MODERN;
        kick = true;
        lockResetCap = 15;
        lockDelay = 0.5f;
        numPreviews = 5;
        generationPolicy = Spawner.PieceGenerationPolicy.RANDOMBAG;
        holderActive = true;
        scorePolicy = Score.ScorePolicy.MODERN;
        showGhost = true;
    }
}
