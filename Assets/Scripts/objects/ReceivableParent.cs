using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReceivableParent : MonoBehaviour {

    [SerializeField] GlowPath glowPath;
    [SerializeField] AudioSource audioSource;
    public bool IsActivated { get; protected set; } = false;


    public virtual void Activate() {
        IsActivated = true;
        if (glowPath != null) {
            glowPath.Activate();
        }
        if (audioSource != null) {
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
