using UnityEngine;
using System.Collections;

public class HazeControl : MonoBehaviour
{
    [SerializeField] private ParticleSystem hazeParticleSystem;
    [SerializeField] private AudioSource hazesound;

    private Coroutine hazeCoroutine;

    
    private void OnEnable()
    {
        hazesound = GetComponent<AudioSource>();
        hazeParticleSystem = GetComponent<ParticleSystem>();

        StopHaze();
    }

    

    public void StartHaze()
    {
        if (hazeCoroutine != null)
        {
            StopCoroutine(hazeCoroutine);
            hazeCoroutine = null;
        }

        hazeCoroutine = StartCoroutine(HazeCoroutine());
    }

    public void StopHaze()
    {
        if (hazeCoroutine != null)
        {
            StopCoroutine(hazeCoroutine);
            hazeCoroutine = null;
        }

        if (hazeParticleSystem != null)
            hazeParticleSystem.Stop();

        if (hazesound != null)
            hazesound.Stop();
    }

    private IEnumerator HazeCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (hazeParticleSystem != null)
            hazeParticleSystem.Play();

        if (hazesound != null)
            hazesound.Play();

        hazeCoroutine = null;
    }
}