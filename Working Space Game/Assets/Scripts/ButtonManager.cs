using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button[] buttons = new Button[6];
    public GameObject starter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DisplayButtons();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
    }

    public void DisplayButtons()
    {
        if (starter.activeInHierarchy == false)
        {

            buttons[0].gameObject.SetActive(true);
            if (PlayerPrefs.GetInt("level" + 1, 0) > 75)
            {
                buttons[1].gameObject.SetActive(true);
            }
            if (PlayerPrefs.GetInt("level" + 2, 0) > 150)
            {
                buttons[2].gameObject.SetActive(true);
            }

            if (PlayerPrefs.GetInt("level" + 3, 0) > 200)
            {
                buttons[3].gameObject.SetActive(false);
            }

            if (PlayerPrefs.GetInt("level" + 4, 0) > 250)
            {
                buttons[4].gameObject.SetActive(false);
            }

            if (PlayerPrefs.GetInt("level" + 5, 0) > 300)
            {
                buttons[5].gameObject.SetActive(false);
            }

            if (PlayerPrefs.GetInt("level" + 6, 0) > 400)
            {
                buttons[6].gameObject.SetActive(false);
            }

        }
    }
}
