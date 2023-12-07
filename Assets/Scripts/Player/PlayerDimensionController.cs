using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Cinemachine;
using System.Collections.Generic;

public class PlayerDimensionController : MonoBehaviour {
    public const float WALL_DRAW_OFFSET = .21f;

    [Header("Player 3D")]
    [SerializeField] private GameObject player3D;
    [SerializeField] private Vector3 directionVectorFromNearestWall;
    [SerializeField] private Collider currentProjectionSurface;
    [SerializeField] private List<Collider> potentialProjectionSurfaces;

    [Header("Player 2D")]
    [SerializeField] private GameObject player2D;
    [SerializeField] private MovementController_2D movementController_2D;
    [SerializeField] private SpriteRenderer dog2DSpriteRenderer;
    [SerializeField] private Collider dog2DHitbox;
   
    [SerializeField] private Vector3 dog2DExtents = new Vector3(0.88f, 4.71f, 4.98f);


    [Header("Cameras")]
    [SerializeField] private GameObject playerCamRoot;
    [SerializeField] private CinemachineVirtualCamera VirtualCamera3D;
    [SerializeField] private GameObject Camera3D;
    [SerializeField] private GameObject Camera2D;
    [SerializeField] private float LockCameraOn3DTranstionTime = 1f;

    [Header("Launch")]
    [SerializeField] private float playerLeaveWallOffset = 6f;
    // [SerializeField] private float launchForce = 10f;

    [Header("Settings")]
    public bool IsProjecting = false;
    public bool DOGEnabled = true;
    private bool paused = false;

    private float wallDrawOffset = WALL_DRAW_OFFSET;
    private KeyControl DOGToggleKey;
    private KeyControl DOGLeaveKey;
    private KeyControl pauseKey;

    private Vector3 gizmoDrawLocation = Vector3.zero;
    private Vector3 gizmoDrawLocation2 = Vector3.zero;


    // public float DOGProjectionRange = 25f;

    public Collider CurrentProjectionSurface { get => currentProjectionSurface; set => currentProjectionSurface = value; }

    private void Awake() {
        DOGToggleKey = Keyboard.current.fKey;
        DOGLeaveKey = Keyboard.current.spaceKey;
        pauseKey = Keyboard.current.escapeKey;
        potentialProjectionSurfaces = new();
        
    }
    private void Start() {
        
    }
    private void Update() {
        PlayerBehaviour.Instance.interfaceScript.SetDogAutoEnabledText(DOGEnabled);
        HandlePauseInput();
        HandleAutoModeInput();
       // if (IsProjecting == false)
        //{
           // DisableProjections();
        //}
        if (PlayerBehaviour.Instance.IsIn3D() && DOGEnabled)
            HandleSurfaceProjection();
    }
    private void HandlePauseInput() {
        if (pauseKey.wasPressedThisFrame) {
            paused = !paused;

            PlayerBehaviour.Instance.SetPaused(paused);
            PlayerBehaviour.Instance.thirdPersonController.SetPaused(paused);
            if (paused) {
                Time.timeScale = 0;
            }
            else {
                Time.timeScale = 1;
            }

            if (paused) {
                PlayerBehaviour.Instance.interfaceScript.Pause();

            }
            else {
                PlayerBehaviour.Instance.interfaceScript.UnPause();

            }
        }
    }
    public void AddWallToPotentialSurfaces(Collider wallCollider) {
        if (potentialProjectionSurfaces.Contains(wallCollider)) return;
        potentialProjectionSurfaces.Add(wallCollider);
    }
    public void RemoveWallFromPotentialSurfaces(Collider wallCollider) {
        potentialProjectionSurfaces.Remove(wallCollider);
    }

    private Vector3 GetOrthogonalVectorTo3DPlayer(Collider collider) {
        Vector3 closestPoint = collider.ClosestPointOnBounds(player3D.transform.position);

        // Calculate the direction from the closest point to the player
        Vector3 direction = player3D.transform.position - closestPoint;

        // Zero out the y component to ensure the direction is only in the x or z direction
        direction.y = 0;

        // Normalize the vector to make it a unit vector
        direction.Normalize();


        return direction;
    }
    // This method checks if two vectors are close based on a threshold.
    private bool IsVectorClose(Vector3 v1, Vector3 v2, float threshold) {
        // Normalize the vectors to compare directions regardless of magnitude
        v1.Normalize();
        v2.Normalize();

        // Use the Dot product to check how similar the directions are
        float dot = Vector3.Dot(v1, v2);

        // If dot product is close to 1, the vectors are similar in direction
        return Mathf.Abs(dot) > (1 - threshold);
    }
    public void EnableProjection(Collider collider, Vector3 closestPointOnBounds) {
        if (!IsProjecting || player2D.activeSelf == false) {
            //offset the drawing a bit
            //goal should be to set it just outside the moveable wall collider 


            var directionToWall = GetOrthogonalVectorTo3DPlayer(collider);
            //var directionToWall = GetOutwardDirectionUsingMeshNormals(collider);

            //dont project onto the forward or -forward of walls
            if (IsVectorClose(directionToWall, collider.transform.forward, .5f)) {
                return;
            }



            SetDirectionFromNearestWall(directionToWall);

            closestPointOnBounds += directionToWall * wallDrawOffset;

            
            
            player2D.SetActive(true);
            dog2DSpriteRenderer.enabled = false;
           
            //move 2d player to this position
            // Debug.Log("before: " + player2D.transform.forward);
            player2D.transform.position = closestPointOnBounds;
            dog2DHitbox.transform.position = closestPointOnBounds;
            player2D.transform.forward = directionToWall;
            movementController_2D.GetComponent<Rigidbody>().position = closestPointOnBounds;
            // Debug.Log("after: " + player2D.transform.forward);
            Debug.Log(dog2DHitbox.transform.rotation);
            
            
            //Debug.Log(dog2DHitbox.transform.rotation);

            //perform a physics overlap test to see if the space is free of walls that arent transferable
            var boxHits = Physics.OverlapBox(closestPointOnBounds, dog2DHitbox.transform.rotation * dog2DExtents, dog2DHitbox.transform.rotation, LayerMask.GetMask("Walls", "Doors", "Default", "Ground"));

            gizmoDrawLocation = closestPointOnBounds;
            //Debug.Log("e: " + dog2DHitbox.bounds.extents);
            //iterate through anything that was hit
            if (boxHits.Length > 0) {
                foreach (var hit in boxHits) {
                    //  Debug.Log("enable: " + hit.name);
                    //make sure its a wall
                    if (hit.TryGetComponent(out WallBehaviour wallB)) {
                        //check if the wall doesnt allow transitioning or walking
                        if (!wallB.AllowsDimensionTransition || !wallB.IsWalkThroughEnabled) {
                            //disable the projects and quit out of the method
                            DisableProjections();
                            return;
                        }
                    }
                    //door was hit
                    else {
                        DisableProjections();
                        return;
                    }
                }
            }

            Set2DSprite(collider);
            IsProjecting = true;
            //  Debug.Log("Enabling projections");
            IsProjecting = true;
            dog2DSpriteRenderer.enabled = true;
            // player2D.SetActive(true);
           
        }
        else {

            //handle potentially changing the projection to the other wall
        }
    }
    private void SetDirectionFromNearestWall(Vector3 direction) {
        directionVectorFromNearestWall = ModifyVector(direction, .01f);
    }
    public void UpdateProjectionPosition(Collider collider, Vector3 closestPointOnBounds) {
        //dont project onto the forward or -forward of walls
        if (!PlayerBehaviour.Instance.IsIn3D()) {
            Debug.LogWarning("Updating projection position when not in 3d");
            return;
        }

        if (IsVectorClose(directionVectorFromNearestWall, collider.transform.forward, .01f)) {
            DisableProjections();
            return;
        }
        SetDirectionFromNearestWall(GetOrthogonalVectorTo3DPlayer(collider));

        closestPointOnBounds += directionVectorFromNearestWall * wallDrawOffset;



        //perform a physics overlap test to see if the space is free of walls that arent transferable
        var boxHits = Physics.OverlapBox(closestPointOnBounds, dog2DHitbox.transform.rotation * dog2DExtents, dog2DHitbox.transform.rotation, LayerMask.GetMask("Walls", "Doors", "Default", "Ground"));
        // Debug.Log("u: " + dog2DHitbox.bounds.extents);
        gizmoDrawLocation2 = closestPointOnBounds;

        //iterate through anything that was hit
        if (boxHits.Length > 0) {
            foreach (var hit in boxHits) {
                // Debug.Log("update: " + hit.name);
                //make sure its a wall
                if (hit.TryGetComponent(out WallBehaviour wallB)) {
                    //check if the wall doesnt allow transitioning or walking
                    if (!wallB.AllowsDimensionTransition || !wallB.IsWalkThroughEnabled) {
                        //disable the projects and quit out of the method
                        DisableProjections();
                        return;
                    }
                }
                //door was hit
                else {
                    DisableProjections();
                    return;
                }
            }
        }

        player2D.transform.position = closestPointOnBounds;
        player2D.transform.forward = directionVectorFromNearestWall;

        Set2DSprite(collider);
    }
    //takes a vector and compares its values to a threshold
    //if more than 1 component is greater than the threshold, the largest component is set to 1 or -1 based on its sign and the others are zeroed out
    public Vector3 ModifyVector(Vector3 vector, float threshold) {
        // Count how many components are greater than the threshold
        int count = 0;
        count += Mathf.Abs(vector.x) > threshold ? 1 : 0;
        count += Mathf.Abs(vector.y) > threshold ? 1 : 0;
        count += Mathf.Abs(vector.z) > threshold ? 1 : 0;

        // If more than one component is greater than the threshold
        if (count > 1) {
            // Find the largest absolute value component and its sign
            float maxValue = Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
            int maxIndex = maxValue == Mathf.Abs(vector.x) ? 0 : (maxValue == Mathf.Abs(vector.y) ? 1 : 2);
            float sign = (maxIndex == 0 ? Mathf.Sign(vector.x) : (maxIndex == 1 ? Mathf.Sign(vector.y) : Mathf.Sign(vector.z)));

            // Set the largest component to 1 or -1 based on its sign and zero out the others
            vector.x = maxIndex == 0 ? sign : 0;
            vector.y = maxIndex == 1 ? sign : 0;
            vector.z = maxIndex == 2 ? sign : 0;
        }

        return vector;
    }

    void Set2DSprite(Collider collider) {
        if (collider.TryGetComponent(out WallBehaviour wallB)) {
            //player is allowed to transition to the wall
            if (wallB.AllowsDimensionTransition) {
                //player can transition and is holding an object
                if (PlayerBehaviour.Instance.pickupController.IsHoldingObject()) {
                    movementController_2D.SetProjectionState(MovementController_2D.ProjectionState.HoldingObject);
                }
                else {
                    movementController_2D.SetProjectionState(MovementController_2D.ProjectionState.OutOfRange);
                }
            }
            //player cant transition
            else {
                //dont show anything
                DisableProjections();
            }
        }
    }
    //activate the 2d player
    void SetWallProjectionToActive() {
        if (PlayerBehaviour.Instance.pickupController.IsHoldingObject()) {
            movementController_2D.SetProjectionState(MovementController_2D.ProjectionState.In2DHoldingObject);
        }
        else {
            movementController_2D.SetProjectionState(MovementController_2D.ProjectionState.In2D);
        }
    }


    public void TryTransitionTo2D() {
        if (movementController_2D.
            IsProjectionSpaceClear(movementController_2D.transform.position)
            && IsProjecting == true) {

            var dist = Vector3.Distance(player2D.transform.position, player3D.transform.position);
           // Debug.Log("dist: " + dist);

            if (Vector3.Distance(player2D.transform.position, player3D.transform.position) < 3.3f) {
              //  Debug.Log("Transitioning to 2d");
                TransitionTo2D();
            }
        }
        else {
            // Debug.Log("Transition area blocked or its not projecting");
        }
    }
    private void OnDrawGizmos() {
        //Debug.Log("hit this");
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(gizmoDrawLocation, .5f);
        Gizmos.DrawWireCube(gizmoDrawLocation, dog2DHitbox.transform.rotation * dog2DExtents);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(gizmoDrawLocation2, dog2DHitbox.transform.rotation * dog2DExtents);
    }

    private void TransitionTo2D() {

        movementController_2D.GetComponent<Rigidbody>().isKinematic = false;
        movementController_2D.SetCurrentWall(currentProjectionSurface.GetComponent<WallBehaviour>());
        Debug.Log(player2D.transform.position);
        SetWallProjectionToActive();
        PlayerBehaviour.Instance.ChangeDimension();
        player3D.SetActive(false);

        
        Camera3D.SetActive(false);
        Camera2D.SetActive(true);

        //tell the movement controller to lock axes
        movementController_2D.LockPlayerMovementInForwardDirection();
        if (player3D.TryGetComponent(out StarterAssetsInputs sAssetsInput)) {
            sAssetsInput.ClearInput();
        }
        Physics.IgnoreLayerCollision(LayerInfo.PLAYER, LayerInfo.INTERACTABLE_OBJECT);

        //player is in the correct position at the end of this function

    }
    public void TransitionTo3D() {
        VirtualCamera3D.LookAt = player2D.transform;
        VirtualCamera3D.Follow = Camera2D.transform;
        PlayerBehaviour.Instance.thirdPersonController.LockCameraPosition = true;
        StartCoroutine(PlayerBehaviour.Instance.thirdPersonController.EnableCameraRotationAfterSeconds(LockCameraOn3DTranstionTime));
        //adjust the player 3d model to be in front of the wall offset by a small amount
        MovePlayerOutOfWall(player2D.transform.position + player2D.transform.forward * playerLeaveWallOffset);
        Physics.IgnoreLayerCollision(LayerInfo.PLAYER, LayerInfo.INTERACTABLE_OBJECT, false);
    }
    private void MovePlayerOutOfWall(Vector3 newPos) {
        player2D.SetActive(false);
        PlayerBehaviour.Instance.pickupController.ClearList();
        ClearSurfaces();
        //set its rotation so its not clipping into the wall hopefully
        player3D.transform.position = newPos;
        player3D.transform.forward = player2D.transform.right;
        player3D.SetActive(true);
        PlayerBehaviour.Instance.ChangeDimension();
        Camera3D.SetActive(true);
        Camera2D.SetActive(false);
        VirtualCamera3D.LookAt = null;
        VirtualCamera3D.Follow = playerCamRoot.transform;
        if (player3D.TryGetComponent(out StarterAssetsInputs sAssetsInput)) {
            sAssetsInput.ClearInput();
        }
    }
    public void TransitionTo3DLaunch() {

        Vector3 launchDirection = player2D.transform.forward;

        //adjust the player 3d model to be in front of the wall offset by a small amount
        Vector3 launchPosition = player2D.transform.position + launchDirection * playerLeaveWallOffset;
        PlayerBehaviour.Instance.thirdPersonController.LockCameraPosition = true;
        StartCoroutine(PlayerBehaviour.Instance.thirdPersonController.EnableCameraRotationAfterSeconds(LockCameraOn3DTranstionTime));

        //  print(player3D.transform.position);
        MovePlayerOutOfWall(launchPosition);

        Rigidbody player3DRigidbody = player3D.GetComponent<Rigidbody>();


        player3DRigidbody.AddForce(launchDirection * (PlayerBehaviour.Instance.player2DMovementController.GetCurrentWall().LaunchForce * player3DRigidbody.mass), ForceMode.Impulse);
        //if (PlayerBehaviour.Instance.pickupController.IsHoldingObject()) {
        //    var objectRb3D = PlayerBehaviour.Instance.pickupController.HeldObject.GetComponent<Rigidbody>();
        //    objectRb3D.AddForce(launchDirection * (PlayerBehaviour.Instance.player2DMovementController.GetCurrentWall().LaunchForce * objectRb3D.mass), ForceMode.Impulse);
        //}
        DOGEnabled = !DOGEnabled;
        //player3DRigidbody.AddForce(launchDirection * 0, ForceMode.Impulse);
        Physics.IgnoreLayerCollision(LayerInfo.PLAYER, LayerInfo.INTERACTABLE_OBJECT, false);
    }
    //handle enable/disasble of DOG device while in auto mode
    private void HandleAutoModeInput() {
        if (DOGLeaveKey.wasPressedThisFrame) {
            if (!PlayerBehaviour.Instance.IsIn3D() && movementController_2D.CanTransitionOutOfCurrentWall()) {
                DOGEnabled = !DOGEnabled;
                TransitionTo3DLaunch();
            }

        }
        if (DOGToggleKey.wasPressedThisFrame) {
            DOGEnabled = !DOGEnabled;
            PlayerBehaviour.Instance.interfaceScript.SetDogAutoEnabledText(DOGEnabled);
            if (PlayerBehaviour.Instance.IsIn3D()) {
                if (IsProjecting) {
                    DisableProjections();
                }
                else {

                    HandleSurfaceProjection();
                    //IsProjecting = true;
                    
                }
            }
            else {
                //if (movementController_2D.CanTransitionOutOfCurrentWall()) {

                //    TransitionTo3D();
                //}
            }
        }
    }
    //disable all projections
    public void DisableProjections() {

        //if (IsProjecting) {
            //  Debug.Log("Disabling projections");
            player2D.SetActive(false);
            IsProjecting = false;

       // }
        //else
        //{
            player2D.SetActive(false);
        //}



    }
    private void HandleOneSurfaceNearby() {
        //if the only surface found is not transferable disable project and quit out
        if (!potentialProjectionSurfaces[0].GetComponent<WallBehaviour>().AllowsDimensionTransition) {
            DisableProjections();
            return;
        }


        currentProjectionSurface = potentialProjectionSurfaces[0];
        if (IsProjecting) {
            //update the position if currently projecting\

            UpdateProjectionPosition(currentProjectionSurface, currentProjectionSurface.ClosestPointOnBounds(PlayerBehaviour.Instance.player3D.transform.position));
        }
        //found a surface and wasnt projecting before
        else {
            //so enable the projection at the closest point
            EnableProjection(currentProjectionSurface, currentProjectionSurface.ClosestPointOnBounds(PlayerBehaviour.Instance.player3D.transform.position));
        }
    }
    private void HandleMultipleSurfacesNearby() {

        float distance = float.MaxValue;
        Collider closest = null;
        Vector3 closestPointOnBounds = Vector3.zero;
        
                var transferableSurfaces = potentialProjectionSurfaces.FindAll(collider => {
                    if (collider.TryGetComponent(out WallBehaviour wallB)) {
                        return wallB;
                    }
                    return false;
                });
        
        //var transferableSurfaces = potentialProjectionSurfaces.FindAll(collider);

        //iterate colliders that are currently in range of the player's interaction range
        foreach (Collider c in transferableSurfaces) {
            var closePoint = c.ClosestPointOnBounds(PlayerBehaviour.Instance.player3D.transform.position);
            var distToCollider = Vector3.Distance(closePoint, PlayerBehaviour.Instance.player3D.transform.position);

            //looking for the closest one to the player
            if (distToCollider < distance) {
                distance = distToCollider;
                closest = c;
                closestPointOnBounds = closePoint;
            }
        }
        //enable the projection on the closest wall
        closest.TryGetComponent(out WallBehaviour wallB);
        if (closest != null && wallB.AllowsDimensionTransition && IsProjecting) {
            currentProjectionSurface = closest;
            if (IsProjecting) {
                UpdateProjectionPosition(currentProjectionSurface, closestPointOnBounds);
            }
            else {
                EnableProjection(currentProjectionSurface, closestPointOnBounds);
            }

        }
        else {
            DisableProjections();
        }
    }

    public void HandleSurfaceProjection() {
        
        if (potentialProjectionSurfaces.Count == 0) {
            DisableProjections();
            return;
        }
        if (potentialProjectionSurfaces.Count == 1) {
            HandleOneSurfaceNearby();
        }
        //more than 1 potential surface need to find the closest one to the player
        else {
            HandleMultipleSurfacesNearby();
        }
    }
    public void ClearSurfaces() {
        potentialProjectionSurfaces.Clear();
    }
}
