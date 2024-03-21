using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject scoreText;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject newGameButton;
    [SerializeField] private GameObject startGameButton;
    
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text lastPossibleWordText;

    private int currentScore;

    public static event Action OnGameStart = () => {};

    private void Awake()
    {
        WordManager.OnScoreIncreased += UpdateScoreText;
        WordManager.OnGameOver += ShowGameOverScreen;
    }

    private void Start()
    {
        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("highscore").ToString();
        mainMenu.SetActive(true);
    }

    private void UpdateScoreText()
    {
        currentScore += 1;
        scoreText.GetComponent<TextMeshPro>().text = currentScore.ToString();
        LeanTween.cancel(scoreText);
        LeanTween.scale(scoreText, scoreText.transform.localScale * 1.5f, 0.75f).setEasePunch(); 
    }

    private void Update()
    {
        if (mainMenu.activeInHierarchy || gameOverScreen.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine(AnimateGameOverScreen());
                StopCoroutine(AnimateScoreText());
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

        StartCoroutine(AnimateGameOverScreen());

        lastPossibleWordText.text = "Last possible word: " + lastPossibleWord;

        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("highscore", currentScore);
        }
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

        StartCoroutine(AnimateScoreTextInGameOverScreen());
    }

    private IEnumerator AnimateScoreTextInGameOverScreen()
    {
        string text = "Score: " + currentScore + "\n\nHigh Score: " + PlayerPrefs.GetInt("highscore").ToString();
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

    public void StartNewGame()
    {
        currentScore = 0;

        AudioManager.Instance.PlayAudioClip(AudioManager.AudioClips.StartGame);

        LeanTween.moveLocalY(newGameButton, -500, 0.5f).setEaseInBack();
        LeanTween.moveLocalY(startGameButton, -500, 0.5f).setEaseInBack().setOnComplete( () => {
                gameOverScreen.SetActive(false);
                mainMenu.SetActive(false);
                gameUI.SetActive(true);
                OnGameStart.Invoke();
            }
        );
    }

    // Called on mouse over
    public void HighLightNewGameButton()
    {
        LeanTween.scale(newGameButton, new Vector3(1.05f, 1.05f, 1.05f), 0.2f);
        LeanTween.scale(startGameButton, new Vector3(1.05f, 1.05f, 1.05f), 0.2f);
    }

    // Called on mouse exit
    public void UnHighLightNewGameButton()
    {
        LeanTween.scale(newGameButton, Vector3.one, 0.2f);
        LeanTween.scale(startGameButton, Vector3.one, 0.2f);
    }
}
