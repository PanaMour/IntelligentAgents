using UnityEngine;

public class LockCameraPosition : MonoBehaviour
{
    public Vector3 lockedRotation = new Vector3(90f, 0f, 0f);

    private void Start()
    {
        lockedRotation = new Vector3(90f, 0f, 0f);
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(lockedRotation);
    }
}
