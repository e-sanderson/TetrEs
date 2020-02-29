using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PageLoader {

    public static void playGame() {
        SceneManager.LoadScene("Game");
    }

    public static void goToGameModeScreen() {
        SceneManager.LoadScene("Mode");
    }

    public static void goToTitleScreen() {
        SceneManager.LoadScene("Title");
    }

    public static void exitGame() {
        Application.Quit();
    }
}
