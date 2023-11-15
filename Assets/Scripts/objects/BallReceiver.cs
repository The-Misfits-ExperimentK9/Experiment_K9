using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReceiver : ReceivableParent {
    [SerializeField] GameObject onLeds;
    [SerializeField] GameObject offLeds;

    bool isOn = false;

    [SerializeField] ActivatablePuzzlePiece puzzlePieceToActivate;


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            if (!isOn) {
                Activate();
            }
        }
    }
    private void OnTriggerExit(Collider other) {
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
        puzzlePieceToActivate.Deactivate(gameObject);
        isOn = false;
    }
}
