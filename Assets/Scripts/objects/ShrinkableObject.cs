using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkableObject : GrabbableObject {
    [Header("Shrink Settings")]
    public float unShrunkScale;
    public float shrinkScale;
    public float shrinkTime;
    [SerializeField] private bool inAir = false;
    [SerializeField] private AudioClip lowDrop;
    [SerializeField] private AudioClip mediumDrop;
    [SerializeField] private AudioClip highDrop;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    protected override void Drop3D() {
        base.Drop3D();
        inAir = true;
        StartCoroutine(ChangeScale(unShrunkScale));
        //ChangeScaleInstant(1);
    }
    public override void Pickup3D(GameObject holder, Transform holdArea, float dragAmount) {
        base.Pickup3D(holder, holdArea, dragAmount);
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
}
