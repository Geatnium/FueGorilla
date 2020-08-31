using UnityEngine;

public class LookCamera : MonoBehaviour
{
    private Transform camTr;

    private void Start()
    {
        camTr = Camera.main.transform;
    }

    private void Update()
    {
        transform.forward = -(camTr.position - transform.position);
    }
}
