using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerNarration : MonoBehaviour
{
    public AudioClip narrationClip; 
    private AudioSource audioSource;
    [SerializeField] private bool onlyPlayOnce = true;
    [SerializeField] private ActivatablePuzzlePiece puzzlePiece;

    void Start() {
        if (!narrationClip) {
            Debug.LogError("No narration clip assigned to " + gameObject.name);
        }
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = narrationClip;
        audioSource.spatialBlend = 0; 
        audioSource.playOnAwake = false;
    }
    private void Update() {
        if (onlyPlayOnce) {
            if (!audioSource.isPlaying && audioSource.time > 0) {
                //hide the object
                puzzlePiece.Activate();
                gameObject.SetActive(false); 
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.PLAYER) {
            PlayNarration();
        }
    }

    private void PlayNarration() {
        //show the object
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }
    public void StopNarration() {
        if (audioSource.isPlaying) {
            puzzlePiece.Activate();
            audioSource.Stop();
        }
    }
}
