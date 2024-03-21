using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Letter : MonoBehaviour
{
    private WordManager wordManager;
    private float fallingSpeed;
    private float wordYpos = -3.3f;
    private int currentLane = 2;
    private bool isFalling = false;
    
    public void Initialize(WordManager wordManager, float fallingSpeed, string character)
    {
        GetComponent<TextMeshPro>().text = character;
        this.wordManager = wordManager;
        this.fallingSpeed = fallingSpeed;
        isFalling = true;
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
        if (transform.position.y > wordYpos)
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

        int moveAmount = 0;
        int originalLane = currentLane;

        do 
        {
            moveAmount += 1;
            currentLane -= 1;
        }
        while (currentLane > 0 && wordManager.IsOccupied(currentLane));

        if (!wordManager.IsOccupied(currentLane))
        {
            transform.position = new Vector3(transform.position.x - moveAmount, transform.position.y, transform.position.z);
        }
        else 
        {
            currentLane = originalLane;
        }
    }

    private void MoveRight()
    {
        if (currentLane >= 4) { return; }

        int moveAmount = 0;
        int originalLane = currentLane;

        do 
        {
            moveAmount += 1;
            currentLane += 1;
        }
        while (currentLane < 4 && wordManager.IsOccupied(currentLane));

        if (!wordManager.IsOccupied(currentLane))
        {
            transform.position = new Vector3(transform.position.x + moveAmount, transform.position.y, transform.position.z);
        }
        else 
        {
            currentLane = originalLane;
        }
    }

    private void PlaceLetter()
    {
        isFalling = false;
        transform.position = new Vector3(transform.position.x, wordYpos, transform.position.z);
        wordManager.AddLetter(this, currentLane);
    }
}   
