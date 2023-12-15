using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerNarration : MonoBehaviour
{
    public AudioClip narrationClip; 
    private AudioSource audioSource;
    [SerializeField] private bool onlyPlayOnce = true;

    [SerializeField] private ActivatablePuzzlePiece puzzlePiece;
    [SerializeField] private List<GameObject> canvases;
    [SerializeField] private List<float> timeStamps;

    int index = 0;
    float prevTime;

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
            if (timeStamps.Count > 0 && index < timeStamps.Count)
            {
                if (audioSource.time >= timeStamps[index] && prevTime <= timeStamps[index])
                {
                    NextCanvas();
                }
            }

            if (!audioSource.isPlaying && audioSource.time > 0) {
                for (int x = 0; x < canvases.Count; x++)
                {
                    canvases[x].SetActive(false);
                }
                Debug.Log("Narration finished");
                if (puzzlePiece != null) puzzlePiece.Activate();
                gameObject.SetActive(false);
                
            }
        }

        prevTime = audioSource.time;
    }

    private void NextCanvas()
    {
        if (index + 1 <= canvases.Count - 1)
        {
            index += 1;
            canvases[index].SetActive(true);
            for (int x = 0; x < canvases.Count; x++)
            {
                if (x != index)
                {
                    canvases[x].SetActive(false);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.PLAYER) {
            PlayNarration();
            canvases[0].SetActive(true);
        }
    }

    private void PlayNarration() {
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }

    public void StopNarration()
    {
        audioSource.Stop();
        if (puzzlePiece != null) puzzlePiece.Activate();
        gameObject.SetActive(false);
    }
}
