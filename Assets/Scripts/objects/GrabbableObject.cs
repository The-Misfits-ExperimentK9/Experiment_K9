using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrabbableObject : MonoBehaviour {
    [SerializeField] protected ObjectInteractDisplayController interactDisplayController;
    public MeshRenderer displayObject3D_Mesh;
  //  public Vector3 HoldOffset3D = new(0, -2.5f, 8);

    public bool IsBeingHeld = false;
    public bool Is3D = true;
    protected GameObject holder;
    public bool isColliding;
    // Start is called before the first frame update
    void Start() {
        
        Is3D = true;

    }
    // Call this method to set the layer of a GameObject and all of its children
    public void SetLayerRecursively(GameObject obj, int newLayer) {
        if (obj == null) {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform) {
            if (child == null)
                continue;

            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    //handles picking up the object when in 3d
    public virtual void Pickup3D(GameObject holder, Transform holdArea, float dragAmount) {
     //   displayObject3D_Mesh.gameObject.layer = LayerInfo.PHYSICS_DISBALE;
        //get the rigid body, disable gravity, set drag to 10, freeze rotation, set parent to the hold area
        var rb3D = displayObject3D_Mesh.GetComponent<Rigidbody>();
        rb3D.useGravity = false;
        rb3D.drag = dragAmount;
        rb3D.constraints = RigidbodyConstraints.FreezeRotation;
        IsBeingHeld = true;
        //displayObject3D_Mesh.transform.parent = holdArea
        transform.parent = holdArea;

        //disable interaction indicator
        interactDisplayController.SetInteractIndicatorActive(false);
        this.holder = holder;


    }
    public virtual void DropObject() {
        Drop3D();
    }
    protected virtual void Drop3D() {
        //reset the rigid body, enable gravity, set drag to 1, unfreeze rotation, set parent to null
        var rb3D = displayObject3D_Mesh.GetComponent<Rigidbody>();
        var sphereCollider = displayObject3D_Mesh.GetComponent<SphereCollider>();
       
        //displayObject3D_Mesh.gameObject.layer = LayerInfo.INTERACTABLE_OBJECT;

        //prevent errors with sphere
        if (displayObject3D_Mesh.name != "actual_cube")
        {
            Debug.Log("dropping sphere");
            sphereCollider.enabled = true;
            if (PlayerBehaviour.Instance.GetClosestReceiver() != null)
                transform.position = PlayerBehaviour.Instance.GetClosestReceiver().transform.position;
            else
                transform.position = displayObject3D_Mesh.transform.position;
            //  sphere.excludeLayers = LayerMask.GetMask("Nothing");
            //  displayObject3D_Mesh.transform.parent = transform;
        }
        transform.localPosition = Vector3.zero;
        transform.parent = null;
        displayObject3D_Mesh.transform.localPosition = Vector3.zero;
        
        rb3D.useGravity = true;
        rb3D.drag = 1;
        rb3D.constraints = RigidbodyConstraints.None;
        interactDisplayController.SetInteractIndicatorActive(true);
        holder = null;
        IsBeingHeld = false;
        
    }
}
