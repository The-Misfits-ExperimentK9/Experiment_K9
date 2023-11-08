using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSpawnerDoorOpener : MonoBehaviour {
    [Header("Doors")]
    [SerializeField]
    GameObject door1;
    [SerializeField]
    GameObject door2;

    [Header("Rotation Settings")]
    [SerializeField] float rotationDuration;
    
  //  [SerializeField] float rotationAmount = 90f;
    // [SerializeField] float openRotation = 90f;

    [SerializeField]
    Vector3 door1StartRotation;
    [SerializeField]
    Vector3 door2StartRotation;
    [SerializeField]
    Vector3 door1EndRotation;
    [SerializeField]
    Vector3 door2EndRotation;

    [Header("Time Settings")]
    [SerializeField] float closeDelay = 1f;


    public void OpenDoors() {
        StartCoroutine(OpenOrCloseDoors(true));
    }
    public void CloseDoors() {
        StartCoroutine(OpenOrCloseDoors(false));
    }


    //rotates the door objects door1 rotates to + openRotation.x and door2 opens to -openRotation.x
    IEnumerator OpenOrCloseDoors(bool open) {
        Quaternion startRotation1, startRotation2, endRotation1, endRotation2;
        startRotation1 = Quaternion.Euler(door1StartRotation);
        startRotation2 = Quaternion.Euler(door2StartRotation);
        endRotation1 = Quaternion.Euler(door1EndRotation);
        endRotation2 = Quaternion.Euler(door2EndRotation);

        //swap the start and end rotations if we are closing the doors
        if (!open) {
            (startRotation1, endRotation1) = (endRotation1, startRotation1);
            (startRotation2, endRotation2) = (endRotation2, startRotation2);
        }

        

        for (float t = 0; t < rotationDuration; t += Time.deltaTime) {
            door1.transform.rotation = Quaternion.Lerp(startRotation1, endRotation1, t / rotationDuration);
            door2.transform.rotation = Quaternion.Lerp(startRotation2, endRotation2, t / rotationDuration);
            yield return null;
        }

        door1.transform.rotation = endRotation1;
        door2.transform.rotation = endRotation2;

        if (open) {
            yield return new WaitForSeconds(closeDelay);
            CloseDoors();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            OpenDoors();
        }
    }



}
