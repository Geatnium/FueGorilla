using UnityEngine;

public class RandomTimingSounder : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float minTime = 1.0f, maxTime = 3.0f;
    private float t = 0f, nextTime;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        nextTime = Random.Range(minTime, maxTime);
    }

    private void Update()
    {
        t += Time.deltaTime;
        if(t > nextTime)
        {
            t = 0f;
            nextTime = Random.Range(minTime, maxTime);
            audioSource.PlayOneShot(clip);
        }
    }
}
