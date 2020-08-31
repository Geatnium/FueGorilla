using UnityEngine;

public class GorillaMover : MonoBehaviour
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

    private GorillaManager gorillaManager;
    private GorillaAnimator gorillaAnimator;
    private HumanGrabing humanGrabing;

    private Vector3 moveDir = Vector3.zero;
    [HideInInspector] public Vector3 fixMoveDir = Vector3.zero;
    private float velocityY = 0.0f;

    private bool jumpFlg = false;

    private Vector3 attackAwayDir = Vector3.zero;

    public void StartM()
    {
        // いろいろ取得
        camTr = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        playerConfig = Resources.Load<PlayerConfig>("PlayerConfigData");
        gorillaManager = GetComponent<GorillaManager>();
        gorillaAnimator = GetComponent<GorillaAnimator>();
        humanGrabing = GetComponent<HumanGrabing>();
        Resources.UnloadUnusedAssets();
    }

    public void UpdateM()
    {
        // WASDの入力
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputV = camTr.forward * v + camTr.right * h;
        inputV.y = 0f;
        inputV = Vector3.Normalize(inputV);
        if (gorillaManager.isGround)
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

        // 移動方向の修正　着地時と掴み時は少し減速　y方向の速度をプラス
        if (gorillaAnimator.stateInfo.IsName("KnockBack"))
        {
            fixMoveDir = attackAwayDir;
        }
        else
        {
            fixMoveDir = moveDir;
            fixMoveDir *= gorillaAnimator.stateInfo.IsName("JumpEnd") ? playerConfig.landingDeceMultiply : 1.0f;
            fixMoveDir *= gorillaAnimator.stateInfo.IsName("Attack") ? playerConfig.gorillaAttackingDeceMultiply : 1.0f;
            fixMoveDir *= gorillaAnimator.stateInfo.IsName("ChestHit") ? 0.0f : 1.0f;
            fixMoveDir *= humanGrabing.isGrabed ? 0.0f : 1.0f; // 人間に掴まれている
        }
        fixMoveDir += Vector3.up * velocityY;
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
        if (!gorillaAnimator.stateInfo.IsName("KnockBack"))
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
