using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 20f;
    public float distanceFromCamera = 5f;
    public float hover = 4.72f;

    public float maxResource = 200f;
    public float resource = 200f;

    public float drainRate = 2000f;
    public float regenRate = 50f;

    public Image resourceBar;

    // Ground check
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

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
        float newZ = Mathf.Lerp(transform.position.z, targetWorldPos.z + 20, t);

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
        }

        // Clamp resource
        resource = Mathf.Clamp(resource, 0f, maxResource);



        // Check if player is touching the ground
        bool onGround = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );

        // Only regenerate resource when on the ground
        if (onGround)
        {
            Debug.Log("hit g");
            resource += regenRate * Time.deltaTime;
        }

        // Update UI
        if (resourceBar != null)
        {
            resourceBar.fillAmount = resource / maxResource;
        }
    }

    // Draw the ray in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
