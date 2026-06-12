using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeakFire : MonoBehaviour, IMouseInteractable
{
    [Header("Scirpt")]
    [SerializeField] private Teleporter teleporter;
    [Header("Button")]
    [SerializeField] private Button fireButton;
    private Image fireButtonImage;

    [Header("Particle")]
    [SerializeField] private List<ParticleSystem> fires = new();

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Color")]
    [SerializeField] private Color originColor;
    [SerializeField] private Color hoverColor = Color.green;

    [Header("State")]
    [SerializeField] private bool isLeakPlaying = false;

    public bool IsPlaying => isLeakPlaying;
    public Button Button => fireButton;

    private void Awake()
    {
        if (fireButton != null)
        {
            fireButtonImage = fireButton.GetComponent<Image>();
            originColor = fireButtonImage.color;
        }

        StopLeak(true);
    }

    public void PlayLeak()
    {
        isLeakPlaying = true;

        for (int i = 0; i < fires.Count; i++)
        {
            if (fires[i] == null)
                continue;

            fires[i].Play();
        }

        if (animator != null)
            animator.SetBool("Toggle", true);
    }

    public void StopLeak(bool clear = false)
    {
        isLeakPlaying = false;

        for (int i = 0; i < fires.Count; i++)
        {
            if (fires[i] == null)
                continue;

            if (clear)
                fires[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                fires[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (animator != null)
            animator.SetBool("Toggle", false);
    }

    public void ToggleLeak()
    {
        if (isLeakPlaying)
            StopLeak(false);
        else
            PlayLeak();
    }

    public void HoverEnter()
    {
        if (fireButtonImage != null)
            fireButtonImage.color = hoverColor;
    }

    public void HoverExit()
    {
        if (fireButtonImage != null)
            fireButtonImage.color = originColor;
    }

    public void ClickEnter()
    {
       teleporter.RequestFireLeak();
    }

    public void ClickExit()
    {
    }

    public void ClickCancle()
    {
    }
}