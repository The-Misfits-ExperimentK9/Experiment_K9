using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReceivableParent : MonoBehaviour {

    [SerializeField] GlowPath glowPath;
    [SerializeField] AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    public bool IsActivated { get; protected set; } = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = audioClip;
    }

    public virtual void Activate() {
        IsActivated = true;
        if (glowPath != null)
        {
            glowPath.Activate();
        }
        if (audioClip != null)
        {
            audioSource.Play();
        }
    }
    public virtual void Deactivate() {
        IsActivated = false;
        if (glowPath != null) {
            glowPath.Deactivate();
        }
    }

}
