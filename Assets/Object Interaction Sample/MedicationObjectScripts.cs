using UnityEngine;

public class MedicationObject : MonoBehaviour
{
    [SerializeField] private float value = 10;
    private void Start() => PlayerController.onPlayerInteracts += PlayerController_onPlayerInteracts;
    private void OnDestroy() => PlayerController.onPlayerInteracts -= PlayerController_onPlayerInteracts;

    private void PlayerController_onPlayerInteracts(RaycastHit obj, PlayerController senderPlayer)
    {
        if (this.gameObject != obj.transform.gameObject) return;      
        senderPlayer.Medication(value, false);
        Destroy(this.gameObject);
    }
}
