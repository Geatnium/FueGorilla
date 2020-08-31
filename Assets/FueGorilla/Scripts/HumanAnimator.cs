using UnityEngine;

public class HumanAnimator : MonoBehaviour
{
    private Animator animator; // アニメーター
    private CharacterController cc;
    private HumanManager humanManager;
    private HumanGrabing humanGrabing;
    public AnimatorStateInfo stateInfoBase, stateInfoUpper;

    private AudioSource audioSource;
    [SerializeField] private AudioClip footStepClip, jumpClip, landClip;

    private void Start()
    {
        animator = GetComponent<Animator>();
        humanManager = GetComponent<HumanManager>();
        cc = GetComponent<CharacterController>();
        humanGrabing = GetComponent<HumanGrabing>();
        audioSource = GetComponent<AudioSource>();
    }

    public void UpdateM()
    {
        stateInfoBase = animator.GetCurrentAnimatorStateInfo(0);
        stateInfoUpper = animator.GetCurrentAnimatorStateInfo(1);
        // アニメーターにパラメーターを送る
        animator.SetFloat("Velocity", new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude);
        animator.SetBool("Ground", humanManager.isGround);
        animator.SetFloat("Velocity_Up", cc.velocity.y);
        animator.SetBool("Grab", humanGrabing.isGrab);
    }

    public void JumpAnimTrigger()
    {
        animator.SetTrigger("Jump");
    }

    public void KnockBackTrigger()
    {
        animator.SetTrigger("Knock");
    }

    public void OnFootStepSound()
    {
        audioSource.PlayOneShot(footStepClip);
    }

    public void OnJumpSound()
    {
        audioSource.PlayOneShot(jumpClip);
    }

    public void OnLandSound()
    {
        audioSource.PlayOneShot(landClip);
    }
}
