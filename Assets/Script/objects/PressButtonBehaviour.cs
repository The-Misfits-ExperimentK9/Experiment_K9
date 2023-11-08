using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressButtonBehaviour : ReceivableParent {
    [SerializeField] private ActivatablePuzzlePiece activatablePuzzlePiece;
    [SerializeField] private GameObject button;
    private readonly float pressAmount = .3f;
    private readonly float pressTimer = .1f;
    private readonly float pressDelay = .5f;
    private bool isPressed = false;
    

    //set materials to pressed and activate the puzzle piece
    public override void Activate() {
        if (!isPressed) {
            isPressed = true;
            StartCoroutine(PressButton(true));
            base.Activate();
            activatablePuzzlePiece.Activate();
        }

    }
    //set materials back to unpressed and deactivate the puzzle piece
    public override void Deactivate() {

        StartCoroutine(PressButton(false));
        base.Deactivate();
    }

    IEnumerator PressButton(bool press) {

        Vector3 endPos = button.transform.localPosition;

        if (press)
            endPos -= new Vector3(0, pressAmount, 0);
        else
            endPos += new Vector3(0, pressAmount, 0);

        for (float t = 0; t < pressTimer; t += Time.deltaTime) {


            button.transform.localPosition = Vector3.Lerp(button.transform.localPosition, endPos, t / pressTimer);

            yield return null;
        }
        if (press) {
            yield return new WaitForSeconds(pressDelay);
            Deactivate();

        }
        else {
            isPressed = false;
        }
    }

}
