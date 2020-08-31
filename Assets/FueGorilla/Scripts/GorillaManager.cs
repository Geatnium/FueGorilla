using UnityEngine;
using Photon.Pun;

public class GorillaManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public int id = -1;
    private GorillaMover gorillaMover;
    private MouseLooker mouseLooker;
    private GorillaAnimator gorillaAnimator;
    private HumanGrabing humanGrabing;
    private Transform nameLabelTr, camTr;

    [SerializeField] private Material blue, green;

    [HideInInspector] public TeamColor teamColor = TeamColor.None;
    [HideInInspector] public PlayerState playerState = PlayerState.Gorilla;

    [HideInInspector] public bool isMoveable = true;

    /// <summary>
    /// 地面に触れているかどうか
    /// </summary>
    [HideInInspector] public bool isGround = false;

    [SerializeField] private GameObject attackColliderObj;

    private AttackCollider attackCollider;

    private float t = 0f;

    private void Start()
    {
        // いろいろ取得
        gorillaMover = GetComponent<GorillaMover>();
        mouseLooker = GetComponent<MouseLooker>();
        gorillaAnimator = GetComponent<GorillaAnimator>();
        humanGrabing = GetComponent<HumanGrabing>();
        // 移動のスタート関数
        gorillaMover.StartM();
        if (photonView.IsMine)
        {
            // マウス移動のスクリプトはカメラの奪い合いが発生するので自分のだけ処理
            mouseLooker.StartM();
        }
        Resources.UnloadUnusedAssets();
        camTr = Camera.main.transform;
        nameLabelTr = transform.Find("NameLabel");
        nameLabelTr.GetComponent<TextMesh>().text = photonView.Owner.NickName;

        if (photonView.Owner.CustomProperties[Hashes.TeamColor] is int teamColor)
        {
            SetTeam((TeamColor)teamColor);
        }
        if (photonView.Owner.CustomProperties[Hashes.ID] is int id)
        {
            this.id = id;
        }
        AttackCollider mineAttackCollider = attackColliderObj.GetComponent<AttackCollider>();
        mineAttackCollider.actorNumber = photonView.OwnerActorNr;
        mineAttackCollider.teamColor = this.teamColor;
        mineAttackCollider.attackerTransform = transform;
        attackColliderObj.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            GameObject.FindWithTag(Tags.GameManager).GetComponent<MyGameManager>().UpdateHumanCount(this.teamColor);
        }
    }

    public void SetTeam(TeamColor teamColor)
    {
        this.teamColor = teamColor;
        if (teamColor == TeamColor.Blue)
        {
            transform.Find("Geometry/Gorilla").GetComponent<SkinnedMeshRenderer>().material = blue;
        }
        else
        {
            transform.Find("Geometry/Gorilla").GetComponent<SkinnedMeshRenderer>().material = green;
        }
    }

    private void Update()
    {
        nameLabelTr.forward = -(camTr.position - nameLabelTr.position);
        // 地面にレイを撃って接地判定
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGround = Physics.Raycast(ray, 0.3f);

        // 自分のオブジェクトの時
        if (photonView.IsMine)
        {
            t += Time.deltaTime;
            if (MyGameManager.state == GameState.Playing || MyGameManager.state == GameState.Matching)
            {
                // 地面に触れている かつ 着地モーションじゃない かつ ジャンプボタンを押したらジャンプ
                if (isGround && !humanGrabing.isGrabed
                    && !gorillaAnimator.stateInfo.IsName("Attack")
                    && !gorillaAnimator.stateInfo.IsName("Fall")
                    && Input.GetButtonDown("Jump"))
                {
                    gorillaAnimator.JumpAnimTrigger();
                    gorillaMover.JumpForce();
                }
                // 地面に触れている かつ ジャンプ系のモーションじゃない かつ Fireボタンを押したら掴む
                if (isGround && !gorillaAnimator.stateInfo.IsName("Jump") && !gorillaAnimator.stateInfo.IsName("Attack")
                    && !gorillaAnimator.stateInfo.IsName("JumpEnd") && !gorillaAnimator.stateInfo.IsName("Fall")
                    && Input.GetButtonDown("Fire"))
                {
                    gorillaAnimator.AttackAnimTrigger();
                }
                gorillaMover.UpdateM();
            }
            else
            {
                gorillaMover.fixMoveDir = new Vector3(0f, gorillaMover.fixMoveDir.y, 0f);
            }
        }
        gorillaMover.MoveUpdateM();
        gorillaAnimator.UpdateM();
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            mouseLooker.UpdateM();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            if (t < 1.0f) return;
            if (other.gameObject.CompareTag(Tags.Attack))
            {
                if (attackCollider == null || attackCollider != other.gameObject.GetComponent<AttackCollider>())
                {
                    attackCollider = other.gameObject.GetComponent<AttackCollider>();
                }
                if (attackCollider.actorNumber != photonView.OwnerActorNr)
                {
                    gorillaMover.AttackAway(attackCollider.attackerTransform.position);
                    gorillaAnimator.KnockBackTrigger();
                }
            }
        }
    }

    // データの送受信
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 自分のオブジェクト
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(gorillaMover.fixMoveDir);
        }
        else // 他の人のオブジェクト
        {
            transform.position = (Vector3)stream.ReceiveNext();
            gorillaMover.fixMoveDir = (Vector3)stream.ReceiveNext();
        }
    }
}
