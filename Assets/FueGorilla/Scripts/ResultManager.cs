using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class ResultData
{
    public int blueTeamHumans = 0, greenTeamHumans = 0;
    public PlayerInfo[] players;
}

public class PlayerInfo
{
    public string nickName = "";
    public TeamColor teamColor = TeamColor.None;
    public PlayerState playerState = PlayerState.None;
}

public class ResultManager : MonoBehaviour
{
    private ResultData resultData;
    private enum Winner
    {
        Blue, Green, Draw
    }
    private Winner winner;
    [SerializeField] private GameObject humanObj, gorillaObj;
    [SerializeField] private Material blueMat, greenMat;
    [SerializeField] private GameObject effect;

    [SerializeField] private Image blackImg;
    [SerializeField] private Image backGroundImg;
    [SerializeField] private Text resultText;
    [SerializeField] private GameObject titleButton;

    [SerializeField] private Color blueColor, greenColor, drawColor;

    public void SetResultData(ResultData resultData)
    {
        this.resultData = resultData;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1.0f;
        if (resultData.blueTeamHumans > resultData.greenTeamHumans)
        {
            winner = Winner.Blue;
            resultText.text = "青チームの勝ち！！";
            resultText.color = blueColor;
        }
        else if (resultData.greenTeamHumans > resultData.blueTeamHumans)
        {
            winner = Winner.Green;
            resultText.text = "緑チームの勝ち！！";
            resultText.color = greenColor;
        }
        else
        {
            winner = Winner.Draw;
            resultText.text = "引き分け";
            resultText.color = drawColor;
        }
        int c = 0;
        for (int i = 0; i < resultData.players.Length; i++)
        {
            if (winner == Winner.Draw)
            {
                effect.SetActive(false);
                if (resultData.players[i].playerState == PlayerState.Human)
                {
                    GameObject human = Instantiate(humanObj, new Vector3(c / 2.0f + 0.25f, 0.0f, 0.5f), Quaternion.Euler(0f, 180f, 0f));
                    human.transform.Find("shadow_mesh").GetComponent<SkinnedMeshRenderer>().material = resultData.players[i].teamColor == TeamColor.Blue ? blueMat : greenMat;
                    human.GetComponent<Animator>().Play("Idle");
                    human.transform.Find("NameLabel").GetComponent<TextMesh>().text = resultData.players[i].nickName;
                }
                else
                {
                    GameObject gorilla = Instantiate(gorillaObj, new Vector3(c / 2.0f + 0.25f, 0.0f, 0.5f), Quaternion.Euler(0f, 180f, 0f));
                    gorilla.transform.Find("Geometry/Gorilla").GetComponent<SkinnedMeshRenderer>().material = resultData.players[i].teamColor == TeamColor.Blue ? blueMat : greenMat;
                    gorilla.GetComponent<Animator>().Play("Idle");
                    gorilla.transform.Find("NameLabel").GetComponent<TextMesh>().text = resultData.players[i].nickName;
                }
                c++;
            }
            else if ((winner == Winner.Blue && resultData.players[i].teamColor == TeamColor.Blue)
                || (winner == Winner.Green && resultData.players[i].teamColor == TeamColor.Green))
            {
                if (resultData.players[i].playerState == PlayerState.Human)
                {
                    GameObject human = Instantiate(humanObj, new Vector3(c + 0.5f, 0.0f, 0.5f), Quaternion.Euler(0f, 180f, 0f));
                    human.transform.Find("shadow_mesh").GetComponent<SkinnedMeshRenderer>().material = resultData.players[i].teamColor == TeamColor.Blue ? blueMat : greenMat;
                    human.GetComponent<Animator>().Play("Dance" + Random.Range(0, 5));
                    human.transform.Find("NameLabel").GetComponent<TextMesh>().text = resultData.players[i].nickName;
                }
                else
                {
                    GameObject gorilla = Instantiate(gorillaObj, new Vector3(c + 0.5f, 0.0f, 0.5f), Quaternion.Euler(0f, 180f, 0f));
                    gorilla.transform.Find("Geometry/Gorilla").GetComponent<SkinnedMeshRenderer>().material = resultData.players[i].teamColor == TeamColor.Blue ? blueMat : greenMat;
                    gorilla.GetComponent<Animator>().Play("Dance" + Random.Range(0, 2));
                    gorilla.transform.Find("NameLabel").GetComponent<TextMesh>().text = resultData.players[i].nickName;
                }
                c++;
            }
        }
        StartCoroutine(AnimationCor());
    }

    private IEnumerator AnimationCor()
    {
        blackImg.DOFade(0.0f, 0.5f);
        Camera.main.transform.DOMoveZ(-3.5f, 3.0f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(4.0f);
        blackImg.gameObject.SetActive(false);
        backGroundImg.DOFade(0.75f, 0.5f);
        resultText.DOFade(1.0f, 0.5f);
        yield return new WaitForSeconds(2.0f);
        titleButton.SetActive(true);
    }

    public void GoTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}
