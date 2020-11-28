using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    
    private bool _isGrounded;
    private Vector3 _gravityDirection;
    private float _lastHorizontalInput = 0f;

    [SerializeField] private LayerMask groundLayer;

    public GameObject planet;

    public enum GravityOrientation {Inner = 1, Outer = -1};
    public GravityOrientation gravityOrientation;
    public float maxGravity;

    public float jumpForce;
    public float throwForce;

    public float speed;
    public float maxSpeed;

    public bool jetpackOn;
    public bool autoMoveRight;
    public bool autoMoveLeft;

    public string bossOrPlayer;

    private bool outOfRange = false;

    public int GetGravitySign()
    {
        return (gravityOrientation == GravityOrientation.Inner) ? 1 : -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();   
    }

    // Update is called once per frame
    void Update()
    {
        int direction = GetGravitySign();
        _gravityDirection = direction * (planet.transform.position - transform.position).normalized;
        // Debug.Log("Gravity direction: " + _gravityDirection.ToString());
        
        // Freeze rotation in the right direction
        Vector3 rotation = transform.rotation.eulerAngles;
        // Debug.Log("Rotation: " + rotation);

        
        float angle = -Mathf.Sign(transform.position.x - planet.transform.position.x) * 
                      (Vector3.Angle(new Vector3(0, 1, 0), direction * -_gravityDirection) +
                       ((gravityOrientation == GravityOrientation.Inner) ? 0: 1) * 180);
        // Debug.Log("Angle: " + angle);
        
        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, angle);
        // Debug.Log("Quaternion with Euler: " + transform.rotation);
        

        // Normal jump
        if (!jetpackOn && _isGrounded && Input.GetKeyDown(KeyCode.UpArrow))
        {   
            Vector2 jumpDirection = -_gravityDirection.normalized;
            _rb.AddForce((jumpForce * 350) * jumpDirection, ForceMode2D.Force);
            // Debug.Log("Force added on jump: " + (jumpForce * 100) * jumpDirection);
            gravityOrientation = (gravityOrientation == GravityOrientation.Inner)
                ? GravityOrientation.Outer
                : GravityOrientation.Inner;
            _spriteRenderer.flipX = _spriteRenderer.flipX ? false : true;
        }
        
        // ----- Jetpack jump ----- //
        // If player is grounded, we may want him to jump higher/lower from ground
        if (jetpackOn && _isGrounded && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown("space"))) {
             Vector2 jumpDirection = -_gravityDirection.normalized;
            _rb.AddForce((jumpForce * 100) * jumpDirection, ForceMode2D.Force);
        
        // Player can't jump if he is too high
        } else if (jetpackOn && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown("space")) && !outOfRange) {
            Vector2 jumpDirection = -_gravityDirection.normalized;
            _rb.AddForce((jumpForce * 75) * jumpDirection, ForceMode2D.Force); 
        
        // If up/space key is not pressed anymore, the player must go down
        } else if (jetpackOn && !Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKeyDown("space")) {
            Vector2 jumpDirection = -_gravityDirection.normalized;
            // _rb.AddForce(0 * jumpDirection, ForceMode2D.Force);
            _rb.AddForce(0 * jumpDirection, ForceMode2D.Force);
 
        }

        
        // Orientation
        if (Input.GetAxisRaw("Horizontal") != 0f)
        {
            _spriteRenderer.flipX = Mathf.Sign(GetGravitySign() * Input.GetAxisRaw("Horizontal")) > 0 ? false : true;
        }
        
        // Animation
        if (!autoMoveRight && autoMoveLeft){
            _animator.SetFloat("Speed", 1);
        } else if (autoMoveRight && !autoMoveLeft) {
            _animator.SetFloat("Speed", 1);
        } else {
            _animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));
        } 
        
    }

    private void FixedUpdate()
    {   
        float dist = Vector2.Distance(planet.transform.position, transform.position);
        // Debug.Log("Distance form center: " + dist);

        float outerRadius = GameObject.Find("Planet Top").GetComponent<InnerCircleCollider>().CurrentRadius;
        float outerScale = GameObject.Find("Planet Top").transform.localScale.x;
        float realOuterRadius = outerRadius * outerScale;
        // Debug.Log("Radius of outer circle: " + outerRadius + " and scale: " + outerScale);

        // If player is out of the circle, something may happen
        if (dist > realOuterRadius - 1.5 * outerRadius){
            Debug.Log("Almost out of playing zone, there is something to do");
            outOfRange = true;
        } else {
            outOfRange = false;
        }

        

        _rb.AddForce(_rb.mass * maxGravity * _gravityDirection);
        // Debug.Log("Gravity force: " + _rb.mass * maxGravity * _gravityDirection);
        
        // Horizontal movement controller
        float moveHorizontal;
        // Player control itself with key arrows
        if (!autoMoveRight && autoMoveLeft){
            moveHorizontal = -1;
        } else if (autoMoveRight && !autoMoveLeft) {
            moveHorizontal = 1;
        } else {
            moveHorizontal = Input.GetAxis("Horizontal");
        } 
        //Debug.Log("Move horizontal: " + moveHorizontal);
        
        Vector2 movementDirection = GetGravitySign() * Vector2.Perpendicular(_gravityDirection).normalized;
        // Debug.Log("Movement direction: " + movementDirection);
       

        float movementVelocity = moveHorizontal * (float)2.5 * speed * dist * Time.fixedDeltaTime;
        // Debug.Log("Movement velocity: " + movementVelocity);
        // Debug.Log("Movement velocity dot product: " + Vector2.Dot(_rb.velocity, movementDirection));
        

        _rb.velocity = new Vector2(
            movementVelocity * movementDirection.x,
            movementVelocity * movementDirection.y
        );


        // Check _isGrounded
        _isGrounded = false;
        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                _isGrounded = true;
            }
        }
    }

    public void ThrowProjectile(Transform projectile)
    {   
        if (bossOrPlayer == "player"){
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            rb.velocity = GetGravitySign() * _gravityDirection * throwForce;
        } else if (bossOrPlayer == "boss"){
            // Little animation here ?
        }
        
    }
}
