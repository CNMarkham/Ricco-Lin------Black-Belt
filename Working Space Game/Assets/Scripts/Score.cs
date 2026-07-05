using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Remove this line if using the regular UI Text

public class ScoreManager : MonoBehaviour
{
    public Dead rest;
    public TMP_Text scoreText; // Change to Text if using the legacy UI Text
    public int score = 0;

    private float timer = 0f;

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= 0f)
        {
            score++;
            scoreText.text = "Score: " + score;
            timer = 0f;
        }
    }

    public void Reset()
    {
        score = 0;
        scoreText.text = "Score: " + score;
        timer = 0f;
    }

}