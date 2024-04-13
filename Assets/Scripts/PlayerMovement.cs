using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerState
{
    IDLE,
    RUNNING,
    FALLING,
    JUMPING,
    DANGLING,
    DASHING,
    HOOKING,
    HOOKED
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float velocity_h = 5.0f;
    public float maxVerticalVelocity = 100.0f;
    public float gravity  = 9.81f;
    public float jumpForce = 15.0f;
    public float jumpingGravityMultiplier = 2.0f;
    public float releaseJumpGravityMultiplier = 1.5f;
    public float dashVelocityX = 30f;
    public float dashVelocityY = 80f;
    public Vector2 velocity;
    public AnimationCurve curve;
    public float ShootTime = 2.0f;
    public float ExtractTime = 2.0f;
    [FormerlySerializedAs("hoolCollided")] public bool hookCollided = false;
    
    private Rigidbody2D rigidBody;
    private bool jumpPressed = false;
    private float gravityMultiplier = 1.0f;
    private bool prematureRelease = false;
    private bool hasDashed = false;
    private float input = 0.0f;
    private bool hookingCanceled = false;
    private Vector3 hookVelocity;
    private Vector3 handHooking = new Vector3(0.07f, -0.25f, 0.0f);
    private Vector3 handHooked = new Vector3(1.38f, 0.86f, 0.0f);
    Vector3 mousePos;

    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject playerSprite;
    [SerializeField] private GameObject hook;
    [SerializeField] private SpriteRenderer renderer;
    
    public PlayerState State { private set; get; }
    
    public delegate void onStateChange(PlayerState newState);
    public static event onStateChange OnStateChange;
    
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Debug.Log("A/D to move");
        Debug.Log("Space to jump");
        Debug.Log("Ctrl to dangle");
        Debug.Log("Shift to dash");
        ChangePlayerState(PlayerState.FALLING);
        
    }
    
    public void ChangePlayerState(PlayerState newState)
    {
        if (State == newState)
            return;
        switch (State)
        {
            case PlayerState.JUMPING:
            {
                InitialJumpState();
                break;
            }
            case PlayerState.DANGLING:
            {
                if (velocity.y > 0.0f)
                {
                    velocity.y += 5.0f;
                }
                break;
            }
            case PlayerState.DASHING:
            {
                renderer.color = Color.white;
                break;
            }
            case PlayerState.HOOKING:
            {
                if (hookingCanceled)
                {
                    line.SetPosition(1, line.GetPosition(0));
                    hook.SetActive(false);
                }
                hookingCanceled = false;
                hookCollided = false;
                break;
            }
            case PlayerState.HOOKED:
            {
                hook.SetActive(false);
                break;
            }
        }
 
        State = newState;
        switch (State)
        {
            case PlayerState.IDLE:
            case PlayerState.RUNNING:
            {
                hasDashed = false;
                velocity.y = 0.0f;
                break;
            }
            case PlayerState.DANGLING:
            {
                hasDashed = false;
                velocity = Vector2.zero;
                break;
            }
            case PlayerState.DASHING:
            {
                renderer.color = Color.red;
                break;
            }
            case PlayerState.HOOKING:
            {
                InitializeHooking();
                break;
            }
            case PlayerState.HOOKED:
            {
                line.SetPosition(0, handHooked);
                break;
            }
        }
        OnStateChange?.Invoke(State);
    }

    void InitializeHooking()
    {
        line.SetPosition(0, handHooking);
        line.SetPosition(1, handHooking);
        velocity = Vector2.zero;
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        mousePos.z = 0.0f;
        Vector2 offset = mousePos - handHooking;
        hook.SetActive(true);
        hook.transform.eulerAngles = new Vector3(0.0f, 0.0f, -Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg);
                
        if (offset.x < 0)
            renderer.flipX = true;
        else 
            renderer.flipX = false;
    }

    private void Update()
    {
        switch (State)
        {
            case PlayerState.RUNNING:
            case PlayerState.IDLE:
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jumpPressed = true;
                    Jump();
                }
                if (Input.GetMouseButtonDown(0))
                {
                    ChangePlayerState(PlayerState.HOOKING);   
                }
                Dash();
                break;
            }
            case PlayerState.FALLING:
            case PlayerState.JUMPING:
                Dash();
                break;
            case PlayerState.DANGLING:
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (renderer.flipX)
                    {
                        if (!Input.GetKey(KeyCode.A))
                            return;
                    }
                    else if (!Input.GetKey(KeyCode.A))
                        return;
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        jumpPressed = true;
                        Jump();
                    }
                }
                else
                {
                    ChangePlayerState(PlayerState.FALLING);
                }
                break;
            }
            case PlayerState.HOOKING:
                Hooking();
                break;
            case PlayerState.HOOKED:
                Hooked();
                break;
        }
    }
    

    void FixedUpdate()
    {
        switch (State)
        {
            case PlayerState.RUNNING:
            case PlayerState.IDLE:
            {
                HorizontalMovement();
                break;
            }
            case PlayerState.FALLING:
            {
                HorizontalMovement();
                ApplyGravity();
                break;
            }
            case PlayerState.JUMPING:
            {
                HorizontalMovement();
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    prematureRelease = true;
                }

                ApplyGravity();
                break;
            }
            case PlayerState.DASHING:
            {
                ApplyGravity();
                break;
            }
        }
        velocity.y = Math.Clamp(velocity.y, -maxVerticalVelocity, maxVerticalVelocity);
        rigidBody.velocity = velocity;
    }

    void ApplyGravity()
    {
        if (State == PlayerState.JUMPING && velocity.y <= 0.0f)
            gravityMultiplier = jumpingGravityMultiplier;
        else if (State == PlayerState.JUMPING && prematureRelease)
            gravityMultiplier = releaseJumpGravityMultiplier;
        else
            gravityMultiplier = 1.0f;
        velocity.y -= gravity * gravityMultiplier * Time.fixedDeltaTime;
    }

    void Hooking()
    {
        if (hookingCanceled)
        {
            line.SetPosition(1, Vector3.SmoothDamp(line.GetPosition(1), 
                line.GetPosition(0), ref hookVelocity, ShootTime));
            hook.transform.localPosition = line.GetPosition(1);
            if (hookVelocity.sqrMagnitude < 0.05f)
            {
                ChangePlayerState(PlayerState.IDLE);
            }
            return;
        }
        if (Input.GetMouseButton(0))
        {
            line.SetPosition(1, Vector3.SmoothDamp(line.GetPosition(1), 
                mousePos, ref hookVelocity, ShootTime)); 
            
            hook.transform.localPosition = line.GetPosition(1);
            if (hookVelocity.sqrMagnitude < 0.05f || hookCollided)
            {
                if (!hookCollided)
                    hookingCanceled = true;
                else
                {
                    ChangePlayerState(PlayerState.HOOKED);
                    return;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            hookingCanceled = true;
        }
    }

    void Hooked()
    {
        playerSprite.transform.localPosition = Vector3.SmoothDamp(playerSprite.transform.localPosition,
            line.GetPosition(1), ref hookVelocity, ExtractTime);
        line.SetPosition(0, Vector3.SmoothDamp(line.GetPosition(0), 
            line.GetPosition(1), ref hookVelocity, ExtractTime));
        
        if (hookVelocity.sqrMagnitude < 0.05f)
        {
            line.SetPosition(1, line.GetPosition(0));
            var pos = playerSprite.transform.localToWorldMatrix.GetPosition();
            playerSprite.transform.localPosition = Vector3.zero;
            hook.transform.localPosition = new Vector3(-0.918f, -0.837f, 0.0f);
            transform.position = pos;
            ChangePlayerState(PlayerState.FALLING);
        }
    }
    
    void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (hasDashed)
                return;
            hasDashed = true;
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (input == Vector2.zero)
                return;
            velocity.x += input.x * dashVelocityX;
            velocity.y += input.y * dashVelocityY;
            ChangePlayerState(PlayerState.DASHING);
            Invoke("OnDashDone", 0.4f);
        }
    }

    void OnDashDone()
    {
        if (State != PlayerState.DASHING)
            return;
        ChangePlayerState(PlayerState.FALLING);
    }

    void Jump()
    {
        ChangePlayerState(PlayerState.JUMPING);
        velocity.y += jumpForce;
    }

    void HorizontalMovement()
    {
        input = curve.Evaluate(Input.GetAxisRaw("Horizontal"));

        if (input < 0.0f)
        {
            renderer.flipX = true;
        }
        else
            renderer.flipX = false;
        if (input != 0.0f)
        {
            if (State == PlayerState.IDLE)
            {
                ChangePlayerState(PlayerState.RUNNING);
            }
        }
        else
        {
            if(State == PlayerState.RUNNING)
                ChangePlayerState(PlayerState.IDLE);
        }
        
        velocity.x = velocity_h * input;
    }
    
    void ClimbingMovement()
    {
        float input = 0.0f;
        if (Input.GetKey(KeyCode.W))
        {
            input = 1.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            input = -1.0f;
        }
        if(Input.GetKey(KeyCode.A))
        {
            renderer.flipX = true;
        }
        if(Input.GetKey(KeyCode.D))
        {
            renderer.flipX = false;
        }
        velocity.y = velocity_h * input;
    }
    
    public void InitialJumpState()
    {
        prematureRelease = false;
        jumpPressed = false;
    }
}
