using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerNarration : MonoBehaviour
{
    public AudioClip narrationClip; 
    private AudioSource audioSource;
    [SerializeField] private bool onlyPlayOnce = true;
    [SerializeField] private ActivatablePuzzlePiece puzzlePieceToActivate;

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
                if (puzzlePieceToActivate)
                    puzzlePieceToActivate.Activate();
                gameObject.SetActive(false); 
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.PLAYER) {
            if (PlayerBehaviour.Instance.TutorialEnabled)
                PlayNarration();
            else {
                if (puzzlePieceToActivate)
                    puzzlePieceToActivate.Activate();
            }
        }
    }

    private void PlayNarration() {
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }
    public void StopNarration() {
        audioSource.Stop();
    }
}
