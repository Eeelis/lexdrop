using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager uiManager;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject newGameButton;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text lastPossibleWordText;

    private bool canStartNewGame;


    private void Awake()
    {
        uiManager = this;
        canStartNewGame = true;
    }

    private void Start()
    {
        gameOverScreen.SetActive(false);
        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("highscore").ToString();
        mainMenu.SetActive(true);
    }

    private void Update()
    {
        if (mainMenu.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) && canStartNewGame)
            {
                StartGame();
            }
        }
        else if (gameOverScreen.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) && canStartNewGame)
            {
                NewGame();
            }
        }
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
    }
    
    public void HideMainMenu()
    {
        mainMenu.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);
        gameOverText.text = "";
        scoreText.text = "";
        StartCoroutine(AnimateGameOverScreen());
        lastPossibleWordText.text = "Last possible word: " + WordManager.wordManager.TargetWord;
    }

    private IEnumerator AnimateGameOverScreen()
    {
        int totalCharacters = "Game Over!".Length;
        int currentCharacter = 0;

        while (currentCharacter < totalCharacters)
        {
            gameOverText.text += "Game Over!"[currentCharacter];
            currentCharacter++;

            yield return new WaitForSeconds(0.08f);
        }

        StartCoroutine(AnimateScoreText());
    }

    private IEnumerator AnimateScoreText()
    {
        string text = "Score: " + GameManager.gameManager.CurrentScore.ToString() + "\n\nHigh Score: " + GameManager.gameManager.HighScore.ToString();
        int totalCharacters = text.Length;
        int currentCharacter = 0;

        while (currentCharacter < totalCharacters)
        {
            scoreText.text += text[currentCharacter];
            currentCharacter++;

            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(0.25f);

        ShowNewGameButton();
    }

    private void ShowNewGameButton()
    {
        newGameButton.SetActive(true);
        LeanTween.moveLocalY(newGameButton, 0, 0.75f).setEaseOutExpo().setOnComplete(() => canStartNewGame = true);
    }

    public void NewGame()
    {
        canStartNewGame = false;
        LeanTween.moveLocalY(newGameButton, -500, 0.5f).setEaseInBack().setOnComplete(HideGameOverScreenAndStartGame);
    }

    public void StartGame()
    {
        canStartNewGame = false;
        LeanTween.moveLocalY(startGameButton, -500, 0.5f).setEaseInBack().setOnComplete(HideMainMenuAndStartGame);
    }

    private void HideGameOverScreenAndStartGame()
    {
        gameOverScreen.SetActive(false);
        GameManager.gameManager.StartNewGame();
    }

    private void HideMainMenuAndStartGame()
    {
        HideMainMenu();
        GameManager.gameManager.StartNewGame();
    }

    // Called on mouse over
    public void HighLightNewGameButton()
    {
        LeanTween.scale(newGameButton, new Vector3(1.05f, 1.05f, 1.05f), 0.2f);
        LeanTween.scale(startGameButton, new Vector3(1.05f, 1.05f, 1.05f), 0.2f);
    }

    // Called on mouse exit
    public void ResetNewGameButton()
    {
        LeanTween.scale(newGameButton, Vector3.one, 0.2f);
        LeanTween.scale(startGameButton, Vector3.one, 0.2f);
    }
}
