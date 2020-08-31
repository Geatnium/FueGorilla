using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public enum TeamColor
{
    None, Blue, Green
}

public enum PlayerState
{
    None, Human, Gorilla
}

public class HumanManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public int id = -1;
    private MyGameManager gameManager;
    private PlayerHumanMover humanMover;
    private MouseLooker mouseLooker;
    private HumanAnimator humanAnimator;
    private HumanGrabing humanGrabing;
    private Transform nameLabelTr, camTr;

    [SerializeField] private Material blue, green;

    [HideInInspector] public TeamColor teamColor = TeamColor.None;
    [HideInInspector] public PlayerState playerState = PlayerState.None;

    [HideInInspector] public bool isMoveable = true;

    private AttackCollider attackCollider;

    [SerializeField] private GameObject deathAudio;

    /// <summary>
    /// 地面に触れているかどうか
    /// </summary>
    [HideInInspector] public bool isGround = false;

    private float damageInterval = -1f;

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != photonView.OwnerActorNr) return;

        if (changedProps[Hashes.TeamColor] is int teamColor)
        {
            SetTeam((TeamColor)teamColor);
        }
        if (changedProps[Hashes.ID] is int id)
        {
            this.id = id;
            if (this.teamColor == TeamColor.Blue)
            {
                transform.position = GameObject.Find("SpawnerBlue").transform.position + new Vector3(1.5f * id, 0.0f, 0.0f);
                transform.rotation = GameObject.Find("SpawnerBlue").transform.rotation;

            }
            else
            {
                transform.position = GameObject.Find("SpawnerGreen").transform.position + new Vector3(-1.5f * (id - GameOpiton.maxPlayers / 2), 0.0f, 0.0f);
                transform.rotation = GameObject.Find("SpawnerGreen").transform.rotation;
            }
        }
        if (changedProps[Hashes.PlayerState] is int playerState)
        {
            this.playerState = (PlayerState)playerState;
            if (this.playerState == PlayerState.Gorilla)
            {
                Gorillaization();
            }
        }
    }

    public void SetTeam(TeamColor teamColor)
    {
        this.teamColor = teamColor;
        if (teamColor == TeamColor.Blue)
        {
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = blue;
        }
        else
        {
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = green;
        }
    }

    public void Gorillaization()
    {
        if (photonView.IsMine)
        {
            gameManager.PlayerGorillaizationed();
            camTr.transform.parent = null;
            PhotonNetwork.Instantiate("PlayerGorilla", transform.position, transform.rotation);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void Start()
    {
        // いろいろ取得
        gameManager = GameObject.FindWithTag(Tags.GameManager).GetComponent<MyGameManager>();
        humanMover = GetComponent<PlayerHumanMover>();
        mouseLooker = GetComponent<MouseLooker>();
        humanAnimator = GetComponent<HumanAnimator>();
        humanGrabing = GetComponent<HumanGrabing>();
        // 移動のスタート関数
        humanMover.StartM();
        if (photonView.IsMine)
        {
            // マウス移動のスクリプトはカメラの奪い合いが発生するので自分のだけ処理
            mouseLooker.StartM();
            humanGrabing.StartM();
        }
        Resources.UnloadUnusedAssets();
        camTr = Camera.main.transform;
        nameLabelTr = transform.Find("NameLabel");
        nameLabelTr.GetComponent<TextMesh>().text = photonView.Owner.NickName;
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
            if (MyGameManager.state == GameState.Playing || MyGameManager.state == GameState.Matching)
            {
                // 地面に触れている かつ 着地モーションじゃない かつ ジャンプボタンを押したらジャンプ
                if (isGround && !humanGrabing.isGrabing && !humanGrabing.isGrabed
                    && !humanAnimator.stateInfoBase.IsName("Fall")
                    && Input.GetButtonDown("Jump"))
                {
                    humanAnimator.JumpAnimTrigger();
                    humanMover.JumpForce();
                }
                // 地面に触れている かつ ジャンプ系のモーションじゃない かつ Fireボタンを押したら掴む
                humanGrabing.wantGrab = isGround
                    && !humanAnimator.stateInfoBase.IsName("Jump")
                    && !humanAnimator.stateInfoBase.IsName("JumpEnd")
                    && !humanAnimator.stateInfoBase.IsName("Fall")
                    && Input.GetButton("Fire");
                humanGrabing.UpdateM();
                humanMover.UpdateM();
            }
            else
            {
                humanMover.fixMoveDir = new Vector3(0f, humanMover.fixMoveDir.y, 0f);
            }
        }
        humanMover.MoveUpdateM();
        humanAnimator.UpdateM();

        if (damageInterval > 0.0f)
        {
            damageInterval -= Time.deltaTime;
        }
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
            if (other.gameObject.CompareTag(Tags.Attack))
            {
                if (attackCollider == null || attackCollider != other.gameObject.GetComponent<AttackCollider>())
                {
                    attackCollider = other.gameObject.GetComponent<AttackCollider>();
                }
                if (damageInterval < 0.0f)
                {
                    damageInterval = 0.5f;
                    if (attackCollider.teamColor != teamColor)
                    {
                        Gorillaization();
                    }
                    else
                    {
                        humanMover.AttackAway(attackCollider.attackerTransform.position);
                        humanAnimator.KnockBackTrigger();
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (MyGameManager.state == GameState.Playing)
        {
            Instantiate(deathAudio, transform.position, Quaternion.identity);
        }
    }

    // データの送受信
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 自分のオブジェクト
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(humanMover.fixMoveDir);
        }
        else // 他の人のオブジェクト
        {
            transform.position = (Vector3)stream.ReceiveNext();
            humanMover.fixMoveDir = (Vector3)stream.ReceiveNext();
        }
    }

}
