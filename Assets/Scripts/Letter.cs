using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Letter : MonoBehaviour
{
    private string letter;
    private float wordYpos = -3.3f;
    private float bottomPosition;
    private int currentLane = 2;
    private bool isFalling;
    

    private void Start()
    {
        letter = GetComponent<TextMeshPro>().text;
        isFalling = true;
    }

    private void Update()
    {
        if (!isFalling) { return; }

        // Move letter down until it hits the bottom
        if (transform.position.y > wordYpos)
        {
            transform.Translate(Vector3.down * GameManager.gameManager.Speed * Time.deltaTime);
        }
        else 
        {
            WordManager.wordManager.AddLetter(this, currentLane);
            isFalling = false;
        }

        // Move letter to first empty lane on the left
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0)
        {
            int moveAmount = 0;
            int originalLane = currentLane;

            do 
            {
                moveAmount += 1;
                currentLane -= 1;
            }
            while (currentLane > 0 && WordManager.wordManager.IsOccupied(currentLane));

            if (!WordManager.wordManager.IsOccupied(currentLane))
            {
                transform.position = new Vector3(transform.position.x - moveAmount, transform.position.y, transform.position.z);
            }
            else 
            {
                currentLane = originalLane;
            }
        }

         // Move letter to first empty lane on the right
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 4)
        {
            int moveAmount = 0;
            int originalLane = currentLane;

            do 
            {
                moveAmount += 1;
                currentLane += 1;
            }
            while (currentLane < 4 && WordManager.wordManager.IsOccupied(currentLane));

            if (!WordManager.wordManager.IsOccupied(currentLane))
            {
                transform.position = new Vector3(transform.position.x + moveAmount, transform.position.y, transform.position.z);
            }
            else 
            {
                currentLane = originalLane;
            }
        }

        // Place letter in current lane
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.position = new Vector3(transform.position.x, wordYpos, transform.position.z);
        }
    }
}   
