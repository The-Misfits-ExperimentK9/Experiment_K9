using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReceiver : ReceivableParent {
    [SerializeField] GameObject onLeds;
    [SerializeField] GameObject offLeds;
    [SerializeField] GameObject holo;
    public bool HoloIsOn = false;

    private GrabbableObject objectThatActivatedReciever;
   // GameObject currentHolo;

    bool isOn = false;
   // PlayerBehaviour player;

    [SerializeField] ActivatablePuzzlePiece puzzlePieceToActivate;

    void Update() {
        //if (PlayerBehaviour.Instance.is3D && PlayerBehaviour.Instance.pickupController.IsHoldingObject()) {
        //    if (PlayerBehaviour.Instance.GetClosestReciever() == gameObject) {
        //        ActivateHolo();
        //    }
        //    else if (PlayerBehaviour.Instance.GetClosestReciever() != gameObject) {
        //        DeactivateHolo();
        //    }
        //}
        //if (isOn && currentHolo != null)
        //    Destroy(currentHolo);

        if (objectThatActivatedReciever != null) {
            if (objectThatActivatedReciever == null && isOn)
                Deactivate();
            else if (objectThatActivatedReciever.IsBeingHeld)
                Deactivate();
            else if (objectThatActivatedReciever != null && !objectThatActivatedReciever.IsBeingHeld)
                Activate();
        }
    }
    public void ActivateHolo() {
        HoloIsOn = true;
        Debug.Log("activating holo");
        holo.SetActive(true);
    }
    public void DeactivateHolo() {
        Debug.Log("deactivating holo");
        HoloIsOn = false;
        holo.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            if (!isOn) {
                Activate();
                objectThatActivatedReciever = other.gameObject.GetComponentInParent<GrabbableObject>();
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            if (isOn) {
                objectThatActivatedReciever = null;
                Deactivate();
            }
        }
    }
    public override void Activate() {
        base.Activate();
        holo.SetActive(false);
        onLeds.SetActive(true);
        offLeds.SetActive(false);
        try {
            puzzlePieceToActivate.Activate();
        }
        catch {
            Debug.LogError("No puzzle piece to activate");
        }
        isOn = true;

    }
    public override void Deactivate() {
        base.Deactivate();
        onLeds.SetActive(false);
        offLeds.SetActive(true);
        try {
            puzzlePieceToActivate.Deactivate();
        }
        catch {
            Debug.LogError("No puzzle piece to activate");
        }
        isOn = false;
    }
}
