using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{   
    public Camera WeaponCamera;

    //объект вылетающий гильзы
    public GameObject bullet;
    public Transform spawnBullet;
    public float shootForce;
    public float spread;


    private void Awake() => PlayerController.onSetCameraParameters += PlayerController_onSetCameraParameters;
    private void OnDestroy() => PlayerController.onSetCameraParameters -= PlayerController_onSetCameraParameters;

    private void PlayerController_onSetCameraParameters(float cameraFOV, Vector3 centerScreen)
    {
        WeaponCamera.fieldOfView = cameraFOV;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    public void Shoot()
    {
        Ray ray = WeaponCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 dirWithoutSpread = targetPoint - spawnBullet.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 dirWithSpread = dirWithoutSpread + new Vector3(x, y, 0);


        GameObject currentBullet = Instantiate(bullet, spawnBullet.position, Quaternion.identity);
        currentBullet.transform.forward = dirWithSpread.normalized;
        currentBullet.GetComponent<Rigidbody>().AddForce(dirWithSpread.normalized * shootForce, ForceMode.Impulse);
    }
}
