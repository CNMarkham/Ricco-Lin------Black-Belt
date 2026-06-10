using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float distanceFromCamera = 5;
    public float hover = 4.72f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mousePos = Input.mousePosition;
        Debug.Log($"Mouse position is {Input.mousePosition}");
        mousePos.z = distanceFromCamera;
        Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        transform.position += Vector3.forward * speed * Time.deltaTime;

        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);


        float newX = Mathf.Lerp(transform.position.x, targetWorldPos.x, t);
        float newZ = Mathf.Lerp(transform.position.z, targetWorldPos.z, t);

        transform.position = new Vector3(newX, transform.position.y, newZ);

        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += new Vector3(0, Time.deltaTime * 50, 0);
            if(transform.position.y >= 20)
            {
                transform.position -= new Vector3(0, Time.deltaTime * 50, 0);
            }
        }
        else if (transform.position.y >= hover)
        {
            transform.position -= new Vector3(0, hover * Time.deltaTime * 10, 0);
        }
    }
}
