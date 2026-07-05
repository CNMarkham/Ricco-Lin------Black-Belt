using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float distanceFromCamera = 5f;
    public float hover = 4.72f;

    public float maxResource = 200f;
    public float resource = 200f;

    public float drainRate = 2000f;
    public float regenRate = 50f;

    public Image resourceBar;

    void Update()
    {
        // Mouse movement
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = distanceFromCamera;
        Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Constant forward movement
        transform.position += Vector3.forward * speed * Time.deltaTime;

        // Smooth movement toward mouse
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);

        float newX = Mathf.Lerp(transform.position.x, targetWorldPos.x, t);
        float newZ = Mathf.Lerp(transform.position.z, targetWorldPos.z, t);

        transform.position = new Vector3(newX, transform.position.y, newZ);

        // Flying
        if (Input.GetKey(KeyCode.Space) && resource > 0)
        {
            if (transform.position.y < 20f)
            {
                transform.position += Vector3.up * 50f * Time.deltaTime;
            }

            resource -= drainRate * Time.deltaTime;
        }
        else
        {
            // Fall back to hover height
            if (transform.position.y > hover)
            {
                transform.position -= Vector3.up * 50f * Time.deltaTime;
            }

            // Regenerate resource
            resource += regenRate * Time.deltaTime;
        }

        // Keep resource between 0 and max
        resource = Mathf.Clamp(resource, 0f, maxResource);

        // Update UI
        if (resourceBar != null)
        {
            resourceBar.fillAmount = resource / maxResource;
        }
    }
}
