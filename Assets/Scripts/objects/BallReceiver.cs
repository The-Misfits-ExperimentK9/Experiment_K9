using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReceiver : ReceivableParent {
    [SerializeField] GameObject onLeds;
    [SerializeField] GameObject offLeds;
    [SerializeField] GameObject holo;

    private GrabbableObject objectThatActivatedReciever;
    GameObject currentHolo;

    bool isOn = false;
    PlayerBehaviour player;

    [SerializeField] ActivatablePuzzlePiece puzzlePieceToActivate;

    void Update()
    {
        if (player == null)
            player = PlayerBehaviour.Instance;
        if (player.is3D && player.pickupController.IsHoldingObject())
            if (player.GetClosestReciever() == gameObject && currentHolo == null)
                currentHolo = Instantiate(holo, transform.position + transform.up, Quaternion.identity);
            else if (player.GetClosestReciever() != gameObject && currentHolo != null)
                Destroy(currentHolo);
        if (isOn && currentHolo != null)
            Destroy(currentHolo);

        if (objectThatActivatedReciever == null)
            Deactivate();
        else if (objectThatActivatedReciever.IsBeingHeld)
            Deactivate();
        else if (objectThatActivatedReciever != null && !objectThatActivatedReciever.IsBeingHeld)
            Activate();
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
        objectThatActivatedReciever = null;
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            if (isOn) {
                Deactivate();
            }
        }
    }
    public override void Activate() {
        base.Activate();
        onLeds.SetActive(true);
        offLeds.SetActive(false);
        puzzlePieceToActivate.Activate();
        isOn = true;

    }
    public override void Deactivate() {
        base.Deactivate();
        onLeds.SetActive(false);
        offLeds.SetActive(true);
        puzzlePieceToActivate.Deactivate();
        isOn = false;
    }
}
