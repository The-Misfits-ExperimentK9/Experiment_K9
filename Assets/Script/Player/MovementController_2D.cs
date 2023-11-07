using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MovementController_2D : MonoBehaviour {
   // [SerializeField] PlayerBehaviour playerController;
    //[SerializeField] PlayerDimensionController playerDimensionController;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Rigidbody playerRigidBody3D;
    [SerializeField] private float offSetAmount = 5.5f;
    [SerializeField] float wallCheckDistance = 0.8f;
    private Collider dogCollider2D;

    public bool Is2DPlayerActive = false;

    private KeyControl jumpKey1, jumpKey2;

    public bool CanMove = true;

    private Vector3 gizmoDrawLoc;
    public WallBehaviour currentWall;
    Vector3 forward;                                    //used to check which wall object is in the foreground to use that as the movement override
   
    public enum ProjectionState {
        OutOfRange,
        HoldingObject,
        In2D,
        In2DHoldingObject
    }
    private ProjectionState projectionState;
    public float moveSpeed2D = 15.0f;
    public float jumpPower2D = 20f;
    [SerializeField] private List<Sprite> sprites = new();

    private Vector3 newSpritePos;
   
    bool gravityEnabled = false;



    public float GroundedRadius = 0.2f;
    public float GroundedOffset = new();
    public LayerMask GroundLayers = new();


    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float movementForceMultiplier = 20f;
    //player movement
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _speedHorizontal;
    private float _speedVertical;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 10.25f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -60.0f;

    public float Friction = 10f;

    // Start is called before the first frame update
    void Awake() {
        // dog2DSprite = GetComponent<SpriteRenderer>();
        dogCollider2D = GetComponent<Collider>();
        jumpKey1 = Keyboard.current.spaceKey;
        jumpKey2 = Keyboard.current.wKey;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!PlayerBehaviour.Instance.IsIn3D()) {
            ApplyFriction();
            if (gravityEnabled) {
                GroundedCheck();
                JumpAndGravity();

            }
            if (CanMove)
                Move();
            //Move2D();
            if (currentWall.AllowsDimensionTransition && !PlayerBehaviour.Instance.playerDimensionController.DOGEnabled) {
                PlayerBehaviour.Instance.playerDimensionController.TransitionTo3D();
            }
        }
        else {
            //HandleWallCollision
        }
    }
    //handles player movement in 2D
    //void Move2D() {
    //    var input = GetInput();
    //    var up = transform.up;
    //    var left = -transform.right;
    //    Vector3 direction;
    //    if (!gravityEnabled)
    //        direction = up * input.y + left * input.x;
    //    else {
    //        direction = left * input.x;
    //        if (jumpKey2.wasPressedThisFrame) {
    //            Jump2D();
    //        }

    //    }
    //    rb.velocity = direction * moveSpeed2D;
    //    // Flip the sprite when the dog moves the other way
    //    if (input.x < 0) {
    //        spriteRenderer.flipX = true;
    //    }
    //    else if (input.x > 0) {
    //        spriteRenderer.flipX = false;
    //    }
    //}
    private void ApplyFriction() {
        Vector3 frictionDirection;

        if (gravityEnabled) {
            frictionDirection = new(-rb.velocity.x, 0f, -rb.velocity.z);
        }
        else {
            frictionDirection = new(-rb.velocity.x, -rb.velocity.y, -rb.velocity.z);
        }
        rb.AddForce(frictionDirection * Friction);
    }
    private void GroundedCheck() {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        var hits = Physics.OverlapSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        if (hits.Length > 0) {
            if (hits.Length == 1) {
                if (hits[0] == currentWall) {
                    Grounded = false;
                    return;
                }
            }
            Grounded = true;
        }
        else {
            Grounded = false;
        }

    }
    private void OnDrawGizmosSelected() {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void Move() {
        float targetSpeed = moveSpeed2D;

        var input = GetInput();

        if (input.x < 0) {
            spriteRenderer.flipX = true;
        }
        else if (input.x > 0) {
            spriteRenderer.flipX = false;
        }
        if (input.x < .01f && input.x > -.01f) targetSpeed = 0.0f;
        // accelerate or decelerate to target speed

        _speedHorizontal = targetSpeed;

        var left = -transform.right;
        Vector3 directionX = left * input.x;

        if (!gravityEnabled) {
            if (input.y < .01f && input.y > -.01f)
                targetSpeed = 0.0f;
            else {
                targetSpeed = moveSpeed2D;
            }
            _speedVertical = targetSpeed;


            var up = transform.up;
            Vector3 directionY = up * input.y;
            // move the player in x and y direction

            rb.AddForce((directionX * _speedHorizontal +
                        directionY * _speedVertical) * movementForceMultiplier);

            //clamp overal velocity at moveSpeed2D
            if (rb.velocity.magnitude > moveSpeed2D) {
                rb.velocity = rb.velocity.normalized * moveSpeed2D;
            }


        }
        else {
            //add horizontal force
            rb.AddForce(directionX * (_speedHorizontal * movementForceMultiplier));

            //clamp horizontal velocity at moveSpeed2D
            if (Mathf.Abs(rb.velocity.x) > moveSpeed2D) {
                rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -moveSpeed2D, moveSpeed2D), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -moveSpeed2D, moveSpeed2D));
            }


            rb.velocity = new Vector3(rb.velocity.x, _verticalVelocity, rb.velocity.x); //apply gravity


        }

    }

    private void JumpAndGravity() {

        if (Grounded) {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character


            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f) {
                _verticalVelocity = -2f;
            }

            // Jump
            if (jumpKey2.wasPressedThisFrame && _jumpTimeoutDelta <= 0.0f) {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character

            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f) {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f) {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else {
                // update animator if using character

            }


        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity) {
            _verticalVelocity += Gravity * .02f;//fixed delta time
        }

    }
    //handles player movement in 2D
   
   
    public Vector2 GetInput() {
        var keyboard = Keyboard.current;
        return new Vector2(keyboard.dKey.isPressed ? 1 : keyboard.aKey.isPressed ? -1 : 0,
                           keyboard.wKey.isPressed ? 1 : keyboard.sKey.isPressed ? -1 : 0);
    }

    //locks the axes to the up/down/left/right on the wall
    //should prevent the dog from slipping into the or out of the wall
    public void ProcessAxisChange() {
        var right = transform.right;

        //crazy floating point errors
        if (right.x > 0.0001 || right.x < -0.0001) {
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
        else if (right.y > 0.0001 || right.y < -0.0001) {
            Debug.LogError("Unsupported behaviour - doggo on floor");
        }
        else if (right.z > 0.0001 || right.z < -0.0001) {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
        }
    }
    private void OnCollisionEnter(Collision collision) {

        if (collision.gameObject.TryGetComponent(out WallBehaviour wallB)) {

            //if (wallB.IsWalkThroughEnabled) {

            HandleWallCollision(collision.collider, wallB);
            //}
        }
    }
    private void OnCollisionExit(Collision collision) {

        if (collision.gameObject.TryGetComponent(out WallBehaviour wallB)) {

            if (currentWall == wallB && !PlayerBehaviour.Instance.IsIn3D() || currentWall == null && !PlayerBehaviour.Instance.IsIn3D()) {
                //  Debug.Log("leaving wall");
                //currentWall = null;
                //print("test");
                //playerController.ChangeDimension();

               // PlayerBehaviour.Instance.playerDimensionController.TransitionTo3D();
                UpdateWallStatus();

            }
            else if (PlayerBehaviour.Instance.IsIn3D()) {
                //currentWall = null;
            }
        }
    }

    private void UpdateWallStatus() {
        if (CheckIfInCurrentWall()) {
            //do nothing still in the wall
        }
        else {
            Debug.Log("leaving wall");
            PlayerBehaviour.Instance.playerDimensionController.TransitionTo3D();
        }
    }
    bool CheckIfInCurrentWall() {
        if (currentWall == null) {

            return false;
        }

        if (Physics.Raycast(transform.position, -transform.forward, 
            out var hit, wallCheckDistance, LayerMask.GetMask("Walls"))) {
            if (hit.collider.gameObject == currentWall.gameObject) {
                return true;
            }
            else {

                return false;
            }
        }
        return false;
    }

    private void HandleWallCollision(Collider collider, WallBehaviour wallB) {
        var closestPoint = collider.ClosestPointOnBounds(transform.position);

        if (wallB.IsWalkThroughEnabled) {
            WallBehaviour pastwall = currentWall;
            UpdateCurrentWall(wallB);
            if (pastwall == null || wallB.transform.up != pastwall.transform.up) {
                //print("its on wall5");
                UpdateCurrentWall(wallB);
                TransitionToNewAxis(collider.ClosestPointOnBounds(transform.position), wallB);

            }
        }

    }
    public bool IsProjectionSpaceClear(Vector3 position) {
        if (dogCollider2D == null) { dogCollider2D = GetComponent<Collider>(); }

        var boxHits = Physics.OverlapBox(position, dogCollider2D.bounds.extents, Quaternion.identity, LayerMask.GetMask("Walls"));

        if (boxHits.Length == 0) return true;



        foreach (var hit in boxHits) {
            if (hit.TryGetComponent(out WallBehaviour wallB)) {
                if (!wallB.IsWalkThroughEnabled)
                    return false;
            }
            else {
                //something that wasnt a wall is blocking 
                return false;
            }
        }

        return true;
    }

    //handles transitioning to anew axis when encountering another wall at a 90 degree angle
    void TransitionToNewAxis(Vector3 pos, WallBehaviour wall) {

        //rotate first to get correct transform.right
        transform.forward = wall.transform.up;

        ProcessAxisChange();
        UpdateCurrentWall(wall);


        //only supports changing x/z plane not y (ceiling/floor)
        var offsetDirection = GetDirection(wall) == 1 ? -transform.right : transform.right;
        newSpritePos = pos + offsetDirection * offSetAmount;
        newSpritePos += transform.forward * PlayerDimensionController.WALL_DRAW_OFFSET;

        //move to offset position
        transform.position = newSpritePos;
    }
    void UpdateCurrentWall(WallBehaviour wall) {
        currentWall = wall;
        if (currentWall is GravityWall) {
            EnableGravity();
        }
        else {
            DisableGravity();
        }
    }

    public int GetDirection(WallBehaviour other) {
        Vector3 toOther = other.transform.position - transform.position;

        float dotUp = Vector3.Dot(toOther.normalized, transform.up);
        float dotRight = Vector3.Dot(toOther.normalized, transform.right);

        //checks +/- 45 degrees to check if object is more above or below than left or right
        if (dotUp > 0.7071) return 0; // Above
        if (dotUp < -0.7071) return 2; // Below

        if (dotRight > 0) return 3; // Right
        if (dotRight < 0) return 1; // Left

        return -1; // Error or the object is too close
    }
    public void SetProjectionState(ProjectionState state) {
        projectionState = state;
        switch (projectionState) {
            case ProjectionState.OutOfRange:
                spriteRenderer.sprite = sprites[1];
                Is2DPlayerActive = false;
                break;
            case ProjectionState.HoldingObject:
                spriteRenderer.sprite = sprites[2];
                Is2DPlayerActive = false;
                break;
            case ProjectionState.In2D:
                spriteRenderer.sprite = sprites[3];
                Is2DPlayerActive = true;
                break;
            case ProjectionState.In2DHoldingObject:
                spriteRenderer.sprite = sprites[4];
                Is2DPlayerActive = true;
                break;

        }
    }
    public bool CanTransitionOutOfCurrentWall() {
        return currentWall.AllowsDimensionTransition;
    }
    public bool IsFlipped() {
        return spriteRenderer.flipX;
    }
    public void EnableGravity() {
        rb.useGravity = true;
        gravityEnabled = true;
    }
    public void DisableGravity() {
        rb.useGravity = false;
        gravityEnabled = false;
    }


}
