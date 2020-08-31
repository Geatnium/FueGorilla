using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class HumanGrabing : MonoBehaviourPunCallbacks, IPunObservable
{
    /// <summary>
    /// 掴みの要求
    /// </summary>
    [HideInInspector] public bool wantGrab = false;
    /// <summary>
    /// 掴める状態（掴みモーションしている）
    /// </summary>
    [HideInInspector] public bool isGrab = false;
    /// <summary>
    /// 他のプレイヤーを掴んでいる
    /// </summary>
    [HideInInspector] public bool isGrabing = false;
    /// <summary>
    /// 誰かに掴まれている
    /// </summary>
    [HideInInspector] public bool isGrabed = false;
    /// <summary>
    /// 掴みモーションができる状態か
    /// </summary>
    [HideInInspector] public bool isGrabable = true;

    private bool isGrabed2 = false;

    [SerializeField] private GameObject grabCollider;
    private Graber mineGraber;
    private HumanGrabing otherPlayer;

    private bool otherPlayerIsGrabed = false, otherPlayerIsGrabed2 = false;

    private float grabingTime = 0f, grabIntervalTime = 0f, grabedTime = 0f;

    private PlayerConfig playerConfig;

    private AudioSource audioSource;
    [SerializeField] private AudioClip grabClip;

    public void StartM()
    {
        mineGraber = grabCollider.GetComponent<Graber>();
        mineGraber.isMine = photonView.IsMine;
        playerConfig = Resources.Load<PlayerConfig>("PlayerConfigData");
        audioSource = GetComponent<AudioSource>();
    }

    public void UpdateM()
    {
        // 掴みの要求がきている
        if (wantGrab)
        {
            // 掴みが可能の場合は掴みをtrue
            isGrab = isGrabable;
        }
        else // 掴みの要求がきていない
        {
            isGrab = false;
            // 誰かを掴んでいた状態だった場合、掴みを出来なくし、相手を話す
            if (isGrabing)
            {
                isGrabable = false;
                otherPlayerIsGrabed = false;
            }
            isGrabing = false;
        }
        // 掴み状態の時はコライダーをオン
        mineGraber.gameObject.SetActive(isGrab);
        // 掴み状態の時
        if (isGrab)
        {
            // 手が誰かを掴んでいるか
            if (!isGrabing)
            {
                if (mineGraber.isGrabing)
                {
                    audioSource.PlayOneShot(grabClip);
                }
            }
            isGrabing = mineGraber.isGrabing;
            if (isGrabing) // 誰かを掴んでいる
            {
                if (otherPlayer == null || otherPlayer != mineGraber.grabingPlayer.GetComponent<HumanGrabing>())
                {
                    otherPlayer = mineGraber.grabingPlayer.GetComponent<HumanGrabing>();
                }
                otherPlayerIsGrabed = true;
                grabingTime += Time.deltaTime;
                if(grabingTime >= playerConfig.maxGrabingTime)
                {
                    isGrab = false;
                    isGrabable = false;
                    isGrabing = false;
                    otherPlayerIsGrabed = false;
                }
            }
            else
            {
                grabingTime = 0f;
                otherPlayerIsGrabed = false;
            }
        }
        if(!isGrab)
        {
            isGrabing = false;
            otherPlayerIsGrabed = false;
        }
        // 掴みが出来ない状態の時は、インターバル後開放
        if (!isGrabable)
        {
            grabIntervalTime += Time.deltaTime;
            if(grabIntervalTime >= playerConfig.grabInterval)
            {
                isGrabable = true;
            }
        }
        else
        {
            grabIntervalTime = 0f;
        }

        if (!isGrabed2)
        {
            if (isGrabed)
            {
                audioSource.PlayOneShot(grabClip);
            }
        }
        isGrabed2 = isGrabed;

        if(otherPlayerIsGrabed2 != otherPlayerIsGrabed)
        {
            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            hashtable[Hashes.IsGrabed] = otherPlayerIsGrabed;
            otherPlayer.photonView.Owner.SetCustomProperties(hashtable);
            otherPlayerIsGrabed2 = otherPlayerIsGrabed;
        }

        if (isGrabed)
        {
            grabedTime += Time.deltaTime;
            if(grabedTime > playerConfig.maxGrabingTime)
            {
                isGrabed = false;
                grabedTime = 0f;
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != photonView.OwnerActorNr) return;
        if (changedProps[Hashes.IsGrabed] is bool isGrabed)
        {
            this.isGrabed = isGrabed;
        }
    }

    // データの送受信
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 自分のオブジェクト
        if (stream.IsWriting)
        {
            stream.SendNext(isGrab);
        }
        else // 他の人のオブジェクト
        {
            isGrab = (bool)stream.ReceiveNext();
        }
    }
}
