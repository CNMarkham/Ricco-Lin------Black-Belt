using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float distanceFromCamera = 5;
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
        /*Vector3 smoothPos = Vector3.Lerp(
            transform.position,
            new Vector3(targetWorldPos.x, transform.position.y, targetWorldPos.z),
            speed * Time.deltaTime
            );

        transform.position = smoothPos;*/
    }
}
