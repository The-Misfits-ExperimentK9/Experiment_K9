using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player3DHitboxTrigger : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.TryGetComponent(out WallBehaviour wallB)) {
            if (wallB.AllowsDimensionTransition && PlayerBehaviour.Instance.IsIn3D() && PlayerBehaviour.Instance.playerDimensionController.DOGEnabled) {
                PlayerBehaviour.Instance.playerDimensionController.TryTransitionTo2D();

            }
        }
    }
    private void OnTriggerEnter(Collider other) {
    }
    private void OnCollisionExit(Collision collision) {
       
    }
}
