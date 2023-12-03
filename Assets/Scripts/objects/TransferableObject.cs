using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferableObject : GrabbableObject {

    [SerializeField] private GameObject displayObject_2D;
    [SerializeField] private SpriteRenderer spriteRenderer2D;
    [SerializeField] private float objectDrawOffset = 4f;
    [SerializeField] private SphereCollider sphere;
    [SerializeField] private bool inAir = false;
    [SerializeField] private AudioClip lowDrop;
    [SerializeField] private AudioClip mediumDrop;
    [SerializeField] private AudioClip highDrop;
    [SerializeField] private AudioSource audioSource;

    private void Awake() {
        spriteRenderer2D = displayObject_2D.GetComponent<SpriteRenderer>();
        sphere = displayObject3D_Mesh.GetComponent<SphereCollider>();
        //turn off the physics if the object starts as 2D
        //interactDisplayController.SetInteractIndicatorActive(true);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (!Is3D) {

            Disable3D();
        }
    }
    private void Update() {
        if (!Is3D) {
            //if (displayObject_2D.transform.localPosition != Vector3.zero) {
            //reset position of sphere and this objec to the location of the 2d sprite while in 2d mode
            //reset position of the 3d object to the 2d object location
            transform.position = displayObject_2D.transform.position;
            displayObject3D_Mesh.transform.position = displayObject_2D.transform.position;
            displayObject_2D.transform.localPosition = Vector3.zero;

            // }
        }
    }

    public void Set3DDisplayMode(bool is3D) {
        Is3D = is3D;

    }
    public void Enable3D() {
        //sphere.enabled = false;
        sphere.excludeLayers = LayerMask.GetMask("Player");
        Set3DDisplayMode(true);
        displayObject3D_Mesh.enabled = true;
    }
    public void Enable2D() {

        displayObject_2D.SetActive(true);

    }
    public void Disable3D() {
        //TogglePhysics(disable: true);
        //turn off colliders so it doesn't drag theplayer

        sphere.enabled = false;
        // sphere.excludeLayers = LayerMask.GetMask("Player");
        displayObject3D_Mesh.enabled = false;
        Is3D = false;
    }
    public void Disable2D() {
        displayObject_2D.SetActive(false);
        Is3D = true;
    }
    public void SetHolder(GameObject holder) {
        this.holder = holder;
        transform.parent = holder.transform;
        transform.localPosition = Vector3.zero;
    }


    public void SetHoldArea(GameObject holder, Transform holdArea) {
        Debug.Log("setting hold: " + holder.name);

        this.holder = holder;
        transform.parent = holdArea;
        transform.position = holdArea.position;
        //transform.localPosition = offset;
    }

    public void Pickup2D(GameObject holder) {

        displayObject3D_Mesh.enabled = false;
        //set the hold as the parent to carry it around
        transform.SetParent(holder.transform);
        spriteRenderer2D.enabled = false;
        sphere.enabled = false;
        //  sphere.excludeLayers = LayerMask.GetMask("Player");
        //disable interaction indicator
        interactDisplayController.SetInteractIndicatorActive(false);
        this.holder = holder;

        transform.localPosition = Vector3.zero;

        IsBeingHeld = true;
    }
    public override void DropObject() {
        Debug.Log("transferable object drop object");
        if (Is3D)
            Drop3D();
        else
            Drop2D();
    }

    // This method checks when a dropped object hits the ground.
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (inAir)
        {
            // If the object hits the ground....
            if (collision.gameObject.CompareTag("Ground"))
            {
                // The inAir bool is set to false, first and foremost.
                inAir = false;
            }

            // The magnitude of RigidBody.velocity determines which of the three clips should play.
            if (rb.velocity.magnitude < 5.0f)
            {
                PlaySound(lowDrop);
            }
            else if (rb.velocity.magnitude >= 5.0f && rb.velocity.magnitude < 10.0f)
            {
                PlaySound(mediumDrop);
            }
            else
            {
                PlaySound(highDrop);
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }


    public void Drop2D() {
        Debug.Log("Dropping 2d object");

        transform.forward = holder.transform.forward;

        //calculate an offset based on the players right axis
        bool facingRight = holder.GetComponent<MovementController_2D>().IsFlipped();
        var offset = (facingRight ? objectDrawOffset : -objectDrawOffset) * holder.transform.right;

        //  displayObject_2D.SetActive(true);   
        //  spriteRenderer2D.enabled = true;

        sphere.enabled = true;
        sphere.excludeLayers = LayerMask.GetMask("Walls");

        holder = null;
        IsBeingHeld = false;
        transform.parent = null;
        transform.position += offset;
        displayObject3D_Mesh.enabled = true;
        interactDisplayController.SetInteractIndicatorActive(true);
        interactDisplayController.ResetPosition();
    }

}
