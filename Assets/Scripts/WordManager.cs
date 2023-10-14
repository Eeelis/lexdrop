using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Linq;

public class WordManager : MonoBehaviour
{
    public static WordManager wordManager;

    private string targetWord = "";
    public string TargetWord
    {
        get { return targetWord; }
    }

    private Letter[] word = new Letter[5];
    private List<string> wordList = new List<string>();
    private string currentWord = "";
    private int previousRandomLetterIndex = 0;
    private bool gameOver = false;


    private void Awake()
    {
        wordManager = this;
    }

    private void Start()
    {
        StartCoroutine(LoadWordList());
    }

    public void AddLetter(Letter letter, int lane)
    {
        gameOver = false;

        // If the lane already contains a letter, move the new letter to the first available lane
        if (word[lane] != null)
        {
            lane = FindFirstFreePosition(lane);
            moveLetter(letter, new Vector3(LaneToXCoordinate(lane), letter.transform.position.y, letter.transform.position.z));
        }

        word[lane] = letter;
        currentWord = GetWordAsString();;

        bool isAWord = false;

        foreach (string w in wordList)
        {
            if (w.Equals(currentWord))
            {
                isAWord = true;
            }
        }

        if (isAWord)
        { 
            ClearWord();
            GameManager.gameManager.IncreaseScore();
            LetterSpawner.letterSpawner.SpawnNewLetter(GenerateAppropriateLetter());
        }
        else 
        {
            LetterSpawner.letterSpawner.SpawnNewLetter(GenerateAppropriateLetter());
        }

        if (gameOver)
        {
            GameManager.gameManager.GameOver();
        }

    }

    // The word being built is represented by list of strings. This function converts that list into a single string.
    public string GetWordAsString()
    {
        StringBuilder combinedText = new StringBuilder();

        foreach (Letter letter in word)
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

    private IEnumerator LoadWordList()
    {
        string fullPath;

        // Not sure why I have to do it like this, but everything else resulted in 404 either in the editor or in the build 
        if (Application.isEditor)
        {
            fullPath = System.IO.Path.Combine(Application.dataPath, "StreamingAssets/wordList.txt");
        }
        else
        {
            fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, "wordList.txt");
        }

        using (UnityWebRequest www = UnityWebRequest.Get(fullPath))
        {

            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "text/plain; charset=utf-8");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string[] lines = www.downloadHandler.text.Split('\n');

                // Store word in a list, removing any special characters
                foreach (string line in lines)
                {
                    string cleanedLine = new string(line.Where(c => !char.IsControl(c)).ToArray());
                    wordList.Add(cleanedLine);
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

    // Find a letter that could be used to make a word based on the existing letters
    private string GenerateAppropriateLetter()
    {
        List<string> matchingWords = new List<string>();

        // Find all words with letters in the same positions as the word being built
        foreach (string word in wordList)
        {
            bool isMatch = true;
            for (int i = 0; i < 5; i++)
            {
                if (currentWord.ToLower()[i] != ' ' && currentWord.ToLower()[i] != word[i])
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

        List<string> potentialLetters = new List<string>();
        
        // Find all letters that correspond to empty spots in the word being built
        if (matchingWords.Count > 0)
        {
            string randomMatchingWord = matchingWords[Random.Range(0, matchingWords.Count)];

            for (int i = 0; i < 5; i++)
            {
                if (currentWord.ToLower()[i] == ' ')
                {
                    potentialLetters.Add(randomMatchingWord[i].ToString());
                }
            }

            targetWord = randomMatchingWord;
        }

        // Return a random letter 
        if (potentialLetters.Count > 0)
        {
            return potentialLetters[Random.Range(0, potentialLetters.Count)];
        }
        else
        {
            // No matching word was found, game over
            gameOver = true;
            return "";
        }

    }

    public void ClearWord(bool keepRandomLetter = true)
    {
        if (keepRandomLetter)
        {
            int randomLetterIndex;

            do
            {
                randomLetterIndex = Random.Range(0, 4);
            }
            while (randomLetterIndex == previousRandomLetterIndex);
            
            previousRandomLetterIndex = randomLetterIndex;

            Letter letterToKeeep = word[randomLetterIndex];
            foreach (Letter l in word)
            {
                if(l && l != letterToKeeep)
                {
                    LeanTween.moveY(l.gameObject, -8, 0.5f).setEaseInBack().setOnComplete(() => DestroyLetter(l));
                }
            }

            word = new Letter[5];
            word[randomLetterIndex] = letterToKeeep;
            currentWord = GetWordAsString();
        }
        else 
        {
            foreach (Letter l in word)
            {
                if(l)
                {
                    Destroy(l.gameObject);
                }
            }
            
            word = new Letter[5];
            currentWord = GetWordAsString();
        }   
    }

    private void DestroyLetter(Letter l)
    {
        Destroy(l.gameObject);
    }

    // Check whether a given lane already contains a letter
    public bool IsOccupied(int lane)
    {
        return word[lane] != null;
    }

    // Return first lane not containing a letter, starting from the left
    public int FindFirstFreePosition(int currentPosition)
    {
        for (int i = 0; i<word.Length; i++)
        {
            if(!IsOccupied(i))
            {
                return i;
            }
        }

        return currentPosition;
    }

    // Return the x coordinate of a given lane
    public int LaneToXCoordinate(int pos)
    {
        return pos - 2;

        // Leaving my old solution here just so I can laugh at myself in the future:
        //
        //      switch(pos)
        //      {
        //          case 0:
        //              return -2;
        //          case 1:
        //              return -1;
        //          case 2:
        //              return 0;
        //          case 3:
        //              return 1;
        //          case 4:
        //              return 2;
        //      }
    }

    // Moves a letter to given position in a a parabolic curve.
    // We achieve this by splitting up the curve into multiple straight lines, and tweening the letter along them. 
    public void moveLetter(Letter l, Vector3 target)
    {
        Vector3 startPoint = l.transform.position;
        int segments = 10;

        Vector3[] waypoints = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float parabolicT = Mathf.Sin(t * Mathf.PI);
            waypoints[i] = Vector3.Lerp(startPoint, target, t) +
                           Vector3.up * parabolicT * 3;
        }

        for (int i = 0; i < segments; i++)
        {
            LeanTween.move(l.gameObject, waypoints[i + 1], 1.5f / segments)
                .setEase(LeanTweenType.linear);
        }
    }
}
