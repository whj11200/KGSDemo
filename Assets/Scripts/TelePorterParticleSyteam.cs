using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TelePorterParticleSyteamw : MonoBehaviour, IMouseInteractable
{
    [Header("Particle")]
    [SerializeField] Button Fire_B;

    [SerializeField] private List<ParticleSystem> smokes = new();
    [SerializeField] private bool isSmokePlaying = false;



    private void Awake()
    {
        InitSmokes();
       
    }
    private void Start()
    {
        Fire_B.onClick.AddListener(ToggleAllSmokes);
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
        isSmokePlaying = !isSmokePlaying;

        if (isSmokePlaying)
        {
            PlayAllSmokes();
        }
        else
        {
            StopAllSmokes(false);
        }
        Debug.Log("불");
    }

    public void PlayAllSmokes()
    {
        isSmokePlaying = true;

        for (int i = 0; i < smokes.Count; i++)
        {
            if (smokes[i] == null)
                continue;

            smokes[i].Play();
        }
        
    }

    public void StopAllSmokes(bool clear = false)
    {
        isSmokePlaying = false;

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

    }

    public void HoverExit()
    {

    }
}
