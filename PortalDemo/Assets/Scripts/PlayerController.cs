using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float jumpForce = 4f;

    [Header("MouseLook Settings")]

    private Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2(0.5f, 0.5f);
    public Vector2 smoothing = new Vector2(3, 3);

    [HideInInspector]
    public Vector2 targetDirection = Vector2.zero;

    [HideInInspector]
    public Rigidbody controllerRigidbody;

    private CapsuleCollider controllerCollider;
    public Transform camHolder;
    private float moveSpeedLocal;

    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    private float distanceToGround;

    private Animator weaponHolderAnimator;

    private float inAirTime;

    //Velocity calculation variable
    private Vector3 previousPos = new Vector3();

    Vector3 dirVector;

    private void Start()
    {
        controllerRigidbody = GetComponent<Rigidbody>();
        controllerCollider = GetComponent<CapsuleCollider>();

        distanceToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        targetDirection = camHolder.transform.forward;
    }

    private void Update()
    {
        MouseLook();
        PlayerInput();

        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
    }
    
    private void FixedUpdate()
    {
        CharacterMovement();
    }

    void PlayerInput()
    {
        if (isGrounded())
        {
            if (CheckMovement())
            {
                moveSpeedLocal = moveSpeed;
            } else
            {
                moveSpeedLocal = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot(true);
        } else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Shoot(false);
        }
    }

    /// <summary>
    /// Apply velocity change to player's rigidbody.
    /// </summary>
    void CharacterMovement()
    {
        var camForward = camHolder.transform.forward;
        var camRight = camHolder.transform.right;
        
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * Input.GetAxis("Vertical") + camRight * Input.GetAxis("Horizontal");
        controllerRigidbody.velocity = moveDirection.normalized * moveSpeedLocal + Vector3.up * controllerRigidbody.velocity.y;
    }
    /// <summary>
    /// Movement check.
    /// </summary>
    /// <returns>False for no velocity.</returns>
    bool CheckMovement()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < Mathf.Epsilon && Mathf.Abs(Input.GetAxis("Vertical")) < Mathf.Epsilon)
        {
            return false;
        } else
        {
            return true;
        }
    }

    void MouseLook()
    {
        Quaternion targetOrientation = Quaternion.Euler(targetDirection);
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        _mouseAbsolute += _smoothMouse;

        if (clampInDegrees.x < 360)
            _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
        camHolder.transform.localRotation = xRotation;

        if (clampInDegrees.y < 360)
            _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, camHolder.transform.InverseTransformDirection(Vector3.up));
        camHolder.transform.localRotation *= yRotation;
        camHolder.transform.rotation *= targetOrientation;
    }

    /// <summary>
    /// Open alpha or beta portal if shoots on specific surface.
    /// </summary>
    /// <param name="isAlpha">True for left mouse button.</param>
    void Shoot(bool isAlpha)
    {

    }

    void Jump()
    {
        if (isGrounded())
            controllerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distanceToGround + 0.1f);
    }
    
}
