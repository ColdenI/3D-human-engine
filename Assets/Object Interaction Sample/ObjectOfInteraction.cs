using UnityEngine;

public class ObjectOfInteraction : MonoBehaviour
{
    private void Start() => PlayerController.onPlayerInteracts += PlayerController_onPlayerInteracts;
    private void OnDestroy() => PlayerController.onPlayerInteracts -= PlayerController_onPlayerInteracts;


    private void PlayerController_onPlayerInteracts(RaycastHit obj, PlayerController senderPlayer)
    {
        if(this.gameObject == obj.transform.gameObject)
            print($"{obj.transform.gameObject.name}   {senderPlayer.Stamina}");
    }
}
