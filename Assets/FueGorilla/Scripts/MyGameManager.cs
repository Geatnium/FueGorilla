using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOpiton
{
    public static int maxPlayers = 14;
    public static int timeLimit = 60;
}

public enum GameState
{
    Matching, Matched, Ready, Countdown, Playing, Finished
}

public class MyGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int matchTimeToReadyOffset = 3, readyToCountdownOffset = 2, startCountdownTime = 3;
    private int matchTimeStamp = 0;
    private int readyTimeStamp, countDownTimeStamp, gameStartTimeStamp, gameFinishTimeStamp;
    public static GameState state = GameState.Matching;

    private TeamColor teamColor;

    [SerializeField] private GameObject waitingUI;

    [SerializeField] private Text countDownText;

    [SerializeField] private GameObject gameUI;
    [SerializeField] private Text timerText;
    [SerializeField] private Text taskText, howFireText1, howFireText2;
    [SerializeField] private RectTransform blueTeamIconsParent, greenTeamIconsParent;
    [SerializeField] private RectTransform finishTextObj;
    [SerializeField] private Image blackImg;
    private int blueTeamHumans = 7, greenTeamHumans = 7;
    private int teamDefultHumans;

    private ExitGames.Client.Photon.Hashtable roomHash;

    public void OnMatched(int timeStamp)
    {
        state = GameState.Matched;
        matchTimeStamp = timeStamp;
        readyTimeStamp = matchTimeStamp + matchTimeToReadyOffset * 1000;
        countDownTimeStamp = readyTimeStamp + readyToCountdownOffset * 1000;
        gameStartTimeStamp = countDownTimeStamp + startCountdownTime * 1000;
        gameFinishTimeStamp = gameStartTimeStamp + GameOpiton.timeLimit * 1000;
        AudioManager.PlaySE(SoundEffect.Matched);
    }

    private void Update()
    {
        if (state == GameState.Matching) return;
        if (state == GameState.Matched && readyTimeStamp - PhotonNetwork.ServerTimestamp < 0)
        {
            state = GameState.Ready;
            gameUI.SetActive(true);
            Ready();
        }
        if (state == GameState.Ready && countDownTimeStamp - PhotonNetwork.ServerTimestamp < 0)
        {
            state = GameState.Countdown;
        }
        if (state == GameState.Countdown && gameStartTimeStamp - PhotonNetwork.ServerTimestamp > 0)
        {
            CountDown();
        }
        if (state == GameState.Countdown && gameStartTimeStamp - PhotonNetwork.ServerTimestamp < 0)
        {
            state = GameState.Playing;
            GameStart();
        }
        if(state == GameState.Playing && gameFinishTimeStamp - PhotonNetwork.ServerTimestamp > 0)
        {
            TimerCount();
        }
        if(state == GameState.Playing && gameFinishTimeStamp - PhotonNetwork.ServerTimestamp < 0)
        {
            GameFinish(true);
        }
    }

    private void Ready()
    {
        AudioManager.ChangeBGM(BGM.None);
        AudioManager.PlaySE(SoundEffect.Trumpet);
        roomHash = new ExitGames.Client.Photon.Hashtable();
        teamDefultHumans = GameOpiton.maxPlayers / 2;
        blueTeamHumans = teamDefultHumans;
        greenTeamHumans = teamDefultHumans;
        Destroy(waitingUI);
        if (!PhotonNetwork.IsMasterClient) return;
        roomHash.Add(Hashes.BlueTeamHumans, blueTeamHumans);
        roomHash.Add(Hashes.GreenTeamHumans, greenTeamHumans);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
        GameObject[] players = GameObject.FindGameObjectsWithTag(Tags.Player);
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView playerPhotonView = players[i].GetComponent<PhotonView>();
            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            hashtable[Hashes.ID] = i;
            if (i == 0)
            {
                hashtable[Hashes.PlayerState] = (int)PlayerState.Gorilla;
                hashtable[Hashes.TeamColor] = (int)TeamColor.Blue;
            }
            else if (i == GameOpiton.maxPlayers / 2)
            {
                hashtable[Hashes.PlayerState] = (int)PlayerState.Gorilla;
                hashtable[Hashes.TeamColor] = (int)TeamColor.Green;
            }
            else
            {
                hashtable[Hashes.PlayerState] = (int)PlayerState.Human;
                if (i < GameOpiton.maxPlayers / 2)
                {
                    hashtable[Hashes.TeamColor] = (int)TeamColor.Blue;
                }
                else
                {
                    hashtable[Hashes.TeamColor] = (int)TeamColor.Green;
                }
            }
            playerPhotonView.Owner.SetCustomProperties(hashtable);
        }
    }

    private void CountDown()
    {
        int t = (gameStartTimeStamp - PhotonNetwork.ServerTimestamp) / 1000 + 1;
        countDownText.text = t.ToString();
    }

    private void GameStart()
    {
        AudioManager.ChangeBGM(BGM.Playing);
        Destroy(countDownText.transform.parent.gameObject);
    }

    public void PlayerGorillaizationed()
    {
        taskText.text = "相手チームの人間をゴリラにしろ！";
        howFireText1.text = "：ゴリラにする";
        howFireText2.text = "：ゴリラにする";
    }

    public void UpdateHumanCount(TeamColor teamColor)
    {
        if (teamColor == TeamColor.Blue)
        {
            blueTeamHumans--;
            roomHash[Hashes.BlueTeamHumans] = blueTeamHumans;
        }
        if (teamColor == TeamColor.Green)
        {
            greenTeamHumans--;
            roomHash[Hashes.GreenTeamHumans] = greenTeamHumans;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }

    public void TimerCount()
    {
        timerText.text = string.Format("残り時間：{0:00.00}", (gameFinishTimeStamp - PhotonNetwork.ServerTimestamp) / 1000f);
    }

    public void GameFinish(bool timeUp)
    {
        state = GameState.Finished;
        AudioManager.ChangeBGM(BGM.None);
        AudioManager.PlaySE(SoundEffect.Switch);
        Time.timeScale = 0.0f;
        if (timeUp)
        {
            timerText.text = string.Format("残り時間 00.00");
        }
        finishTextObj.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        StartCoroutine(GoResultScene());
    }

    private ResultData resultData;
    private IEnumerator GoResultScene()
    {
        resultData = new ResultData();
        resultData.blueTeamHumans = blueTeamHumans;
        resultData.greenTeamHumans = greenTeamHumans;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        resultData.players = new PlayerInfo[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            resultData.players[i] = new PlayerInfo();
            resultData.players[i].nickName = players[i].GetPhotonView().Owner.NickName;
            if (players[i].GetComponent<HumanManager>() != null)
            {
                resultData.players[i].teamColor = players[i].GetComponent<HumanManager>().teamColor;
                resultData.players[i].playerState = PlayerState.Human;
            }
            else
            {
                resultData.players[i].teamColor = players[i].GetComponent<GorillaManager>().teamColor;
                resultData.players[i].playerState = PlayerState.Gorilla;
            }
        }
        yield return new WaitForSecondsRealtime(2.5f);
        blackImg.gameObject.SetActive(true);
        blackImg.DOFade(1.0f, 0.4f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);
        PhotonNetwork.Disconnect();
        SceneManager.sceneLoaded += SendResultData;
        SceneManager.LoadScene("Result");
    }

    private void SendResultData(Scene next, LoadSceneMode mode)
    {
        GameObject.FindWithTag(Tags.ResultManager).GetComponent<ResultManager>().SetResultData(resultData);
        SceneManager.sceneLoaded -= SendResultData;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        object blue = null, green = null;
        if (propertiesThatChanged.TryGetValue(Hashes.BlueTeamHumans, out blue))
        {
            blueTeamHumans = (int)blue;
        }
        if (propertiesThatChanged.TryGetValue(Hashes.GreenTeamHumans, out green))
        {
            greenTeamHumans = (int)green;
        }
        for (int i = 0; i < 6; i++)
        {
            blueTeamIconsParent.GetChild(i).gameObject.SetActive(i < blueTeamHumans);
            greenTeamIconsParent.GetChild(i).gameObject.SetActive(i < greenTeamHumans);
        }
        if(blueTeamHumans == 0 || greenTeamHumans == 0)
        {
            GameFinish(false);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber) return;

        if (changedProps[Hashes.TeamColor] is int teamColor)
        {
            this.teamColor = (TeamColor)teamColor;
            if (this.teamColor == TeamColor.Green)
            {
                blueTeamIconsParent.localPosition += new Vector3(1000f, 0f, 0f);
                greenTeamIconsParent.localPosition -= new Vector3(1000f, 0f, 0f);
                blueTeamIconsParent.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
                greenTeamIconsParent.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleRight;
            }
        }
    }
}
