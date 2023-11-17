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
    
    [SerializeField] private LayerMask interactableLayerMask; //layer mask for the interactable objects

    private void Awake() {
        interactKey = Keyboard.current.eKey;
        objectsInInteractRange = new();
    }



    private void Update() {
        if (canInteract)
            HandleInteractionInput();

        
    }
    private void FixedUpdate() {
        if (IsHoldingObject() && PlayerBehaviour.Instance.IsIn3D()) {
            MoveObject();
        }
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
            heldObjectRigidbody.AddForce(moveDirection * pickupForce, ForceMode.Acceleration);
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
        var closestGOToCamera = PlayerBehaviour.Instance.GetClosest3DObjectOnLayers(interactableLayerMask);

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
    //returns the object to pick up that is closest to the player transform
    //this behaviour might want to be changed later
    private TransferableObject GetObjectClosestTo2DPlayer() {

        var objectsInRange = Physics.OverlapSphere(PlayerBehaviour.Instance.player2D.transform.position, PlayerBehaviour.Instance.interactDisplayRadius, interactableLayerMask);


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
