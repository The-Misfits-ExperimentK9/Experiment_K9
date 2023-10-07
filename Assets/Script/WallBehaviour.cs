using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBehaviour : MonoBehaviour {
    //to check if the player can enter or leave 2d/3d space using this wall
    public bool AllowsDimensionTransition = false;
    //to check if the player can move into this object or not
    public bool IsWalkThroughEnabled = true;
    //to check if the player can pass through the wall while in 2D
    public bool IsPassthrough = false;
    //remove from all checks for player movement
    public bool RemoveFromWalkChecks = false;

    public float pushForce = 1f;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.layer == LayerInfo.PLAYER) {
            other.GetComponent<Rigidbody>().AddForce(other.transform.forward * pushForce);
        }
    }
}
