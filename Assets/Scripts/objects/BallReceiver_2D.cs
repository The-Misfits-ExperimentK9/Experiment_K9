using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReceiver_2D : ReceivableParent {
    [SerializeField] GameObject outsideOff;
    [SerializeField] GameObject outsideOn;
    [SerializeField] ActivatablePuzzlePiece puzzlePieceToActivate;
    [SerializeField] bool Allow3DActivation = false;
    private GrabbableObject objectThatActivatedReciever;

    public override void Activate() {
        base.Activate();
        puzzlePieceToActivate.Activate();
        outsideOff.SetActive(false);
        outsideOn.SetActive(true);
    }
    public override void Deactivate() {
        base.Deactivate();
        puzzlePieceToActivate.Deactivate(gameObject);
        outsideOff.SetActive(true);
        outsideOn.SetActive(false);
    }
    private void OnTriggerEnter(Collider other) {
        //Debug.Log("2d ball receive hit " + other.name);
        //check object layer
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {

            //get the object trying to activate the switch
            if (TryGetTransferableObjectScript(other.gameObject, out TransferableObject tObject)) {
                //check if its 3d 
                if (tObject != null) {
                    Debug.Log("2d ball receive hit interactable object " + tObject.name);
                    //only activate if it is 3d and 3d activation is enabled or the object is 2d
                    if ((tObject.Is3D && Allow3DActivation) || !tObject.Is3D) {
                        objectThatActivatedReciever = tObject;
                        Activate();
                    }
                }
            }


        }
    }
    private void Update() {
        if (objectThatActivatedReciever != null && objectThatActivatedReciever.IsBeingHeld) {
            Deactivate();
            objectThatActivatedReciever = null;
            return;

        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            Deactivate();
        }
    }

    private bool TryGetTransferableObjectScript(GameObject interactableObject, out TransferableObject tObject) {
        if (interactableObject.layer != LayerInfo.INTERACTABLE_OBJECT) {
            tObject = null;
            return false;
        }
        if (interactableObject.TryGetComponent(out TransferableObject component)) {
            tObject = component;
            return true;
        }
        else if (interactableObject.transform.parent.TryGetComponent(out TransferableObject compontent1)) {
            tObject = compontent1;
            return true;
        }
        tObject = null;
        return false;

    }

}
