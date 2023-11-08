using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressButtonBehaviour : ReceivableParent {
    [SerializeField] private ActivatablePuzzlePiece activatablePuzzlePiece;
    [SerializeField] private GameObject button;
    private float pressAmount;
    private float pressTimer = .5f;
    private float pressDelay = 1f;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    //set materials to pressed and activate the puzzle piece
    protected override void Activate() {
        StartCoroutine(PressButton(true));
        base.Activate();

    }
    //set materials back to unpressed and deactivate the puzzle piece
    protected override void Deactivate() {

        StartCoroutine(PressButton(false));
        base.Deactivate();
    }

    IEnumerator PressButton(bool press) {

        for (float t = 0; t < pressTimer; t += Time.deltaTime) {
            if (press) {
                pressAmount = Mathf.Lerp(0, 1, t / pressTimer);
            }
            else {
                pressAmount = Mathf.Lerp(1, 0, t / pressTimer);
            }
            button.transform.localPosition = new Vector3(0, -pressAmount, 0);
            yield return null;
        }
        if (press) {
            yield return new WaitForSeconds(pressDelay);
            Deactivate();

        }
    }

}
