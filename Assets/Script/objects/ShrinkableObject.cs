using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkableObject : GrabbableObject {
    [Header("Shrink Settings")]
    public float unShrunkScale;
    public float shrinkScale;
    public float shrinkTime;
    protected override void Drop3D() {
        base.Drop3D();
        StartCoroutine(ChangeScale(unShrunkScale));
        //ChangeScaleInstant(1);
    }
    public override void Pickup3D(GameObject holder, Transform holdArea) {
        base.Pickup3D(holder, holdArea);
        StartCoroutine(ChangeScale(shrinkScale));
        // ChangeScaleInstant(.2f);
    }
    private void ChangeScaleInstant(float targetScale) {

        displayObject3D_Mesh.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
    }

    private IEnumerator ChangeScale(float targetScale) {
        float currentTime = 0;
        float startScale = displayObject3D_Mesh.transform.localScale.x;
        while (currentTime < shrinkTime) {
            currentTime += Time.deltaTime;
            float newScale = Mathf.Lerp(startScale, targetScale, currentTime / shrinkTime);
            displayObject3D_Mesh.transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;
        }
        displayObject3D_Mesh.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
    }
}
