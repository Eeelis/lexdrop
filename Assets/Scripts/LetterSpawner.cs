using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LetterSpawner : MonoBehaviour
{
    public static LetterSpawner letterSpawner;

    [SerializeField] private GameObject letter;

    private GameObject newLetter;
    private char[] allLetters = "abcdefghijklmnopqrsty".ToCharArray();
    private Vector3 spawnLocation = new Vector3(0, 4, 0);
    

    void Awake()
    {
        letterSpawner = this;
    }

    public void SpawnNewLetter(string character = "")
    {
        newLetter = Instantiate(letter, spawnLocation, Quaternion.identity);

        if (character != "")
        {
            newLetter.GetComponent<TextMeshPro>().text = character; 
        }
        else 
        {
            newLetter.GetComponent<TextMeshPro>().text = GenerateRandomLetter();
        }
        
    }

    public string GenerateRandomLetter()
    {
        int randomIndex = Random.Range(0, allLetters.Length);
        return allLetters[randomIndex].ToString();
    }

    public void DestroyCurrentLetter()
    {
        if (newLetter)
        {
            Destroy(newLetter);
        }
    }

}
