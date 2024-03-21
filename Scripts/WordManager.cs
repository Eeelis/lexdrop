using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using System;
using System.IO;


public class WordManager : MonoBehaviour
{
    [SerializeField] private Letter letterPrefab;

    private List<string> wordList = new List<string>();
    private Letter[] currentWord = new Letter[5];
    private char[] allLetters = "abcdefghijklmnopqrsty".ToCharArray();
    private Letter currentLetter;
    private Vector3 letterSpawnPosition = new Vector3(0, 4, 0);
    private string targetWord = "";
    private float initialLetterFallingSpeed = 2;
    private float currentLetterFallingSpeed;
    private int previousRandomLetterIndex = 0;

    public static event Action<string> OnGameOver = (lastPossibleWord) => {};
    public static event Action OnLetterAdded = () => {};
    public static event Action OnScoreIncreased = () => {};

    private void Awake()
    {
        currentLetterFallingSpeed = initialLetterFallingSpeed;
        UIManager.OnGameStart += SpawnRandomLetter;
        StartCoroutine(LoadWordList());
    }

    private IEnumerator LoadWordList()
    {
        string fullPath = Application.isEditor
            ? Path.Combine(Application.dataPath, "StreamingAssets/wordList.txt")
            : Path.Combine(Application.streamingAssetsPath, "wordList.txt");

        using (UnityWebRequest www = UnityWebRequest.Get(fullPath))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "text/plain; charset=utf-8");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string[] lines = www.downloadHandler.text.Split('\n');

                foreach (string line in lines)
                {
                    string cleanLine = new string(line.Where(c => !char.IsControl(c)).ToArray());
                    wordList.Add(cleanLine);
                }
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();
                }
            }
            else 
            {
                Debug.LogError("Failed to load file: " + www.error);
            }
        }
    }

    public void AddLetter(Letter letter, int lane)
    {
        // If the lane already contains a letter, move the new letter to the first available lane
        if (currentWord[lane] != null)
        {
            lane = FindEmptyLane(lane);
            MoveLetterToLane(letter, lane);
        }

        currentWord[lane] = letter;

        // If the word is complete, clear it, increase the score and spawn a new letter to start a new word
        if (wordList.Any(w => w.Equals(GetCurrentWordAsString())))
        { 
            ClearWord();
            OnScoreIncreased.Invoke();
            AudioManager.Instance.PlayAudioClip(AudioManager.AudioClips.ScoreUp);
            currentLetterFallingSpeed += 0.02f;
            SpawnMatchingLetter();
        }
        else 
        {
            // Othervise simply spawn the next letter
            SpawnMatchingLetter();
        }

        AudioManager.Instance.PlayRandomKeyPressSound();
    }

    // The word being built is represented by list of Letter objects. This function converts that list into a string.
    public string GetCurrentWordAsString()
    {
        StringBuilder combinedText = new StringBuilder();

        foreach (Letter letter in currentWord)
        {
            if (letter != null && letter.GetComponent<TextMeshPro>().text != null)
            {
                combinedText.Append(letter.GetComponent<TextMeshPro>().text);
            }
            else 
            {
                combinedText.Append(" ");
            }
        }

        return combinedText.ToString();
    }
    
    public void SpawnRandomLetter()
    {
        currentLetter = Instantiate(letterPrefab, letterSpawnPosition, Quaternion.identity);
        currentLetter.Initialize(this, currentLetterFallingSpeed, GenerateRandomLetter());
    }

    public void SpawnMatchingLetter()
    {
        currentLetter = Instantiate(letterPrefab, letterSpawnPosition, Quaternion.identity);
        currentLetter.Initialize(this, currentLetterFallingSpeed, GenerateMatchingLetter());
    }

    public string GenerateRandomLetter()
    {
        int randomIndex = UnityEngine.Random.Range(0, allLetters.Length);
        return allLetters[randomIndex].ToString();
    }

    // Find a letter that could be used to make a word based on the existing letters in the word that is being built
    private string GenerateMatchingLetter()
    {
        List<string> matchingWords = FindMatchingWords();

        if (matchingWords.Count > 0)
        {
            string randomMatchingWord = GetRandomMatchingWord(matchingWords);

            List<string> potentialLetters = FindPotentialLetters(randomMatchingWord);

            targetWord = randomMatchingWord;

            if (potentialLetters.Count > 0)
            {
                return GetRandomLetter(potentialLetters);
            }
        }

        // No matches, game over
        GameOver();
        return "";
    }

    private void GameOver()
    {
        AudioManager.Instance.PlayAudioClip(AudioManager.AudioClips.GameOver);
        OnGameOver.Invoke(targetWord);

        Destroy(currentLetter.gameObject);
        ClearWord(false);

        currentLetterFallingSpeed = initialLetterFallingSpeed;
    }

    // Find words with letters in the same positions as the word that is being built
    private List<string> FindMatchingWords()
    {
        List<string> matchingWords = new List<string>();

        foreach (string word in wordList)
        {
            bool isMatch = true;

            for (int i = 0; i < 5; i++)
            {
                if (GetCurrentWordAsString().ToLower()[i] != ' ' && GetCurrentWordAsString().ToLower()[i] != word[i])
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                matchingWords.Add(word);
            }
        }

        return matchingWords;
    }

    private string GetRandomMatchingWord(List<string> matchingWords)
    {
        return matchingWords[UnityEngine.Random.Range(0, matchingWords.Count)];
    }

    // Find all letters that could be used to make a word based on the letters in the word being built
    private List<string> FindPotentialLetters(string randomMatchingWord)
    {
        List<string> potentialLetters = new List<string>();

        for (int i = 0; i < 5; i++)
        {
            if (GetCurrentWordAsString().ToLower()[i] == ' ')
            {
                potentialLetters.Add(randomMatchingWord[i].ToString());
            }
        }

        return potentialLetters;
    }

    private string GetRandomLetter(List<string> potentialLetters)
    {
        return potentialLetters[UnityEngine.Random.Range(0, potentialLetters.Count)];
    }

    public void ClearWord(bool keepRandomLetter = true)
    {
        // Keep a random letter from the current word, making sure that the same letter is not kept twice
        if (keepRandomLetter)
        {
            int randomLetterIndex;

            do
            {
                randomLetterIndex = UnityEngine.Random.Range(0, 4);
            }
            while (randomLetterIndex == previousRandomLetterIndex);
            
            previousRandomLetterIndex = randomLetterIndex;

            Letter letterToKeeep = currentWord[randomLetterIndex];

            foreach (Letter letter in currentWord)
            {
                if(letter && letter != letterToKeeep)
                {
                    LeanTween.moveY(letter.gameObject, -8, 0.5f).setEaseInBack().setOnComplete(() => Destroy(letter.gameObject));
                }
            }

            currentWord = new Letter[5];
            currentWord[randomLetterIndex] = letterToKeeep;
        }
        else 
        {
            foreach (Letter letter in currentWord)
            {
                if(letter)
                {
                    Destroy(letter.gameObject);
                }
            }
            
            currentWord = new Letter[5];
        }   
    }

    // Check whether a given lane already contains a letter
    public bool IsOccupied(int lane)
    {
        return currentWord[lane] != null;
    }

    // Return first lane not containing a letter, starting from the left
    public int FindEmptyLane(int currentPosition)
    {
        for (int i = 0; i < currentWord.Length; i++)
        {
            if(!IsOccupied(i))
            {
                return i;
            }
        }

        return currentPosition;
    }

    // Moves a letter to given lane in a a parabolic curve.
    // We achieve this by splitting up the curve into multiple straight lines, and tweening the letter in along them. 
    public void MoveLetterToLane(Letter letter, int lane)
    {
        Vector3 startPoint = letter.transform.position;
        Vector3 target = new Vector3((lane - 2), startPoint.y, startPoint.z);
        int segments = 10;

        Vector3[] waypoints = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float parabolicT = Mathf.Sin(t * Mathf.PI);
            waypoints[i] = Vector3.Lerp(startPoint, target, t) + Vector3.up * parabolicT * 3;
        }

        for (int i = 0; i < segments; i++)
        {
            LeanTween.move(letter.gameObject, waypoints[i + 1], 1.5f / segments).setEase(LeanTweenType.linear);
        }
    }
}
