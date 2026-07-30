using UnityEngine;

public class NPC_AnimationController : MonoBehaviour
{
    [SerializeField] CharacterController PlayerController;
    [SerializeField] Transform HeadPos;
    [SerializeField] Transform PlayerTransform;
    [SerializeField] Animator Animator;
    [SerializeField] AudioSource AudioSource;
    [SerializeField] GameObject Phone;

    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float lookWeightSpeed = 5f;

    public float Weight = 1f;
    public bool LookAtPlayer = false;

    private void Awake()
    {
        if (Animator == null) 
            Animator = GetComponent<Animator>();

        Call(0);
    }

    private void Start()
    {
        PlayerController = FindAnyObjectByType<CharacterController>();
        PlayerTransform = PlayerController.transform;
        HeadPos = Camera.main.transform;
    }

    private float currentWeight;

    private void OnAnimatorIK(int layerIndex)
    {
        if (Animator == null || PlayerTransform == null)
            return;

        if (!LookAtPlayer)
        {
            currentWeight = Mathf.MoveTowards(
                currentWeight,
                0f,
                lookWeightSpeed * Time.deltaTime);

            Animator.SetLookAtWeight(currentWeight);
            return;
        }

        Vector3 toPlayer = PlayerTransform.position - transform.position;
        toPlayer.y = 0f;

        float angle = Vector3.Angle(transform.forward, toPlayer);

        float targetWeight = 0f;

        if (angle <= maxLookAngle)
        {
            // 각도가 작을수록 Weight가 커짐
            targetWeight = Mathf.InverseLerp(maxLookAngle, 0f, angle);
        }

        currentWeight = Mathf.MoveTowards(
            currentWeight,
            targetWeight * Weight,
            lookWeightSpeed * Time.deltaTime);

        Animator.SetLookAtWeight(
            currentWeight, // 전체
            0.2f,          // body
            0.5f,          // head
            1f,            // eyes
            0.6f);         // clamp

        Animator.SetLookAtPosition(HeadPos.position);
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
