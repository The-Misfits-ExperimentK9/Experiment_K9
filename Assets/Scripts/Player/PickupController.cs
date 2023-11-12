using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PickupController : MonoBehaviour {
    [Header("Pickup Settings")]
    [SerializeField] Transform holdArea;
    public GrabbableObject HeldObject;
    private Rigidbody heldObjectRigidbody;
    
    [Header("Physics Params")]
    [SerializeField] float pickupForce = 100f;
    [SerializeField] float pickupDrag = 20f;
    private bool canInteract = true;                        //disable or enable player interactions
    KeyControl interactKey;

    [SerializeField] private List<GrabbableObject> objectsInInteractRange;    //a list of all the objects that are in interactable range
    [SerializeField] LayerMask PickupBlockingLayers;
    [SerializeField] LayerMask HoldBlockingLayers;

    [SerializeField] private float holdSpaceBlockedMoveOffset = 10f;
    [SerializeField] private float holdAreaMoveSpeed = 10f;
    private Vector3 holdAreaStartLocalPosition;

    private void Awake() {
        interactKey = Keyboard.current.eKey;
        objectsInInteractRange = new();
        holdAreaStartLocalPosition = holdArea.localPosition;
    }



    private void Update() {
        if (canInteract)
            HandleInteractionInput();

        
    }


    private void HandleInteractionInput() {

        if (interactKey.wasPressedThisFrame) {

            //if the player is already holding something then drop it
            if (IsHoldingObject()) {
                DropHeldObject();

            }
            //only process interact press if theres something to interact with
            else {
                PickupObject();
            }
        }
        //only do this if its in 3d 2d won't work
        if (IsHoldingObject() && PlayerBehaviour.Instance.IsIn3D()) {
            MoveObject();
        }
    }
    public bool IsHoldingObject() {
        return HeldObject != null;
    }
    private void PickupObject() {
        if (PlayerBehaviour.Instance.IsIn3D()) {
            Handle3DInteractions();
        }
        else {
            Pickup2DObject();
        }
    }
    private void MoveObject() {
        if (transform.localPosition.magnitude > 0.2f) {

            var moveDirection = holdArea.position - heldObjectRigidbody.transform.position;
            heldObjectRigidbody.AddForce(moveDirection * pickupForce);
            //Debug.Log(transform.parent);
            //transform.parent = holdArea;
        }
    }

    private void DropHeldObject() {
        //AddObjectToInRangeList(HeldObject);
        //reset the layer to interactable object
        HeldObject.SetLayerRecursively(HeldObject.gameObject, LayerInfo.INTERACTABLE_OBJECT);
        HeldObject.DropObject();
        HeldObject = null;
        heldObjectRigidbody = null;


        //set the sprite to not holding the object
        if (!PlayerBehaviour.Instance.IsIn3D()) {
            PlayerBehaviour.Instance.player2DMovementController.SetProjectionState(MovementController_2D.ProjectionState.In2D);
          //  ResetHoldAreaPosition();
        }
    }

    public void ChangeDimension() {
        if (IsHoldingObject() && HeldObject is TransferableObject) {
            var tObject = HeldObject as TransferableObject;
            heldObjectRigidbody = HeldObject.displayObject3D_Mesh.GetComponent<Rigidbody>();
            HeldObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //swap parent and add offset when moving back to 3D with object
            if (PlayerBehaviour.Instance.IsIn3D()) {
                tObject.SetHoldArea(gameObject, holdArea);

                tObject.Enable3D();
            }
            else {
                tObject.SetHolder(PlayerBehaviour.Instance.player2D);

                tObject.Disable3D();
            }
        }
        else if (IsHoldingObject()) {
            DropHeldObject();
        }
    }

    //handle picking up objects while in 2d
    private void Pickup2DObject() {
        // Debug.Log("trying to drop 2d");
        var tObject = GetObjectClosestTo2DPlayer();

        if (tObject != null && !tObject.Is3D) {
            HeldObject = tObject;
            //pick up the object that was found to be the closest
            (HeldObject as TransferableObject).Pickup2D(PlayerBehaviour.Instance.player2D);
            HeldObject.SetLayerRecursively(HeldObject.gameObject, LayerInfo.IGNORE_PLAYER_COLLISION);
            PlayerBehaviour.Instance.player2DMovementController.SetProjectionState(MovementController_2D.ProjectionState.In2DHoldingObject);

        }
    }
    //handle picking up 3d objects while in 3d 
    private void Handle3DInteractions() {
        var closestGOToCamera = PlayerBehaviour.Instance.GetClosestInteractable3D();

        //handle picking up objects
        if (closestGOToCamera != null) {
            //perform raycast to the object from the camera
            //var ray = new Ray(Camera.main.transform.position, closestGOToCamera.transform.position - Camera.main.transform.position);

            var position = PlayerBehaviour.Instance.player3D.transform.position;
            var ray = new Ray(position, closestGOToCamera.transform.position - position);



            //if the raycast hits something that wasnt the object then return
            if (Physics.Raycast(ray, out var hit,
                100f, PickupBlockingLayers) && hit.collider.gameObject != closestGOToCamera) {
                
                return;
            }

            if (closestGOToCamera.layer == LayerInfo.INTERACTABLE_OBJECT) {
                var tObject = closestGOToCamera.transform.parent.GetComponent<GrabbableObject>();
                //only process interactions with 3d objects while in 3d
                if (tObject != null && tObject.Is3D) {

                    HeldObject = tObject;
                    heldObjectRigidbody = HeldObject.displayObject3D_Mesh.GetComponent<Rigidbody>();

                    HeldObject.SetLayerRecursively(HeldObject.gameObject, LayerInfo.IGNORE_PLAYER_COLLISION);


                    var moveDirection = holdArea.position - heldObjectRigidbody.transform.position;
                    heldObjectRigidbody.AddForce(moveDirection * pickupForce);
                    //pick up the object that was found to be the closest
                    HeldObject.Pickup3D(gameObject, holdArea, pickupDrag);
                }
            }
            //closest object to camera is a Interactable object no pickup (button)
            else if (closestGOToCamera.layer == LayerInfo.INTERACTABLE_OBJECT_NO_PICKUP) {
                //interact with the button
                closestGOToCamera.GetComponent<ReceivableParent>().Activate();
            }
        }

    }
    private bool IsHoldSpaceClear() {
        return !Physics.OverlapSphere(holdArea.position, 0.5f, HoldBlockingLayers).Any();
    }
    private bool IsOriginalHoldSpaceClear() {
        return !Physics.OverlapSphere(transform.position + holdAreaStartLocalPosition, 0.5f, HoldBlockingLayers).Any();
    }
    private void ResetHoldAreaPosition() {
        holdArea.localPosition = holdAreaStartLocalPosition;
    }

    private void AdjustHoldAreaPosition() {
        // Check if the hold area is clear
        if (IsHoldSpaceClear()) {
            // Slowly return to the start position
            holdArea.localPosition = Vector3.MoveTowards(holdArea.localPosition, holdAreaStartLocalPosition, Time.deltaTime * holdAreaMoveSpeed * 2f);
            return;
        }

        // Check space availability in both directions
        float rightClearance = CheckClearance(transform.right);
        float leftClearance = CheckClearance(-transform.right);

        // Choose the direction with more space
        Vector3 moveDirection = rightClearance > leftClearance ? transform.right : -transform.right;

        // Move the hold area
        Vector3 newPosition = holdArea.localPosition + moveDirection * (Time.deltaTime * holdAreaMoveSpeed);
        //Debug.Log("moving hold area");

        // Ensure the movement is within the allowed offset
        if ((newPosition - holdAreaStartLocalPosition).magnitude <= holdSpaceBlockedMoveOffset) {
            holdArea.localPosition = newPosition;
        }
    }

    private float CheckClearance(Vector3 direction) {
        if (Physics.Raycast(holdArea.position, direction, out RaycastHit hit)) {
            return hit.distance;
        }
        else {
            return Mathf.Infinity;
        }
    }
    //returns the interactable object closest to where the player is looking at with the camera
    //TODO not working 100% of the time sometimes choses object closeset to camera even if player isnt looking at it
    //private GrabbableObject GetObjectClosestToCameraLookAt() {
    //    if (!objectsInInteractRange.Any()) {
    //        return null;
    //    }
    //    float closestToCameraLookDirection = float.MaxValue;
    //    GrabbableObject gObject = null;
    //    //iterate each object
    //    foreach (var obj in objectsInInteractRange) {
    //        //get the vector from the object to the main camera
    //        var vecToObject = obj.transform.position - Camera.main.transform.position;
    //        //use the dot product to project the vector onto the camera's right axis
    //        var dist = Mathf.Abs(
    //            Vector3.Dot(
    //                vecToObject,
    //                Camera.main.transform.right));
    //        //compare the distance to camera and find the smallest one
    //        if (dist < closestToCameraLookDirection) {
    //            closestToCameraLookDirection = dist;
    //            gObject = obj;
    //        }
    //    }
    //    return gObject;
    //}

    //only allows one copy of each object
    //public void AddObjectToInRangeList(GrabbableObject tObject) {


    //    if (objectsInInteractRange.Contains(tObject)) return;

    //    objectsInInteractRange.Add(tObject);

    //}

    //public void RemoveObjectFromRangeList(GrabbableObject tObject) {

    //    objectsInInteractRange.Remove(tObject);
    //}


    //returns the object to pick up that is closest to the player transform
    //this behaviour might want to be changed later
    private TransferableObject GetObjectClosestTo2DPlayer() {

        var objectsInRange = Physics.OverlapSphere(PlayerBehaviour.Instance.player2D.transform.position, PlayerBehaviour.Instance.interactDisplayRadius, LayerMask.GetMask("Interactable Objects"));


        if (!objectsInRange.Any()) return null;

        float closestToPlayer = float.MaxValue;
        TransferableObject tObject = null;

        foreach (var objectCollider in objectsInRange) {
            if (objectCollider.transform.parent.TryGetComponent(out TransferableObject transferObject)) {
                var vecToObject = objectCollider.transform.position - transform.position;
                var length = vecToObject.sqrMagnitude;
                if (length < closestToPlayer) {
                    closestToPlayer = length;
                    tObject = transferObject;
                }
            }
        }
        return tObject;
    }


    public void ClearList() {
        objectsInInteractRange.Clear();
    }


}
