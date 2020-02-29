using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    // Popup text management
    public PopupTextManger popup;

    // Score
    public uint score { get; private set; } = 0;
    public TextMeshPro scoreText;

    // Level
    public uint level { get; private set; } = Settings.startLevel;
    private uint levelProgress = 0;
    public TextMeshPro levelText;
    public Image levelProgressFill;


    // Lines
    private uint lineTotal = 0;
    private uint[] lines = new uint[4];
    public TextMeshPro[] linesText;
    public TextMeshPro lineTotalText;

    // Tspins
    uint tSpinTotal = 0;
    uint miniTSpinTotal = 0;

    // Statistics
    private uint[] statistics = new uint[7];

    // Scoring Policies
    public enum ScorePolicy {
        CLASSIC, 
        MODERN
    }
    private ScorePolicy scorePolicy = Settings.scorePolicy;

    // Classic Scoring References
    private readonly uint[] classicLinePoints = new uint[] { 0, 40, 100, 300, 1200 };

    // Modern Scoring Reference
    private readonly uint[] modernLinePoints = new uint[] {0, 100, 300, 500, 800, 100, 200, 400, 400, 800, 1200, 1600};

    // Modern Scoring Parameters
    bool difficultPreviousClear = false;
    float backToBackMultiplier = 1.5f;
    int combo = 0;
    uint comboBonus = 50;

    private void Start() {
        levelText.SetText(level.ToString());
    }

    public bool updateScore(uint linesCleared, uint softDrop, uint hardDrop, uint tspin) {

        // Lines
        if (linesCleared > 0) {
            lineTotal += linesCleared;
            lines[linesCleared - 1]++;
            linesText[linesCleared - 1].SetText(lines[linesCleared - 1].ToString());
            lineTotalText.SetText(lineTotal.ToString());
            if (linesCleared == 4) {
                popup.createTetrisPopup();
            }
        }
        
        // Tspins
        switch (tspin) {
            case 1:
                popup.createTSpinPopup(true, linesCleared);
                miniTSpinTotal++;
                break;
            case 2:
                popup.createTSpinPopup(false, linesCleared);
                tSpinTotal++;
                break;
        }

        // BackToBack
        uint clearType = modernScoreIndex(linesCleared, tspin);
        bool b2b = backToBack(clearType, linesCleared);
        if (b2b) {
            // BackToBack graphic
            popup.createBackToBackPopup();
        }

        // Combo
        combo = linesCleared > 0 ? combo + 1 : -1;
        if (combo > 0) {
            // Combo graphinc
            popup.createComboPopup(combo);
        }


        // Score
        switch (scorePolicy) {
            case ScorePolicy.CLASSIC:
                score += (level + 1) * classicLinePoints[linesCleared] + softDrop + hardDrop;
                break;
            case ScorePolicy.MODERN:
                float linePoints = modernLinePoints[clearType];
                uint points = (uint)Mathf.Round(linePoints * (b2b ? backToBackMultiplier : 1));
                uint comboPoints = (uint)(comboBonus * (level + 1) * (combo > 0 ? combo : 0));
                score +=  (level + 1) * points  + softDrop + 2 * hardDrop + comboPoints;
                break;
        }
        scoreText.SetText(score.ToString());

        // Level
        return updateLevel(linesCleared);

        
    }

    /* index is given by index = 5 * tspin + lines, Unless its a full tspin, then its index = 4 x tspin + lines.
     * This is to ensure that the indices are [0, 4] - No tspin, [5, 7], Mini tspin, [8, 11]. No wasted space.
     * 
     * Note: tspin=0->No tspin, tspin=1->Mini tspin, tspin=2->Tspin.
     * Examples:
     * 
     * index = 6 -> 5 * 1 + 1 -> Mini tspin, 1 line
     * index = 1 -> 5 * 0 + 1 -> No tspin, 1 lines
     * index = 11 -> 4 * 2 + 3 -> tspin, 3 lines
     * 
     * 
     * idx = (tspin >= 1 ? 5 : 4) * tspin + lines
     * Entries are [(0lines, NoTs), (1line, NoTs), (2lines, NoTs), ... , (0lines, MiniTs), ... , (3lines, Ts)]
     */
    private uint modernScoreIndex(uint linesCleared, uint tspin) {
        return (uint)(tspin == 2 ? 4 : 5) * tspin + linesCleared;
    }

    // Returns true if the cleartype was a tetris or a tspin line clear.
    private bool difficultClear(uint clearType) {
        return !(clearType < 4 || clearType == 5 || clearType == 8);
    }

    // Returns true if the last two line clears involved a "difficult" type of clear (Tetris, Tspin, ...)
    private bool backToBack(uint cleartype, uint linesCleared) {
        bool difficult = difficultClear(cleartype);
        if (difficult && difficultPreviousClear) {
            // Lines were cleared, and they were difficult (tetris clear or tspin clear). Back to back chain has been maintained
            return true;
        } else if (linesCleared != 0) {
            // Chain was either broken or started.
            // Either way, it wasnt a back to back, but note it
            difficultPreviousClear = difficult;
        }
        return false;
    }

    // Returns true if a level up has occured
    private bool updateLevel(uint lines) {
        levelProgress += lines;
        if (levelProgress >= 10) {
            levelProgress %= 10;
            levelProgressFill.fillAmount = levelProgress / 10.0f;
            level++;
            levelText.SetText(level.ToString());
            return true;
        }
        levelProgressFill.fillAmount = levelProgress / 10.0f;
        return false;
    }

}
