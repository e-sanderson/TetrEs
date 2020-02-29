using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour {

    // References
    private ParticleSystem particleEffect;
    private ParticleSystem.MainModule particleEffectMain;

    // Activates the particle effect at a given location using a particular color
    public void activate(Vector3 particleEffectLocation, Color32 particleEffectColor) {
        particleEffect = GetComponent<ParticleSystem>();
        particleEffectMain = particleEffect.main;
        transform.position = particleEffectLocation;
        particleEffectMain.startColor = new ParticleSystem.MinMaxGradient(particleEffectColor);
        gameObject.SetActive(true);
        particleEffect.Play();
    }

}
