using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(2)]  //スクリプトの実行順序を宣言。数値が小さいほど優先される
public class Player : MonoBehaviour
{
  public Vector2 pos;
  [SerializeField] private int speed;
  private Animator myAnimator;
  [SerializeField] RuntimeAnimatorController animeController;
  private bool isMoving;
  MapMaster mapScript;
  [SerializeField] private GameObject cam;
  private int mineCounter;//センサーを設置した回数を格納
  private List<Mine> mineList;
  Rigidbody rb;

    public void setting(MapMaster mapS, GameObject _cam){
      mapScript = mapS;
      cam = _cam;

      mineCounter = 0;
      mineList = new List<Mine>();

      myAnimator = this.GetComponent<Animator>();
      myAnimator.runtimeAnimatorController = animeController;
      rb = this.GetComponent<Rigidbody>();
      while(true){
        int x = Random.Range(0,mapScript.mapWidth);
        int y = Random.Range(0,mapScript.mapHeight);
        if(mapScript.isRoad(x,y)){
          pos = new Vector2(x, y);
          this.transform.position = mapScript.convertRealPosition(x,y);
            break;
        }
      }
    }

    public void stopMove(){
      rb.velocity = Vector3.zero;
      isMoving = false;
      Animation();
    }

    public void move(float input){
      //カメラの向きを取得
      float camRotation = cam.transform.localEulerAngles.y;
      // カメラの向きと入力内容から移動方向を決定
      float direction = (camRotation-input)*Mathf.Deg2Rad;
      // 移動方向から各座標の移動量を決定
      float vX = Mathf.Sin(direction)*speed;
      float vY = Mathf.Cos(direction)*speed;
      // 移動する
      rb.velocity = new Vector3(vX,0,vY);          
      // 移動方向に合わせてキャラを回転
      this.transform.localEulerAngles = new Vector3(0,direction*Mathf.Rad2Deg,0);
      isMoving = true;
      // マップ上の座標を更新
      pos = mapScript.convertMapPosition(this.transform.position);
    }

    public GameObject performObject(){//目の前にタスクがあるか確認,あったらタスク起動
      Vector3 realThisPos = this.transform.position;
      float thisRotation = this.transform.localEulerAngles.y*Mathf.Deg2Rad;
      Vector3 rayDirection = new Vector3(Mathf.Sin(thisRotation), 0, Mathf.Cos(thisRotation));
      Ray ray = new Ray(realThisPos, rayDirection);
      RaycastHit hit;
      if(Physics.Raycast(ray, out hit, 1.5f)){
        return hit.collider.gameObject;
      }
      return null;
    }
    public void SetMine(){
      GameObject newMine = mapScript.putMine(this.transform.position, this.transform.localEulerAngles.y*Mathf.Deg2Rad, this);
      if(newMine == null)
        return;
      Mine newMineSc = newMine.GetComponent<Mine>();
      mineList.Add(newMineSc);
      mineCounter++;
    }

    public void RemoveMine(Mine m){
      mineList.Remove(m);
    }

    public List<Mine> getMineList(){
      return mineList;
    }

    public void Animation(){
      myAnimator.SetBool("isMoving",isMoving);
    }
}
