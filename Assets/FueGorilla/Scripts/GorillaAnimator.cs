using UnityEngine;

public class GorillaAnimator : MonoBehaviour
{
    private Animator animator; // アニメーター
    private CharacterController cc;
    private GorillaManager gorillaManager;
    public AnimatorStateInfo stateInfo;

    private AudioSource audioSource;
    [SerializeField] private AudioClip footStepClip, jumpClip, attackClip, chestHitClip;

    private void Start()
    {
        animator = GetComponent<Animator>();
        gorillaManager = GetComponent<GorillaManager>();
        cc = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    public void UpdateM()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // アニメーターにパラメーターを送る
        animator.SetFloat("Velocity", new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude);
        animator.SetBool("Ground", gorillaManager.isGround);
        animator.SetFloat("Velocity_Up", cc.velocity.y);
    }

    public void AttackAnimTrigger()
    {
        animator.SetTrigger("Attack");
    }

    public void JumpAnimTrigger()
    {
        animator.SetTrigger("Jump");
    }

    public void KnockBackTrigger()
    {
        animator.SetTrigger("Knock");
    }

    public void OnFootSound()
    {
        audioSource.PlayOneShot(footStepClip);
    }

    public void OnJumpSound()
    {
        audioSource.PlayOneShot(jumpClip);
    }

    public void OnAttackSound()
    {
        audioSource.PlayOneShot(attackClip);
    }

    public void OnChestHitSound()
    {
        audioSource.PlayOneShot(chestHitClip);
    }
}
