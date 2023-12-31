using Cinemachine;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MovementController_2D : MonoBehaviour {
    #region Variables

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Rigidbody playerRigidBody3D;
    private Collider dogCollider2D;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -60.0f;
    public float Friction = 10f;
    [Space(10)]
    [Header("Wall Collision")]
    [SerializeField] private float offSetAmount = 5.5f;
    [SerializeField] float wallCheckDistance = 0.8f;
    [SerializeField] private WallBehaviour currentWall;
    private Collider currentWallCollider;
    [Space(10)]
    [Header("2D Camera")]
    [SerializeField] private GameObject Camera2dLookAt;
    [SerializeField] private GameObject CinemachineFollowTarget;
    [SerializeField] private CinemachineVirtualCamera virtualCamera2D;
    [SerializeField] private CameraWallConfiner cameraWallConfiner;


    [Space(10)]
    [Header("Settings")]
    public bool Is2DPlayerActive = false;
    public bool CanMove = true;
    bool gravityEnabled = false;
    public float maxSpeed2D = 15.0f;
    [SerializeField] private float movementForceMultiplier = 20f;

    [Tooltip("The height the player can jump")]
    public float JumpHeight = 10.25f;

    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    private KeyControl jumpKey;

    private Vector3 gizmoDrawLoc, gizmoDrawLoc2;

    public enum ProjectionState {
        OutOfRange,
        HoldingObject,
        In2D,
        In2DHoldingObject
    }
    [SerializeField] private ProjectionState projectionState;

    [SerializeField] private List<Sprite> sprites = new();

    private Vector3 newSpritePos;

    [Header("Ground checks")]
    public float GroundedRadius = 0.2f;
    public float GroundedOffset = new();
    public LayerMask GroundLayers = new();
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Space(10)]
    [SerializeField] private SpriteRenderer spriteRenderer;

    //player movement
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _speedHorizontal;
    private float _speedVertical;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;


    #endregion

    #region Start and Update
    // Start is called before the first frame update
    void Awake() {
        dogCollider2D = GetComponent<Collider>();
        jumpKey = Keyboard.current.wKey;
    }
    private void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!PlayerBehaviour.Instance.IsIn3D()) {

            ApplyFriction();

            if (CanMove) {
                Move();
            }
            
        }
        else {
            //HandleWallCollision
        }
    }
    private void Update() {
        if (gravityEnabled) {
            GroundedCheck();
            JumpAndGravity();

        }
    }
    #endregion
    
    #region gizmos
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
    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(gizmoDrawLoc, 1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(gizmoDrawLoc2, 1f);
    }
    #endregion
    #region GroundedCheck, Friction, Move, Jump, Gravity

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
        Vector3 spherePosition = new(transform.position.x, transform.position.y - GroundedOffset,
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


    private void Move() {
        
        float targetSpeed = maxSpeed2D;

        var input = GetInput();

        if (input.x < 0) {
            spriteRenderer.flipX = true;
        }
        else if (input.x > 0) {
            spriteRenderer.flipX = false;
        }
        if (input.x < .01f && input.x > -.01f) {
            targetSpeed = 0.0f;
        }
        else {
        }
        // accelerate or decelerate to target speed

        _speedHorizontal = targetSpeed;

        var left = -transform.right;
        Vector3 directionX = left * input.x;

        if (!gravityEnabled) {
            if (input.y < .01f && input.y > -.01f)
                targetSpeed = 0.0f;
            else {
                targetSpeed = maxSpeed2D;
            }
            _speedVertical = targetSpeed;


            var up = transform.up;
            Vector3 directionY = up * input.y;
            // move the player in x and y direction

            rb.AddForce((directionX * _speedHorizontal +
                        directionY * _speedVertical) * movementForceMultiplier);

            //clamp overal velocity at moveSpeed2D
            if (rb.velocity.magnitude > maxSpeed2D) {
                rb.velocity = rb.velocity.normalized * maxSpeed2D;
            }


        }
        //gravity is enabled
        else {
            //add horizontal force
            rb.AddForce(directionX * (_speedHorizontal * movementForceMultiplier * 6f));

            //clamp horizontal velocity at moveSpeed2D
            if (Mathf.Abs(rb.velocity.x) > maxSpeed2D) {
                rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed2D, maxSpeed2D), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxSpeed2D, maxSpeed2D));
            }


            rb.velocity = new Vector3(rb.velocity.x, _verticalVelocity, rb.velocity.x); //apply gravity
            //Debug.Log(rb.velocity);

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
            if (jumpKey.wasPressedThisFrame && _jumpTimeoutDelta <= 0.0f) {
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
            _verticalVelocity += Gravity * Time.deltaTime;
        }

    }
    //handles player movement in 2D
    #endregion
    #region transition to a new axis
    void TransitionToNewAxis(Vector3 closestPointOnBounds, WallBehaviour wall) {
        Debug.Log("TransitionToNewAxis");
        // Store the current world position of the CinemachineFollowTarget
        Vector3 originalFollowTargetPosition = Camera.main.transform.position;
        
        CinemachineFollowTarget.transform.localRotation = Quaternion.identity;
        //cameraTotalRotation = 0f;
        bool flipOffset = transform.forward.x < -.001 || transform.forward.z < -.001;
        //rotate first to get correct transform.right

        transform.forward = GetOrthogonalVectorTo2DPlayer(wall.GetComponent<Collider>());


        LockPlayerMovementInForwardDirection();
        SetCurrentWall(wall);


        //only supports changing x/z plane not y (ceiling/floor)
        var offsetDirection = (transform.forward.x < -.001 || transform.forward.z > .001) ? transform.right : -transform.right;

        //flip the offset direction if the player is flipped
        offsetDirection = flipOffset ? -offsetDirection : offsetDirection;

        //get the new position
        newSpritePos = closestPointOnBounds + offsetDirection * offSetAmount;

        //add the offset to the new position
        newSpritePos += transform.forward * PlayerDimensionController.WALL_DRAW_OFFSET;


        var newCameraDesiredPosition = (closestPointOnBounds + offsetDirection * cameraWallConfiner.MinAllowedDistanceToWall);

        gizmoDrawLoc = newCameraDesiredPosition;


        // Move the parent object
        transform.position = newSpritePos;

        // After moving the parent, reset the CinemachineFollowTarget's world position
        CinemachineFollowTarget.transform.position = originalFollowTargetPosition;
        gizmoDrawLoc2 = CinemachineFollowTarget.transform.position;

        //start the coroutine to new position

        cameraWallConfiner.MoveToCorner(newCameraDesiredPosition);
       

    }
    //locks the axes to the up/down/left/right on the wall
    //prevents the dog from slipping into the or out of the wall
    public void LockPlayerMovementInForwardDirection() {

        var right = transform.right;
        var fwd = transform.forward;

        if (fwd.x > 0.1) {
            cameraWallConfiner.SetZeroRotaion(-90f);

        }
        else if (fwd.x < -0.1) {
            cameraWallConfiner.SetZeroRotaion(90f);
        }
        else if (fwd.y > 0.1 || fwd.y < -0.0001) {
            Debug.LogError("Unsupported behaviour - doggo on floor");
        }
        else if (fwd.z > 0.1) {
            cameraWallConfiner.SetZeroRotaion(180f);
        }
        else if (fwd.z < -0.1) {
            cameraWallConfiner.SetZeroRotaion(0f);
        }

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

    #endregion
    #region Unity Collision Methods
    public void HandleWallCollision(Collider collider, WallBehaviour wallB, bool convex) {

        if (PlayerBehaviour.Instance.IsIn3D()) return;
        

        var closestPoint = collider.ClosestPointOnBounds(transform.position);

        if (wallB.IsWalkThroughEnabled) {
           // Debug.Log("HandleWallCollision");
            WallBehaviour pastwall = currentWall;
            SetCurrentWall(wallB);
            //if (pastwall == null || IsPerpendicular(wallB.transform, pastwall.transform)) {
            //    SetCurrentWall(wallB);
            //    TransitionToNewAxis(closestPoint, wallB);

            //}
            if (pastwall == null || IsWallAtNewAngle(wallB.transform)) {
              //  Debug.Log("1");
                SetCurrentWall(wallB);
                TransitionToNewAxis(closestPoint, wallB);
            }
            else if (convex) {
              //  Debug.Log("2");
                SetCurrentWall(wallB);
                TransitionToNewAxis(closestPoint, wallB);
            }
        }
    }

    private bool IsWallAtNewAngle(Transform wall) {
        return wall.up != transform.forward && -wall.up != transform.forward;
    }
    private void OnCollisionEnter(Collision collision) {
        if (PlayerBehaviour.Instance.IsIn3D()) return;

        if (collision.gameObject.TryGetComponent(out WallBehaviour wallB)) {
           // Debug.Log("wallB " + wallB.name);
            //if (wallB.IsWalkThroughEnabled) {

            HandleWallCollision(collision.collider, wallB, false);
            //}
        }
    }
    private void OnCollisionExit(Collision collision) {

        if (PlayerBehaviour.Instance.IsIn3D()) return;

        if (collision.gameObject.TryGetComponent(out WallBehaviour wallB)) {


            if (currentWall == wallB && !PlayerBehaviour.Instance.IsIn3D()) {
                //Debug.Log(collision.gameObject.name + " exited");

                UpdateWallStatus();

            }
            else if (PlayerBehaviour.Instance.IsIn3D()) {
                //currentWall = null;
            }
        }
    }
    #endregion
   
    #region checking if in current wall

    private void UpdateWallStatus() {
        if (CheckIfInCurrentWall()) {

            //do nothing still in the wall
        }
        else {
        //    Debug.Log("update wall status exit wall");
            PlayerBehaviour.Instance.playerDimensionController.TransitionTo3D();
        }
    }
    bool CheckIfInCurrentWall() {
        if (currentWall == null) {

            return false;
        }
        var ray = new Ray(transform.position, -transform.forward);
        var hits = Physics.RaycastAll(ray, 10f, LayerMask.GetMask("Walls"));

        

        if (Physics.Raycast(transform.position, -transform.forward,
            out var hit, wallCheckDistance, LayerMask.GetMask("Walls"))) {
            if (hit.collider.gameObject == currentWall.gameObject) {
                
                return true;
            }
            else {
                var newWallB = hit.collider.GetComponent<WallBehaviour>();
                if (newWallB.IsWalkThroughEnabled)
                {
                    currentWall = hit.collider.GetComponent<WallBehaviour>();
                    transform.position = hit.point + wallCheckDistance / 4 * transform.forward;
                   // Debug.Log("player moved closer to wall");
                    return true;
                }
                return false;
                
            }
        }
        return false;
    }
    #endregion
    #region helper methods
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    public Vector2 GetInput() {
        var keyboard = Keyboard.current;
        return new Vector2(keyboard.dKey.isPressed ? 1 : keyboard.aKey.isPressed ? -1 : 0,
                           keyboard.wKey.isPressed ? 1 : keyboard.sKey.isPressed ? -1 : 0);
    }
    bool IsPerpendicular(Transform obj1, Transform obj2) {
        float dot = Vector3.Dot(obj1.forward, obj2.forward);
        return Mathf.Approximately(dot, 0);
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
                Debug.Log("something that wasnt a wall is blocking " + hit.name);
                return false;
            }
        }

        return true;
    }
    private Vector3 GetOrthogonalVectorTo2DPlayer(Collider collider) {


        Vector3 closestPoint = collider.ClosestPointOnBounds(transform.position);

        // Calculate the direction from the closest point to the player
        Vector3 direction = transform.position - closestPoint;

        // Zero out the y component to ensure the direction is only in the x or z direction
        direction.y = 0;

        // Normalize the vector to make it a unit vector
        direction.Normalize();

        // Ensure the vector points outwards from the collider
        //if (Vector3.Dot(direction, collider.transform.forward) > 0) {
        //    direction = -direction;
        //}

        return direction;
    }
    public void SetCurrentWall(WallBehaviour wall) {
        currentWallCollider = wall.GetComponent<Collider>();
        currentWall = wall;
        if (currentWall is GravityWall) {
            EnableGravity();
        }
        else {
            DisableGravity();
        }
    }
    public WallBehaviour GetCurrentWall() {
        return currentWall;
    }
    public void SetProjectionState(ProjectionState state) {
        projectionState = state;
        switch (projectionState) {
            case ProjectionState.OutOfRange:
                spriteRenderer.sprite = sprites[1];
                Is2DPlayerActive = false;
                break;
            case ProjectionState.HoldingObject:
                if (PlayerBehaviour.Instance.pickupController.HeldObject is TransferableObject)
                    spriteRenderer.sprite = sprites[2];
                else
                    spriteRenderer.sprite = sprites[1];
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
        gravityEnabled = true;
    }
    public void DisableGravity() {
        gravityEnabled = false;
    }
    #endregion
}
