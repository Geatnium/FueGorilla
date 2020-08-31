using UnityEngine;

public class PlayerHumanMover : MonoBehaviour
{
    /// <summary>
    /// リープ用の移動ベクトル
    /// </summary>
    //[HideInInspector] public Vector3 currentMove;

    private Transform camTr; // カメラのトランスフォーム
    private CharacterController controller; // キャラクターコントローラー

    /// <summary>
    /// 移動速度や加速度など、プレイヤーの設定が入っている
    /// </summary>
    private PlayerConfig playerConfig;

    private HumanManager humanManager;
    private HumanAnimator humanAnimator;
    private HumanGrabing humanGrabing;

    private Vector3 moveDir = Vector3.zero;
    [HideInInspector] public Vector3 fixMoveDir = Vector3.zero;
    private float velocityY = 0.0f;
    private Vector3 attackAwayDir = Vector3.zero;

    private bool jumpFlg = false;

    public void StartM()
    {
        // いろいろ取得
        camTr = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        playerConfig = Resources.Load<PlayerConfig>("PlayerConfigData");
        humanManager = GetComponent<HumanManager>();
        humanAnimator = GetComponent<HumanAnimator>();
        humanGrabing = GetComponent<HumanGrabing>();
    }

    public void UpdateM()
    {
        // WASDの入力
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputV = camTr.forward * v + camTr.right * h;
        inputV.y = 0f;
        inputV = Vector3.Normalize(inputV);
        if (humanManager.isGround)
        {
            // 移動方向と速さ　じわっと加速させる
            moveDir = Vector3.Lerp(moveDir, inputV * playerConfig.moveSpeed, playerConfig.moveAcc * Time.deltaTime);
            if (jumpFlg) // ジャンプ
            {
                velocityY = playerConfig.jumpForce;
                jumpFlg = false;
            }
        }
        else
        {
            // 移動方向と速さ　じわっと加速させる
            moveDir = Vector3.Lerp(moveDir, inputV * playerConfig.moveSpeed, playerConfig.moveAcc * playerConfig.airMoveMultiply * Time.deltaTime);
            // 重力加速度
            velocityY -= 9.81f * Time.deltaTime;
        }
        if (humanAnimator.stateInfoBase.IsName("KnockBack"))
        {
            fixMoveDir = attackAwayDir;
        }
        else
        {
            // 移動方向の修正
            fixMoveDir = moveDir;
            fixMoveDir *= humanAnimator.stateInfoBase.IsName("JumpEnd") ? playerConfig.landingDeceMultiply : 1.0f; // 着地時
            fixMoveDir *= humanGrabing.isGrab ? playerConfig.grabingDeceMultiply : 1.0f; // 掴みモーション時
            fixMoveDir *= humanGrabing.isGrabed || humanGrabing.isGrabing ? 0.0f : 1.0f; // 誰かに掴まれているか、誰かを掴んでいる
        }
        fixMoveDir += Vector3.up * velocityY; // y方向の追加
    }

    public void AttackAway(Vector3 attackerPos)
    {
        Vector3 d = transform.position - attackerPos;
        d.y = 0.0f;
        attackAwayDir = Vector3.Normalize(d) * playerConfig.gorillaAttackPower;
    }

    public void MoveUpdateM()
    {
        // プレイヤーを移動させる、着地モーションと掴みモーションの時は減速
        controller.Move(fixMoveDir * Time.deltaTime);
        if (!humanAnimator.stateInfoBase.IsName("KnockBack"))
        {
            // 移動する方向を向く
            Vector3 newRot = Vector3.RotateTowards(transform.forward, new Vector3(fixMoveDir.x, 0.0f, fixMoveDir.z), playerConfig.rotateSpeed * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(newRot);
        }
    }

    public void JumpForce()
    {
        jumpFlg = true;
    }
}
