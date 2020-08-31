using UnityEngine;

public class TimerDestroyer : MonoBehaviour
{
    [SerializeField] private float destroyTime = 5.0f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

}
