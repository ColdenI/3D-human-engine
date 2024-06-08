using UnityEditor.UIElements;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private float liveTime = 5f;
    private bool flagTwo = false;

    private void Start() => Destroy(gameObject, liveTime);

    private void OnCollisionEnter(Collision collision)
    {
        // проверка на попадание во врагов
        if(flagTwo) Destroy(gameObject);
        flagTwo = true;
    }
}
