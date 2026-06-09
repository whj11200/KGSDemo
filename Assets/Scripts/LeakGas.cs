using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeakGas : MonoBehaviour, IMouseInteractable
{
    [Header("Button")]
    [SerializeField] Button Gas_B;
    Image Gas_B_image;
    [Header("Particle")]
    [SerializeField] List<ParticleSystem> smokes = new();
    [Header("Bools")]
    [SerializeField] bool isLeakPlaying = false;

    [Header("Color")]
    [SerializeField] Color orgincolor;
    [SerializeField] Color hovercolor;
    [Header("Animatior")]
    [SerializeField] Animator animator;

    private void Awake()
    {
        InitSmokes();
       
    }
    private void Start()
    {
        Gas_B.onClick.AddListener(ToggleAllSmokes);
        orgincolor = GetComponent<Image>().color;
        hovercolor = Color.green;
        Gas_B_image = Gas_B.GetComponent<Image>();
        StopAllSmokes(false);
        
    }


    private void InitSmokes()
    {
        if (smokes == null)
        {
            smokes = new List<ParticleSystem>();
        }

        if (smokes.Count == 0)
        {
            ParticleSystem[] childSmokes = GetComponentsInChildren<ParticleSystem>(true);
            smokes.AddRange(childSmokes);
        }

        StopAllSmokes(true);
    }



    public void ToggleAllSmokes()
    {
        isLeakPlaying = !isLeakPlaying;

        if (isLeakPlaying)
        {
            PlayAllSmokes();
            animator.SetBool("Toggle", true);
        }
        else
        {
            StopAllSmokes(false);
            animator.SetBool("Toggle", false);
        }
        Debug.Log("불");
    }

    public void PlayAllSmokes()
    {
        isLeakPlaying = true;

        for (int i = 0; i < smokes.Count; i++)
        {
            if (smokes[i] == null)
                continue;

            smokes[i].Play();
        }
        
    }

    public void StopAllSmokes(bool clear = false)
    {
        isLeakPlaying = false;

        for (int i = 0; i < smokes.Count; i++)
        {
            if (smokes[i] == null)
                continue;

            if (clear)
            {
                smokes[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                smokes[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    public void PlaySmoke(int index)
    {
        if (!IsValidSmokeIndex(index))
            return;

        smokes[index].Play();
    }

    public void StopSmoke(int index, bool clear = false)
    {
        if (!IsValidSmokeIndex(index))
            return;

        if (clear)
        {
            smokes[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            smokes[index].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void ToggleSmoke(int index)
    {
        if (!IsValidSmokeIndex(index))
            return;

        ParticleSystem smoke = smokes[index];

        if (smoke.isPlaying)
        {
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        else
        {
            smoke.Play();
        }
    }

    public void ToggleSmoke(int index, bool clearWhenStop)
    {
        if (!IsValidSmokeIndex(index))
            return;

        ParticleSystem smoke = smokes[index];

        if (smoke.isPlaying)
        {
            if (clearWhenStop)
            {
                smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        else
        {
            smoke.Play();
        }
    }

    private bool IsValidSmokeIndex(int index)
    {
        if (index < 0 || index >= smokes.Count)
        {
            Debug.LogWarning($"{name} : smokes[{index}]가 존재하지 않습니다.");
            return false;
        }

        if (smokes[index] == null)
        {
            Debug.LogWarning($"{name} : smokes[{index}]가 비어 있습니다.");
            return false;
        }

        return true;
    }

    public void ClickCancle()
    {
       
    }

    public void ClickEnter()
    {
        ToggleAllSmokes();
    }

    public void ClickExit()
    {

    }

    public void HoverEnter()
    {
        Gas_B_image.color = hovercolor;
    }

    public void HoverExit()
    {
       Gas_B_image.color = orgincolor;
    }
    
}
