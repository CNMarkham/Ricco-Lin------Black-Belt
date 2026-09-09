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

    public void DisplayButtons()
    {
        if (starter.activeInHierarchy == false)
        {
            for (int i = 0; i < buttons.Length; i++)
            { 
                if (i == 0)
                {
                    buttons[i].gameObject.SetActive(true);
                }
                else if (PlayerPrefs.GetInt("level" + i, 0) > 0)
                {
                    buttons[i].gameObject.SetActive(true);
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
