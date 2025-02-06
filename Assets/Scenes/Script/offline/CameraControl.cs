using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(3)]  //スクリプトの実行順序を宣言。数値が小さいほど優先される
public class CameraControl : MonoBehaviour
{
  private Vector3 thisPos;
  private Vector3 PlayerPos;
  private Vector3 rotation;
  private float nextRotationY;
  [SerializeField]private int radius = 10;
  [SerializeField]private int height = 8;
  [SerializeField]private float rotateSpeed = 0.05f;
  [SerializeField] private GameObject Player;

  private bool deltaGameOver = false;
  private float gameOverRadius;
  private Vector3 deltaThisPos;
  private Camera thisCam;

  public void setMainCam(GameObject p){
    Player = p;
    rotation = Vector3.zero;
    deltaGameOver = false;
    rotation.y = 180;
    thisCam = GetComponent<Camera>();
    enemyHide();
  }

  private void followPlayer(){
    thisPos = PlayerPos + new Vector3(0,height,0);
  }
  public void aroundPlayer(float input){
    if(input == 0.0f){
      fixAngle();
    }
    rotation.y += (float)input*rotateSpeed*Time.deltaTime;
    thisPos.x  += Mathf.Sin(rotation.y*Mathf.Deg2Rad)*radius;
    thisPos.z  += Mathf.Cos(rotation.y*Mathf.Deg2Rad)*radius;
    transform.rotation = Quaternion.LookRotation(PlayerPos-thisPos, Vector3.up);
    transform.position = thisPos;
  }
  //カメラの角度を入力がない間に修正。45度刻みで近い角度にする。
  private void fixAngle(){
    nextRotationY = rotation.y;
    nextRotationY = Mathf.Round(nextRotationY / 45.0f) * 45.0f;
    rotation.y = Mathf.MoveTowards(rotation.y,nextRotationY,Time.deltaTime*rotateSpeed);
    this.transform.localEulerAngles = rotation;
  }
  public void move(){
    PlayerPos = Player.transform.position;
    followPlayer();
  }
  public float getRotate(){
    return rotation.y;
  }
  public bool GameOverAngle(GameObject enemy){
    // カメラの回転を取得
    if(rotation.x == 90f){
        return true;
    }
    rotation = this.transform.localEulerAngles;

    // プレイヤーと敵の位置を取得
    PlayerPos = Player.transform.position;
    Vector3 targetPosition = Vector3.Lerp(PlayerPos, enemy.transform.position, 0.5f); // プレイヤーと敵の中間点

    if(!deltaGameOver){
      // 現在のカメラ位置と目標位置の距離（半径）
      gameOverRadius = Vector3.Distance(this.transform.position, targetPosition);
      deltaThisPos = thisPos;
      deltaGameOver = true;
    }

    // カメラの回転を徐々に90度に向かって移動
    rotation.x = Mathf.MoveTowards(rotation.x, 90, Time.deltaTime * rotateSpeed / 3f);

    // 弧を描くようにカメラの位置を計算
    float heightOffset = Mathf.Sin(rotation.x*Mathf.Deg2Rad) * gameOverRadius;
    float horizontalOffset = Mathf.Cos(rotation.x*Mathf.Deg2Rad) * gameOverRadius;

    Vector3 directionToTarget = (deltaThisPos - targetPosition).normalized;

    // カメラの新しい位置を設定
    thisPos.x = targetPosition.x + directionToTarget.x * horizontalOffset;
    thisPos.y = targetPosition.y + heightOffset;
    thisPos.z = targetPosition.z + directionToTarget.z * horizontalOffset;

    // カメラの回転と位置を更新
    this.transform.position = thisPos;
    this.transform.localEulerAngles = rotation;

    return false;
  }

  //敵を見えないようにする
  public void enemyHide(){
    thisCam.cullingMask = ~(1<<3);//"Enemy"のレイヤー番号は3
  }
  //プレイヤーを見えないようにする
  public void playerHide(){
    thisCam.cullingMask = ~(1<<7);//"Player"のレイヤー番号は7
  }

  //敵、プレイヤーなどの全オブジェクトを見えるようにする
  public void showAll(){
    thisCam.cullingMask = -1;//全てのレイヤーを有効にする
  }
}
