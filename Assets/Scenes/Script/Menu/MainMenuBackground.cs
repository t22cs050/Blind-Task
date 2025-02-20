using System;
using UnityEngine;

public class MainMenuBackground : MonoBehaviour
{
    [SerializeField]private GameObject MapObj;
    private MapMaster MapSc;
    
    [SerializeField]private GameObject Cam;
    void Start()
    {
        MapObj = Instantiate(MapObj, this.transform);
        MapSc = MapObj.GetComponent<MapMaster>();

        //シード値として現在時刻のミリ秒を指定
        int seed = DateTime.Now.Millisecond;
        MapSc.create(new offlineMain(), seed, 61, 61);

        Vector3 newCamPos = MapObj.transform.position;
        newCamPos += new Vector3(30, 4, -30);
        Cam.transform.position = newCamPos;

        Vector3 newCamRot = Cam.transform.eulerAngles;
        newCamRot.x = 40;
        Cam.transform.eulerAngles = newCamRot;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newCamRot = Cam.transform.eulerAngles;
        newCamRot.y += Time.deltaTime*4;
        Cam.transform.eulerAngles = newCamRot;
    }
}
