using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeakGas : MonoBehaviour, IMouseInteractable
{
    [Header("Scirpt")]
    [SerializeField] private Teleporter teleporter;
    [Header("Button")]
    [SerializeField] private Button gasButton;
    private Image gasButtonImage;

    [Header("Particle")]
    [SerializeField] private List<ParticleSystem> smokes = new();

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Color")]
    [SerializeField] private Color originColor;
    [SerializeField] private Color hoverColor = Color.green;

    [Header("State")]
    [SerializeField] private bool isLeakPlaying = false;

    public bool IsPlaying => isLeakPlaying;
    public Button Button => gasButton;

    private void Awake()
    {
        if (gasButton != null)
        {
            gasButtonImage = gasButton.GetComponent<Image>();
            originColor = gasButtonImage.color;
        }

        StopLeak(true);
    }

    public void PlayLeak()
    {
        isLeakPlaying = true;

        for (int i = 0; i < smokes.Count; i++)
        {
            if (smokes[i] == null)
                continue;

            smokes[i].Play();
        }

        if (animator != null)
            animator.SetBool("Toggle", true);
    }

    public void StopLeak(bool clear = false)
    {
        isLeakPlaying = false;

        for (int i = 0; i < smokes.Count; i++)
        {
            if (smokes[i] == null)
                continue;

            if (clear)
                smokes[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                smokes[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
        if (gasButtonImage != null)
            gasButtonImage.color = hoverColor;
    }

    public void HoverExit()
    {
        if (gasButtonImage != null)
            gasButtonImage.color = originColor;
    }

    public void ClickEnter()
    {
       teleporter.RequestGasLeak();
    }

    public void ClickExit()
    {
    }

    public void ClickCancle()
    {
    }
}