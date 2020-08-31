using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviourPunCallbacks
{
    private Text waitingMessage;

    private void Start()
    {
        waitingMessage = GameObject.FindWithTag(Tags.WaitingMessage).GetComponent<Text>();
        if (photonView.IsMine)
        {
            UpdatePlayerCount(PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ルームにいる人数が最大人数だったら、ルームをクローズにして、マッチング完了にする
            if (PhotonNetwork.CurrentRoom.PlayerCount == (byte)GameOpiton.maxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                photonView.RPC(nameof(MatchingComplete), RpcTarget.All, PhotonNetwork.ServerTimestamp);
            }
            else
            {
                photonView.RPC(nameof(UpdatePlayerCount), RpcTarget.All, PhotonNetwork.CurrentRoom.PlayerCount);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(UpdatePlayerCount), RpcTarget.All, PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }

    [PunRPC]
    private void UpdatePlayerCount(byte playerCount)
    {
        waitingMessage.text = string.Format("マッチング中...\n{0:##} / {1:##}", playerCount, GameOpiton.maxPlayers);
    }

    [PunRPC]
    private void MatchingComplete(int matchTimestamp)
    {
        waitingMessage.text = "マッチング完了！\nまもなくゲーム開始です！！";
        GameObject.FindWithTag(Tags.GameManager).GetComponent<MyGameManager>().OnMatched(matchTimestamp);
    }
}
