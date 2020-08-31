using UnityEngine;

public enum BGM
{
    Title, Playing, Result, None
}

public enum SoundEffect
{
    Matched, Trumpet, Switch
}

public class AudioManager : MonoBehaviour
{
    private AudioSource bgm_audioSource, se_audioSource;
    [SerializeField]private AudioClip[] bgms;
    [SerializeField]private AudioClip[] ses;

    private void Start()
    {
        bgm_audioSource = transform.GetChild(0).GetComponent<AudioSource>();
        se_audioSource = transform.GetChild(1).GetComponent<AudioSource>();
    }

    private void ChangeBGM_(BGM bgm)
    {
        if (bgm == BGM.None)
        {
            bgm_audioSource.Stop();
        }
        else
        {
            bgm_audioSource.clip = bgms[(int)bgm];
            bgm_audioSource.Play();
        }
    }

    private void PlaySE_(SoundEffect se)
    {
        se_audioSource.PlayOneShot(ses[(int)se]);
    }

    public static void ChangeBGM(BGM bgm)
    {
        GameObject.FindWithTag(Tags.AudioSource).GetComponent<AudioManager>().ChangeBGM_(bgm);
    }

    public static void PlaySE(SoundEffect soundEffect)
    {
        GameObject.FindWithTag(Tags.AudioSource).GetComponent<AudioManager>().PlaySE_(soundEffect);
    }
}
