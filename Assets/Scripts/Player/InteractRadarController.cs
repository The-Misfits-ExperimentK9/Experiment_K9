using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractRadarController : MonoBehaviour {

    private void Update() {
        //if (!PlayerBehaviour.Instance.IsIn3D() || !PlayerBehaviour.Instance.playerDimensionController.DOGEnabled) {
        //    PlayerBehaviour.Instance.playerDimensionController.DisableProjections();
        //    return;
        //}
    }

    private void OnTriggerEnter(Collider other) {
        //tell projection to enable
        if (other.gameObject.layer == LayerInfo.WALL) {

            PlayerBehaviour.Instance.playerDimensionController.AddWallToPotentialSurfaces(other);
        }
        else if (other.CompareTag("BallReceiver")) {
            PlayerBehaviour.Instance.AddReceivableToList(other);
        }
    }
    private void OnTriggerExit(Collider other) {
        //tell projection to disasble
        if (other.gameObject.layer == LayerInfo.WALL) {
            PlayerBehaviour.Instance.playerDimensionController.RemoveWallFromPotentialSurfaces(other);  
        }
        else if (other.CompareTag("BallReceiver")) {
            PlayerBehaviour.Instance.RemoveReceivableFromList(other);
        }
    }
    

}
