using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject newGameButton;
    [SerializeField] private GameObject startGameButton;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text lastPossibleWordText;

    private Coroutine showGameOverScreenCoroutine;
    private Coroutine animateScoreTextCoroutine;

    private int currentScore;

    public static event Action OnGameStart = () => {};

    private void Start()
    {
        WordManager.OnScoreIncreased += UpdateScoreText;
        WordManager.OnGameOver += ShowGameOverScreen;
        WordManager.OnGameOver += StoreHighScore;
        WordManager.OnGameOver += ResetScore;

        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("highscore", 0).ToString();

        mainMenu.SetActive(true);
        gameUI.SetActive(false);
        gameOverScreen.SetActive(false);
    }

    private void ResetScore(string lastPossibleWord)
    {
        currentScore = 0;
        scoreText.text = currentScore.ToString();
    }

    private void StoreHighScore(string lastPossibleWord)
    {
        if (currentScore > PlayerPrefs.GetInt("highscore", 0))
        {
            PlayerPrefs.SetInt("highscore", currentScore);
        }
    }

    public void UpdateScoreText()
    {
        currentScore += 1;
        scoreText.text = currentScore.ToString();
        LeanTween.cancel(scoreText.gameObject);
        LeanTween.scale(scoreText.gameObject, scoreText.transform.localScale * 1.5f, 0.75f).setEasePunch(); 
    }

    private void Update()
    {
        if (mainMenu.activeInHierarchy || gameOverScreen.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (showGameOverScreenCoroutine != null)
                {
                    StopCoroutine(showGameOverScreenCoroutine);
                }
                
                if (animateScoreTextCoroutine != null)
                {
                    StopCoroutine(animateScoreTextCoroutine);
                }
                
                NewGame();
            }
        }
    }

    public void ShowGameOverScreen(string lastPossibleWord)
    {
        gameUI.SetActive(false);
        gameOverScreen.SetActive(true);

        gameOverText.text = "";
        gameOverScoreText.text = "";

        showGameOverScreenCoroutine = StartCoroutine(AnimateGameOverScreen());

        lastPossibleWordText.text = "Last possible word: " + lastPossibleWord;
    }

    private IEnumerator AnimateGameOverScreen()
    {
        int totalCharacters = "Game Over!".Length;
        int currentCharacter = 0;

        while (currentCharacter < totalCharacters)
        {
            gameOverText.text += "Game Over!"[currentCharacter];
            currentCharacter++;

            if (currentCharacter % 2 == 0)
            {
                AudioManager.Instance.PlayRandomKeyPressSound(0.4f);
            }

            yield return new WaitForSeconds(0.08f);
        }

        animateScoreTextCoroutine = StartCoroutine(AnimateScoreText());
    }

    private IEnumerator AnimateScoreText()
    {
        string text = "Score: " + currentScore + "\n\nHigh Score: " + PlayerPrefs.GetInt("highscore", 0).ToString();
        int totalCharacters = text.Length;
        int currentCharacter = 0;

        while (currentCharacter < totalCharacters)
        {
            gameOverScoreText.text += text[currentCharacter];
            currentCharacter++;

            if (currentCharacter % 2 == 0)
            {
                AudioManager.Instance.PlayRandomKeyPressSound(0.4f);
            }

            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(0.25f);

        ShowNewGameButton();
    }

    private void ShowNewGameButton()
    {
        newGameButton.SetActive(true);
        LeanTween.moveLocalY(newGameButton, 0, 0.75f).setEaseOutExpo();
    }

    public void NewGame()
    {
        AudioManager.Instance.PlayAudioClip(AudioManager.AudioClips.StartGame);

        LeanTween.moveLocalY(newGameButton, -5000, 0.5f).setEaseInBack();
        LeanTween.moveLocalY(startGameButton, -5000, 0.5f).setEaseInBack().setOnComplete( () => {
                gameOverScreen.SetActive(false);
                mainMenu.SetActive(false);
                gameUI.SetActive(true);
                OnGameStart.Invoke();
            }
        );
    }

    public void HighLightNewGameButton()
    {
        LeanTween.scale(newGameButton, new Vector3(1.05f, 1.05f, 1.05f), 0.2f);
        LeanTween.scale(startGameButton, new Vector3(1.05f, 1.05f, 1.05f), 0.2f);
    }

    public void ResetNewGameButton()
    {
        LeanTween.scale(newGameButton, Vector3.one, 0.2f);
        LeanTween.scale(startGameButton, Vector3.one, 0.2f);
    }
}
