using UnityEngine;

public class NonLocalPlayerAnimation : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private float maxSpeed = 8f;
    
    private void LateUpdate()
    {
        // Get velocity on X and Z axes (ignore Y for ground movement)
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        // Calculate magnitude for overall speed
        float speed = horizontalVelocity.magnitude;
        
        // Debug log both axes
        Debug.Log($"X Speed: {rb.linearVelocity.x:F2}, Z Speed: {rb.linearVelocity.z:F2}, Total Speed: {speed:F2}");
        
        // Clamp the speed
        speed = Mathf.Clamp(speed, 0, maxSpeed);
        
        animator.SetFloat("Speed", speed);
    }
}