using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWallConfiner : MonoBehaviour {
    public Transform playerTransform; // Reference to the player transform
    public MovementController_2D playerMovementController; // Reference to the player's movement controller
    public CinemachineVirtualCamera virtualCamera2D; // Reference to the virtual camera
    public float maxDistanceToWall = 5f; // Maximum allowed distance to the wall
    public LayerMask wallLayer; // Layer mask to identify walls
    private CinemachinePOV cameraPOV;
    private float yZeroRotationAngle;
    private bool offset = false;

    [SerializeField] private float HorizontalCameraClamp = 25f;
    [SerializeField] private float transitionSpeed = 1f; // Speed of transition to new angle

    private float yMin, yMax;
    private float yZeroMin, yZeroMax;

    private Vector3 offsetFromPlayer; // Initial offset from the player

    //public float YRotation { get => yZeroRotationAngle; set => yZeroRotationAngle = value; }

    public void SetZeroRotaion(float angle) {
        yZeroRotationAngle = angle;
        yZeroMin = yZeroRotationAngle - HorizontalCameraClamp;
        yZeroMax = yZeroRotationAngle + HorizontalCameraClamp;
    }

    void Start() {
        // Calculate and store the initial offset from the player
        offsetFromPlayer = transform.position - playerTransform.position;
        cameraPOV = virtualCamera2D.GetCinemachineComponent<CinemachinePOV>();
    }

    void Update() {
        // Update the position based on the player's position and the initial offset
        Vector3 desiredPosition = playerTransform.position + offsetFromPlayer;

        // Raycast to the left
        RaycastHit hit;
        Debug.DrawRay(playerTransform.position, -playerTransform.right * maxDistanceToWall, Color.yellow);
        Debug.DrawRay(playerTransform.position, playerTransform.right * maxDistanceToWall, Color.yellow);

        var rightColliders = Physics.RaycastAll(playerTransform.position, -playerTransform.right, maxDistanceToWall, wallLayer);
        var leftColliders = Physics.RaycastAll(playerTransform.position, playerTransform.right, maxDistanceToWall, wallLayer);

        var currentWall = playerMovementController.GetCurrentWall();
        GameObject currentWallGameobject;
        if (currentWall) {
            currentWallGameobject = currentWall.gameObject;
        }
        else {
            currentWallGameobject = PlayerBehaviour.Instance.playerDimensionController.CurrentProjectionSurface.gameObject;
        }

        foreach (var rayCastHit in rightColliders) {
            if (rayCastHit.collider.gameObject == currentWallGameobject) {
                continue;
            }
            float distanceToWall = rayCastHit.distance;
            desiredPosition = (rayCastHit.point + playerTransform.right * maxDistanceToWall) + offsetFromPlayer;
           // Debug.Log("right: " + desiredPosition);
        }
        foreach (var rayCastHit in leftColliders) {
            if (rayCastHit.collider.gameObject == currentWallGameobject) {
                continue;
            }
            
            float distanceToWall = rayCastHit.distance;
            //Debug.Log("left: " + desiredPosition);
            desiredPosition = (rayCastHit.point + -playerTransform.right * maxDistanceToWall) + offsetFromPlayer;

        }
        //if (Mathf.Approximately(transform.localPosition.x, 0f) && Mathf.Approximately(transform.localPosition.z, 0f)) {
        //    transform.localPosition = Vector3.zero;

        //}
        //else {
        //    var directionToWall = -playerTransform.forward;
        //    var vectorToPlayer = playerTransform.position - virtualCamera2D.transform.position;
        //    var angle = Vector3.SignedAngle(directionToWall, vectorToPlayer, Vector3.up);

        //   // Debug.Log(angle);

        //    var currentHorizontalAxis = cameraPOV.m_HorizontalAxis.Value;
        //    var difference = yZeroRotationAngle - currentHorizontalAxis;
        //    var angleAndDifference = angle + difference;

        //    Debug.Log(angleAndDifference);

        //    cameraPOV.m_HorizontalAxis.Value = yZeroRotationAngle + angle;
        //    cameraPOV.m_HorizontalAxis.m_MinValue = yZeroMin + angle;
        //    cameraPOV.m_HorizontalAxis.m_MaxValue = yZeroMax + angle;


        //}
        
        transform.position = desiredPosition;
    }
    private void LateUpdate() {
        if (!Mathf.Approximately(transform.localPosition.x, 0f) || !Mathf.Approximately(transform.localPosition.z, 0f)) {
            var directionToWall = -playerTransform.forward;
            var vectorToPlayer = playerTransform.position - virtualCamera2D.transform.position;
            var targetAngle = Vector3.SignedAngle(directionToWall, vectorToPlayer, Vector3.up);

            // Smoothly interpolate the camera's horizontal axis value towards the target angle
            var currentHorizontalAxis = cameraPOV.m_HorizontalAxis.Value;
            cameraPOV.m_HorizontalAxis.Value = Mathf.Lerp(currentHorizontalAxis, yZeroRotationAngle + targetAngle, transitionSpeed * Time.deltaTime);
            cameraPOV.m_HorizontalAxis.m_MinValue = yZeroMin + targetAngle;
            cameraPOV.m_HorizontalAxis.m_MaxValue = yZeroMax + targetAngle;
        }
        // Adjust camera rotation to track the player
        //   AdjustCameraRotation();
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
