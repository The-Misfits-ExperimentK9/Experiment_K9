using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractRadarController : MonoBehaviour {
    [SerializeField] private List<Collider> potentialProjectionSurfaces = new();
    private Collider currentProjectionSurface;

    private Vector3 gizmoDrawLoc;

    private void Update() {
        if (!PlayerBehaviour.Instance.IsIn3D() || !PlayerBehaviour.Instance.playerDimensionController.DOGEnabled) {
            PlayerBehaviour.Instance.playerDimensionController.DisableProjections();
            return;
        }
        CheckForPotentialSurfaces();
    }
    
    //Checks through potentialSurfaces list to see if there are any to project onto
    //calls helper methods to handle one of multiple surfaces nearby
    //disables projections if there are no surfaces found
    private void CheckForPotentialSurfaces() {
        //theres at least 1 wall to project to
        if (potentialProjectionSurfaces.Any()) {
            PlayerBehaviour.Instance.playerDimensionController.HandleSurfaceProjection(potentialProjectionSurfaces);

            //if (potentialProjectionSurfaces.Count == 1) {
            //    HandleOneSurfaceNearby();
            //}
            ////more than 1 potential surface need to find the closest one to the player
            //else {
            //    HandleMultipleSurfacesNearby();
            //}
        }
        //no surface was found so disable all projections
        else {
            PlayerBehaviour.Instance.playerDimensionController.DisableProjections();
        }
    }

    private void OnTriggerEnter(Collider other) {
        //found an object the player can interact with so add it to the pickupController list
        //if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
        //    var tGObject = other.transform.parent;
        //    if (tGObject == null) {
        //        tGObject = other.transform;
        //    }
        //    if (tGObject.TryGetComponent(out GrabbableObject grabbableObject)) {
        //        PlayerBehaviour.Instance.pickupController.AddObjectToInRangeList(grabbableObject);
        //    }
        //}
        //tell projection to enable
        if (other.gameObject.layer == LayerInfo.WALL) {

            if (potentialProjectionSurfaces.Contains(other)) return;
            potentialProjectionSurfaces.Add(other);
        }
    }
    private void OnTriggerExit(Collider other) {
        //found an object the player can interact with so remove it from pickupController list
        //if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
        //    var tGObject = other.transform.parent;
        //    if (tGObject == null) {
        //        tGObject = other.transform;
        //    }
        //    Debug.Log(tGObject.name);
        //    if (tGObject.TryGetComponent(out GrabbableObject tObject)) {
        //        PlayerBehaviour.Instance.pickupController.RemoveObjectFromRangeList(tObject);
        //    }
        //}
        //tell projection to disasble
        if (other.gameObject.layer == LayerInfo.WALL) {
            if (other.gameObject.GetComponent<WallBehaviour>().AllowsDimensionTransition) {
                potentialProjectionSurfaces.Remove(other);
            }
        }
    }
    public void clearsurfaces() {
        potentialProjectionSurfaces.Clear();
    }

}
