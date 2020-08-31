using UnityEngine;
using Photon.Pun;

public class Graber : MonoBehaviour
{
    [HideInInspector] public int actorNumber;
    [HideInInspector] public bool isMine;
    /// <summary>
    /// 誰かを掴んでいる
    /// </summary>
    [HideInInspector] public bool isGrabing;

    [HideInInspector] public GameObject grabingPlayer;

    private void OnTriggerStay(Collider other)
    {
        if (isMine)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (grabingPlayer == null || grabingPlayer != other.gameObject)
                {
                    grabingPlayer = other.gameObject;
                }
                if (!grabingPlayer.GetPhotonView().IsMine)
                {
                    isGrabing = true;
                }
                else
                {
                    isGrabing = false;
                    grabingPlayer = null;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isMine)
        {
            isGrabing = false;
            grabingPlayer = null;
        }
    }

    private void OnEnable()
    {
        if (isMine)
        {
            isGrabing = false;
            grabingPlayer = null;
        }
    }

    private void OnDisable()
    {
        if (isMine)
        {
            isGrabing = false;
            grabingPlayer = null;
        }
    }
}
