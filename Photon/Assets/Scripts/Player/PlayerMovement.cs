using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float maxVelocityChange = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Vector2 movementInput;
    private float currentSpeed;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent tipping over
    }

    private void Update()
    {
        // Get movement input
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Run / Walk
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Check ground state
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(CalculateMovement(currentSpeed), ForceMode.VelocityChange);
    }

    private Vector3 CalculateMovement(float speed)
    {
        Vector3 targetVelocity = new Vector3(movementInput.x, 0, movementInput.y);
        targetVelocity = transform.TransformDirection(targetVelocity) * speed;

        Vector3 velocity = rb.linearVelocity;

        if (movementInput.magnitude > 0.1f)
        {
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            return velocityChange;
        }

        return Vector3.zero;
    }

    private void Jump()
    {
        // Reset vertical velocity before jumping (prevents stacking forces)
        Vector3 v = rb.linearVelocity;
        v.y = 0;
        rb.linearVelocity = v;

        // Apply instant upward force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize ground check ray
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
#endif
}
