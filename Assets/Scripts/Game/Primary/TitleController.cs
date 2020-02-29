using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleController : MonoBehaviour {

    // References and settings
    public Tetromino[] tetrominos;
    private int topY = int.MinValue;
    private int bottomY = int.MaxValue;
    public float fallrate = 0.065f;
    public TextMeshProUGUI titleText;
    public ParticleSystem particleEffect;
    private ParticleSystem.MainModule particleEffectMain;

    // Shuffle Tetromino colors on first awake and load keybinds
    private void Awake() {

        // First awake.
        if (Time.time == 0) {
            BlockUpdater.shuffleColorPallet();
            if (Screen.fullScreen != Settings.fullScreen) {
                Screen.fullScreen = Settings.fullScreen;
            }
        }


        BlockUpdater.updateColors();
        BlockUpdater.updateSprites();

        //if (!false) {                // Check for saved player prefs here?
            Settings.keyBinds = new Dictionary<Board.PlayerInput, KeyCode>(Settings.defaultKeybinds);
        //}

        particleEffectMain = particleEffect.main;
        particleEffectMain.startColor = new ParticleSystem.MinMaxGradient(titleText.fontSharedMaterial.GetColor(ShaderUtilities.ID_FaceColor));
    }

    // Initialize title animation
    void Start() {
        Time.timeScale = 1;
        for (int i = 0; i < tetrominos.Length; i++) {
            for (int j = 0; j < (int) (Random.value * 4); j++) {
                tetrominos[i].rotate(1);
            }
            if (tetrominos[i].transform.position.y > topY) {
                topY = (int)tetrominos[i].transform.position.y;
            }
            if (tetrominos[i].transform.position.y < bottomY) {
                bottomY = (int)tetrominos[i].transform.position.y;
            }
            tetrominos[i].InvokeRepeating("down", Random.Range(0, fallrate), fallrate);
        }
    }

    // Play title animation
    void Update() {
        
        for (int i = 0; i < tetrominos.Length; i++) {
            if (Camera.main.WorldToScreenPoint(tetrominos[i].transform.position).y < 4) {
                Vector3 position = tetrominos[i].transform.position;
                position.y = (topY - bottomY) - (topY - bottomY + position.y) % 1;
                tetrominos[i].transform.position = position;
                for (int j = 0; j < (int)(Random.value * 4); j++) {
                    tetrominos[i].rotate(1);
                }
            }
        }
        
    }

    public void play() {
        PageLoader.goToGameModeScreen();
    }

    public void quit() {
        PageLoader.exitGame();
    }
}