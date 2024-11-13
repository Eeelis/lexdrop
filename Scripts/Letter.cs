using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Letter : MonoBehaviour
{
    [SerializeField] private TMP_Text characterText;
    private WordManager wordManager;
    private float fallingSpeed;
    private float wordYpos = -625f;
    private int currentLane = 2;
    private bool isFalling = false;

    public void Initialize(WordManager wordManager, float fallingSpeed, string character, Transform ui, float wordYpos)
    {
        this.wordYpos = wordYpos;
        characterText.text = character;
        this.wordManager = wordManager;
        this.fallingSpeed = fallingSpeed;
        isFalling = true;
        transform.SetParent(ui, true);
        transform.localPosition = new Vector3(0f, transform.localPosition.y, transform.localPosition.z);
    }

    public string GetCharacter()
    {
        return characterText.text;
    }

    private void Update()
    {
        if (!isFalling) { return; }

        Fall();

        // Move letter to first empty lane on the left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }

         // Move letter to first empty lane on the right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
        }

        // Place letter in current lane
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PlaceLetter();
        }
    }

    private void Fall()
    {
        if (transform.localPosition.y > wordYpos)
        {
            transform.Translate(Vector3.down * fallingSpeed * Time.deltaTime);
        }
        else 
        {
            PlaceLetter();
        }
    }

    private void MoveLeft()
    {
        if (currentLane <= 0) { return; }

        float moveAmount = 0;
        int originalLane = currentLane;

        do 
        {
            moveAmount += (Screen.width / 5) * 0.9f;
            currentLane -= 1;
        }
        while (currentLane > 0 && wordManager.IsOccupied(currentLane));

        if (!wordManager.IsOccupied(currentLane))
        {
            transform.localPosition = new Vector3(transform.localPosition.x - moveAmount, transform.localPosition.y, transform.localPosition.z);
        }
        else 
        {
            currentLane = originalLane;
        }
    }

    private void MoveRight()
    {
        if (currentLane >= 4) { return; }

        float moveAmount = 0;
        int originalLane = currentLane;

        do 
        {
            moveAmount += (Screen.width / 5) * 0.9f;
            currentLane += 1;
        }
        while (currentLane < 4 && wordManager.IsOccupied(currentLane));

        if (!wordManager.IsOccupied(currentLane))
        {
            transform.localPosition = new Vector3(transform.localPosition.x + moveAmount, transform.localPosition.y, transform.localPosition.z);
        }
        else 
        {
            currentLane = originalLane;
        }
    }

    private void PlaceLetter()
    {
        isFalling = false;
        transform.localPosition = new Vector3(transform.localPosition.x, wordYpos, transform.localPosition.z);
        wordManager.AddLetter(this, currentLane);
    }
}   
