using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : ReceivableParent {
    public bool IsLocked = false;
    [SerializeField] private ActivatablePuzzlePiece puzzlePieceToActivate;



    [SerializeField] private float springForce = 1f;
    private Rigidbody rb;
    [SerializeField] private MeshRenderer pawMeshRenderer;
    //list of 2 to replace both textures on the paw
    [SerializeField] private List<Material> pressedMaterials;
    [SerializeField] private List<Material> unPressedMaterials;
    //the minimum mass the button needs to be in contact with to activate
    [SerializeField] private float minMassToPress = 5f;
    //control to make the button not unpress itself if it was successfully pressed
    [SerializeField] private bool IsPermanentlyPressedOnPress = true;

    private bool unpressed;
    //keeps track of who or what pressed the button to check its mass against minimum required
    [SerializeField] private GameObject presser;

    // Start is called before the first frame update
    void Awake() {

        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        if (!IsPermanentlyPressedOnPress)
            UnPress();
        //fix for button ascending too high
        if (transform.localPosition.y > 0) {
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
    }



    //move the button up if it is not at its starting position
    void UnPress() {
        if (presser == null) {
            if (transform.localPosition.y < -0.01f) {
                unpressed = false;
                rb.AddForce(springForce * Time.deltaTime * Vector3.up, ForceMode.Acceleration);
            }
            //clear and reset everything to 0
            else if (!unpressed) {
                unpressed = true;
                var tempVec = transform.localPosition;
                tempVec.y = 0;
                transform.localPosition = tempVec;
                rb.velocity = Vector3.zero;
            }
        }

    }


    private void OnCollisionEnter(Collision collision) {
        // Debug.Log("collision " + collision.gameObject.layer);
        if (collision.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            presser = collision.gameObject;
        }
        //button hit the trigger cube
        //check if the button can be pressed and open door and swap material if so
        else if (collision.collider.CompareTag("EventTrigger")) {
            if (CanPressButton()) {
                Activate();
            }
        }
        //otherwise the collision was with something trying to press it so store it as the presser


    }
    //set materials to pressed and activate the puzzle piece
    public override void Activate() {
        base.Activate();
        pawMeshRenderer.SetMaterials(pressedMaterials);
        puzzlePieceToActivate.Activate();
    }
    //set materials back to unpressed and deactivate the puzzle piece
    public override void Deactivate() {
        pawMeshRenderer.SetMaterials(unPressedMaterials);
        puzzlePieceToActivate.Deactivate(gameObject);

        base.Deactivate();
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject == presser)
            presser = null;
        if (collision.collider.CompareTag("EventTrigger")) {
            Deactivate();
        }

    }
    //checks if the button can be pressed by checking presser null then for player presser
    //then checks for mass on the attached rigid body
    //if no rigid body is found also return false
    private bool CanPressButton() {
        return true;
        //if (presser == null) return false;
        //if (presser.layer == LayerInfo.PLAYER) return true;
        //if (presser.TryGetComponent(out Rigidbody rb)) {
        //    return rb.mass > minMassToPress;
        //}
        //return false;
    }
}
