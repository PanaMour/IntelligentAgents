using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public float cameraSpeed = 1f;
    [SerializeField] private GameObject ground;
    public float initialCameraHeight = 30f;
    public float maxZoom = 10f;
    public float minZoom = 60f;

    private void Start()
    {
        // Set initial camera position and field of view
        Camera.main.fieldOfView = minZoom;
    }

    private void Update()
    {
        if (ground != null)
        {
            if (Camera.main != GetComponent<Camera>())
            {
                return;
            }
            // Move camera horizontally with arrow keys or WASD
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0f, vertical) * cameraSpeed * Time.deltaTime;
            transform.position += movement;

            // Zoom in/out with mouse wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            float newZoom = Mathf.Clamp(Camera.main.fieldOfView + scroll * -10f, maxZoom, minZoom);
            Camera.main.fieldOfView = newZoom;

            // Set camera position to top-down view centered on the ground
            transform.position = new Vector3(ground.transform.position.x, initialCameraHeight, ground.transform.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            ground = GameObject.Find("Ground");
        }
    }
}
