using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CameraWallConfiner : MonoBehaviour {
    public Transform playerTransform; // Reference to the player transform
    public MovementController_2D playerMovementController; // Reference to the player's movement controller
    public CinemachineVirtualCamera virtualCamera2D; // Reference to the virtual camera

    public float MinAllowedDistanceToWall = 30f; // Maximum allowed distance to the wall
    public LayerMask wallLayer; // Layer mask to identify walls
    private CinemachinePOV cameraPOV;
    private CinemachineTransposer cameraTransposer;
    private Vector3 originalCameraOffset;
    private float yZeroRotationAngle;
    public bool IsTranstitingCorner { get; set; } = false;
    public float cornerTransitDuration = 0.5f; // Duration of the transition

    [SerializeField] private float HorizontalCameraClamp = 25f;
    [SerializeField] private float transitionSpeed = 5f; // Speed of transition to new angle
    [SerializeField] private float resetWaitTime = 2f; // Speed of transition to new angle
    private bool isResetting = false;
    private float resetTransitionSpeed; // Speed of transition to new angle
    private bool cameraNeedsToBeReset = false;


    private Vector3 desiredLocation;
    private float yMin, yMax;
    private float yZeroMin, yZeroMax;

    public Vector3 OffsetFromPlayer; // Initial offset from the player

    //public float YRotation { get => yZeroRotationAngle; set => yZeroRotationAngle = value; }

    public void SetZeroRotaion(float angle) {
        yZeroRotationAngle = angle;
        yZeroMin = yZeroRotationAngle - HorizontalCameraClamp;
        yZeroMax = yZeroRotationAngle + HorizontalCameraClamp;
    }

    void Start() {
        // Calculate and store the initial offset from the player
        OffsetFromPlayer = transform.position - playerTransform.position;
        cameraPOV = virtualCamera2D.GetCinemachineComponent<CinemachinePOV>();
        cameraTransposer = virtualCamera2D.GetCinemachineComponent<CinemachineTransposer>();
        originalCameraOffset = cameraTransposer.m_FollowOffset;
        resetTransitionSpeed = transitionSpeed;
    }
    void FindAndSetDesiredPosition() {

        Debug.DrawRay(playerTransform.position, -playerTransform.right * MinAllowedDistanceToWall, Color.yellow);
        Debug.DrawRay(playerTransform.position, playerTransform.right * MinAllowedDistanceToWall, Color.yellow);

        if (IsTranstitingCorner) {
            return;
        }

        var rightColliders = Physics.RaycastAll(playerTransform.position, -playerTransform.right, MinAllowedDistanceToWall, wallLayer);
        var leftColliders = Physics.RaycastAll(playerTransform.position, playerTransform.right, MinAllowedDistanceToWall, wallLayer);

        var currentWall = playerMovementController.GetCurrentWall();
        GameObject currentWallGameobject;
        if (currentWall) {
            currentWallGameobject = currentWall.gameObject;
        }
        else {
            currentWallGameobject = PlayerBehaviour.Instance.playerDimensionController.CurrentProjectionSurface.gameObject;
        }

        //do not adjust position if the player is transitioning corners
        //allows the camera to smoothly move to the next corner without being interrupted

        foreach (var rayCastHit in rightColliders) {
            if (rayCastHit.collider.gameObject == currentWallGameobject) {
                continue;
            }
            transform.position = (rayCastHit.point + playerTransform.right * MinAllowedDistanceToWall) + OffsetFromPlayer;
            return;
        }
        foreach (var rayCastHit in leftColliders) {
            if (rayCastHit.collider.gameObject == currentWallGameobject) {
                continue;
            }
            transform.position = (rayCastHit.point + -playerTransform.right * MinAllowedDistanceToWall) + OffsetFromPlayer;
            return;

        }
        if (IsCenteredOnPlayer()) {
            transform.localPosition = Vector3.zero;
        }

    }

    void Update() {
        cameraNeedsToBeReset = cameraNeedsToBeReset || cameraPOV.m_HorizontalAxis.m_InputAxisValue > .01f;
        FindAndSetDesiredPosition();

    }
    private bool IsCenteredOnPlayer() {
        return Mathf.Approximately(transform.localPosition.x, 0f) || !Mathf.Approximately(transform.localPosition.z, 0f);
    }
    public void MoveToCorner(Vector3 targetPosition) {
        cameraTransposer.m_FollowOffset = Vector3.zero;
        StartCoroutine(MoveToPosition(targetPosition));
    }
    IEnumerator MoveToPosition(Vector3 targetPosition) {
        float elapsedTime = 0;

        Vector3 startPosition = transform.position;
      //  IsTranstitingCorner = true;
        while (elapsedTime < cornerTransitDuration) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / cornerTransitDuration);
           cameraTransposer.m_FollowOffset = Vector3.Lerp(Vector3.zero, originalCameraOffset, elapsedTime / cornerTransitDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
     //   IsTranstitingCorner = false;
        transform.position = targetPosition; // Ensure the position is set to the target at the end
        cameraTransposer.m_FollowOffset = originalCameraOffset;
    }
    
    private void LateUpdate() {

        //dont adjust the camera while the player is moving the mouse
        if (cameraPOV.m_HorizontalAxis.m_InputAxisValue > .01f) {
            transitionSpeed = 0f;
        }
        else if (!isResetting && cameraNeedsToBeReset){
            StartCoroutine(ResetCameraAfter(resetWaitTime));
        }

        var directionToWall = -playerTransform.forward;
        var vectorToPlayer = playerTransform.position - virtualCamera2D.transform.position;
        var targetAngle = Vector3.SignedAngle(directionToWall, vectorToPlayer, Vector3.up);

        // Smoothly interpolate the camera's horizontal axis value towards the target angle
        var currentHorizontalAxis = cameraPOV.m_HorizontalAxis.Value;
        cameraPOV.m_HorizontalAxis.Value = Mathf.Lerp(currentHorizontalAxis, yZeroRotationAngle + targetAngle, transitionSpeed * Time.deltaTime);
        cameraPOV.m_HorizontalAxis.m_MinValue = yZeroMin + targetAngle;
        cameraPOV.m_HorizontalAxis.m_MaxValue = yZeroMax + targetAngle;


        // Adjust camera rotation to track the player
        //   AdjustCameraRotation();
    }
    IEnumerator ResetCameraAfter(float time) {
        isResetting = true;
        yield return new WaitForSeconds(time);
        transitionSpeed = resetTransitionSpeed;
        isResetting = false;
        cameraNeedsToBeReset = false;
        Debug.Log("reset camera rotation");
    }
    private void AdjustCameraRotation() {
        // CinemachinePOV cameraPOV = virtualCamera2D.GetCinemachineComponent<CinemachinePOV>();

        // Calculate the direction from the camera to the player
        Vector3 cameraForwardHorizontal = virtualCamera2D.transform.forward;
        cameraForwardHorizontal.y = 0;

        Vector3 directionToPlayerHorizontal = playerTransform.position - virtualCamera2D.transform.position;
        directionToPlayerHorizontal.y = 0;

        // Calculate the signed angle on the horizontal plane
        float angleToPlayer = Vector3.SignedAngle(cameraForwardHorizontal, directionToPlayerHorizontal, Vector3.up);

        // Set the horizontal axis value to angle to player
        //cameraPOV.m_HorizontalAxis.Value = angleToPlayer;

        // Debug.Log(angleToPlayer);

        // Set the horizontal axis value to angle to player
        //    cameraPOV.m_HorizontalAxis.Value = angleToPlayer;

        // Adjust min and max values to allow a range of +/- 25 degrees from the current angle
        //  cameraPOV.m_HorizontalAxis.m_MinValue = angleToPlayer - 25f;
        //   cameraPOV.m_HorizontalAxis.m_MaxValue = angleToPlayer + 25f;
    }
}
