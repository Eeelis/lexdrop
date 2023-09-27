using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    [SerializeField] private GameObject score;
    [SerializeField] private GameObject scoreText;
    [SerializeField] private GameObject bottomLines;

    private int currentScore = 0;
    public int CurrentScore
    {
        get { return currentScore; }
    }

    private int highScore = 0;
    public int HighScore
    {
        get { return highScore; }
    }

    private float speed = 2;
    public float Speed
    {
        get { return speed; }
    }


    private void Awake()
    {
        gameManager = this;
        highScore = PlayerPrefs.GetInt("highscore");
    }

    private void Start()
    {
        HideGameUI();
    }

    public void IncreaseScore()
    {
        currentScore += 1;
        // Also increase the speed at which letters fall
        if (speed < 8)
        {
            speed += 0.05f;
        }
        
        scoreText.GetComponent<TextMeshPro>().text = currentScore.ToString();
        LeanTween.scale(scoreText, new Vector3(1.5f, 1.5f, 1.5f), 0.75f).setEasePunch();  
    }

    public void StartNewGame()
    {
        currentScore = 0;
        speed = 2;
        scoreText.GetComponent<TextMeshPro>().text = "0";
        UIManager.uiManager.HideMainMenu();
        ShowGameUI();
        LetterSpawner.letterSpawner.SpawnNewLetter();
    }

    public void GameOver()
    {
        HideGameUI();

        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("highscore", currentScore);
            highScore = currentScore;
        }

        WordManager.wordManager.ClearWord(false);
        LetterSpawner.letterSpawner.DestroyCurrentLetter();
        UIManager.uiManager.ShowGameOverScreen();
    }

    public void ShowGameUI()
    {
        score.SetActive(true);
        bottomLines.SetActive(true);
    }

    public void HideGameUI()
    {
        score.SetActive(false);
        bottomLines.SetActive(false);
    }
}
