using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [Space(10)]
    
    public float MoveSpeed = 50f;
    public float RotateSpeed = 100f;
    
    [Space(10)]
    
    public bool UseEdgeScrolling = false;
    public int EdgeScrollSize = 20;
    
    private void Update()
    {
        // Movement
        Vector3 inputDirection = new Vector3(0, 0, 0);
        
        if (Input.GetKey(KeyCode.W)) inputDirection.z = +1f;
        if (Input.GetKey(KeyCode.S)) inputDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) inputDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDirection.x = +1f;

        if (Input.mousePosition.x < EdgeScrollSize) inputDirection.x = -0.5f;
        if (Input.mousePosition.y < EdgeScrollSize) inputDirection.z = -0.5f;
        if (Input.mousePosition.x > Screen.width - EdgeScrollSize) inputDirection.x = +0.5f;
        if (Input.mousePosition.y > Screen.height - EdgeScrollSize) inputDirection.z = +0.5f;
        
        Vector3 moveDirection = transform.forward * inputDirection.z + transform.right * inputDirection.x;
        transform.position += moveDirection * (MoveSpeed * Time.deltaTime);
        
        // Rotation
        float rotateDirection = 0f;
        
        if (Input.GetKey(KeyCode.Q)) rotateDirection = +1f;
        if (Input.GetKey(KeyCode.E)) rotateDirection = -1f;
        
        transform.eulerAngles += new Vector3(0, rotateDirection * RotateSpeed * Time.deltaTime, 0);
    }
}
