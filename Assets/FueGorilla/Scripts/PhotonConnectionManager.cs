using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PhotonConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text messageText;
    [SerializeField] private InputField nameField;
    [SerializeField] private GameObject titleCanvas, startButton;
    [SerializeField] private GameObject howUI;

    //ゲームバージョン指定（設定しないと警告が出る）
    private const string gameVersion = "Ver1.0";

    private void Start()
    {
        Application.targetFrameRate = 30;
        PhotonNetwork.SendRate = 10;
        PhotonNetwork.SerializationRate = 10;
        PhotonNetwork.GameVersion = gameVersion;
    }

    public void OnConnectionStart()
    {
        if (string.IsNullOrEmpty(nameField.text) || string.IsNullOrWhiteSpace(nameField.text))
        {
            messageText.text = "表示名を入力してください。";
            return;
        }
        startButton.SetActive(false);
        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
        messageText.text = "接続中...";
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        // ルームに最大人数いる場合は接続解除
        if (PhotonNetwork.CountOfPlayersInRooms >= GameOpiton.maxPlayers)
        {
            startButton.SetActive(true);
            messageText.text = "現在他のプレイヤーが試合中です。\nちょっとだけ待ってから（10秒〜1分くらい）、もう一度お試しください。";
            PhotonNetwork.Disconnect();
            return;
        }
        // "room"という名前のルームに参加する（ルームが無ければ作成してから参加する）
        PhotonNetwork.JoinOrCreateRoom("room",
            new RoomOptions()
            {
                MaxPlayers = (byte)GameOpiton.maxPlayers,
                IsOpen = true
            }, TypedLobby.Default);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        startButton.SetActive(true);
        messageText.text = "ルームの作成に失敗しました。\nもう一度お試しください。";
        PhotonNetwork.Disconnect();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        startButton.SetActive(true);
        messageText.text = "ルームへの参加に失敗しました。。\nもう一度お試しください。";
        PhotonNetwork.Disconnect();
    }

    // マッチングが成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = nameField.text; //playerの名前を設定
        // マッチング後、ランダムな位置に自分自身のネットワークオブジェクトを生成する
        Vector3 p = new Vector3(Random.Range(-15f, 15f), 5.0f, Random.Range(-15f, 15f));
        PhotonNetwork.Instantiate("PlayerHuman", p, Quaternion.Euler(0.0f, Random.Range(0f, 360f), 0.0f));
        PhotonNetwork.Instantiate("MatchingManager", Vector3.zero, Quaternion.identity);
        howUI.SetActive(true);
        MyGameManager.state = GameState.Matching;
        Destroy(titleCanvas);
        Destroy(GameObject.Find("TitleGorillas"));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            startButton.SetActive(true);
            messageText.text = "接続に失敗しました。";
        }
    }
}
