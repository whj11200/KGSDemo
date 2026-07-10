using UnityEngine;

public class NPC_AnimationController : MonoBehaviour
{
    [SerializeField] CharacterController PlayerController;
    [SerializeField] Transform PlayerTransform;
    [SerializeField] Animator Animator;
    [SerializeField] AudioSource AudioSource;
    [SerializeField] GameObject Phone;

    public float Weight = 1f;
    public bool LookAtPlayer = false;

    private void Awake()
    {
        if (Animator == null) 
            Animator = GetComponent<Animator>();

        if (PlayerController == null || PlayerTransform == null)
        {
            PlayerController = FindAnyObjectByType<CharacterController>();
            PlayerTransform = PlayerController.transform;
        }

        Call(0);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (PlayerTransform == null) return;

        if (LookAtPlayer)
        {
            Animator.SetLookAtWeight(Weight, 0.1f, 1f, 1f, 0.4f);
            Animator.SetLookAtPosition(PlayerTransform.position);
        }
    }

    public void SetLookAtPlayer(bool value)
    {
        LookAtPlayer = value;
    }

    public void SetTrigger(string paramName)
    {
        Animator.SetTrigger(paramName);
        Phone.SetActive(true);
    }

    public void SetBool(string paramName, bool Value)
    {
        Animator.SetBool(paramName, Value);
    }

    public void PlayAudio(AudioClip clip)
    {
        if (clip == null) return;

        if (AudioSource.isPlaying)
        {
            AudioSource.Stop();
        }

        AudioSource.PlayOneShot(clip);
    }

    public void StopAudio()
    {
        if (AudioSource.isPlaying)
        {
            AudioSource.Stop();
        }
    }

    public void Call(int active)
    {
        Phone.SetActive(active == 1);
    }
}
