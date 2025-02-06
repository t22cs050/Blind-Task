using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(4)]
public class EnemyCpu : MonoBehaviour
{
  private Vector2 nextPos;
  private Vector2 pos;
  [SerializeField]private Vector2 direction = Vector2.zero;
  [SerializeField] RuntimeAnimatorController animeController;
  [SerializeField] private float speed;
  private MapMaster mapScript;
  private offlineMain main;
  private Player PlayerScript;
  //目的地までの経路
  List<Vector2> path;
  private bool visiableEnemy = false;//姿が見えるかどうか（基本は見えない）
  private float vTimer;
  
  [SerializeField] private GameObject PointUi;
  private GameObject InstantiatedPointUi;

  public void setting(offlineMain mainS, MapMaster mapS, Player playerS){
    main = mainS;
    mapScript = mapS;
    PlayerScript = playerS;
    Animator myAnimator = this.GetComponent<Animator>();
    myAnimator.runtimeAnimatorController = animeController;
  }

  public void setStartPos(){//初期位置の設定
    while(true){
      int x = Random.Range(0,mapScript.mapWidth);
      int y = Random.Range(0,mapScript.mapHeight);
      path = new List<Vector2>();
      Vector2 pp = PlayerScript.pos;//Player Position
      if(mapScript.isRoad(x,y) && (Mathf.Abs(x-pp.x) > 3) && (Mathf.Abs(y-pp.y) > 3)){
        setMapPos(new Vector2(x, y));
        nextPos = new Vector2(x, y);
        break;
      }
    }
  }

  //マップ上の座標から実際の座標を変更
  private void setMapPos(Vector2 newPos){
    float x = newPos.x;
    float y = newPos.y;
    pos = new Vector2(x,y);
    this.transform.position = mapScript.convertRealPosition(x,y);
  }

  private void setDestination(){//ランダムに行き先を決める関数
    if(Random.Range(0,3) == 0)//一定確率で(その時点で)プレイヤーのいる位置に来る
      GoToPlayer();
    else
      GotoRandom();
  }

  public void GoToPlayer(){
    Debug.Log("プレイヤーの場所へ行きます");
    int rx,ry;//目的地の座標
    rx = (int)PlayerScript.pos.x;
    ry = (int)PlayerScript.pos.y;
    setPath(new Vector2(rx,ry));
  }
  private void GotoRandom(){
    Debug.Log("ランダムに移動します");
    int rx,ry;//目的地の座標
    while(true){
      rx = Random.Range(0,mapScript.mapWidth);
      ry = Random.Range(0,mapScript.mapHeight);
      if(mapScript.isRoad(rx,ry) && pos != new Vector2(rx,ry))
        break;
    }
    setPath(new Vector2(rx,ry));
  }

  private void setAngle(){
    float y = Mathf.Atan2(direction.x, -direction.y)*Mathf.Rad2Deg;
    this.transform.localEulerAngles = new Vector3(0,y,0);
  }

  public void move(){
    if(visiablePlayer()){//もしプレイヤーを見つけたら
      main.GameOver();
      return;
    }
    float delaPos = speed*Time.deltaTime;//このフレームでの移動量(移動がかくかくするのを防ぐため)
    Vector2 beforePos = pos;
    pos = Vector2.MoveTowards(pos, nextPos, delaPos);
    if(pos == nextPos){
      createNewNextPos();//nextPosを更新
    }
    pos = Vector2.MoveTowards(pos, nextPos, delaPos-Vector2.Distance(beforePos,pos));
    setMapPos(pos);
  }

  private void createNewNextPos(){//nextPosを更新
    if(path == null || path.Count == 0){//もし行き先がなかったら
      setDestination();//ランダムに行き先とそこへの経路を決める
    }
    traceRoute();//経路を辿る
  }

  private void traceRoute() {
    if(path.Count <= 0)
      return;
    nextPos = path[0];
    path.RemoveAt(0);
    direction = vectorToDirection(nextPos);
    setAngle();
  }

  private Vector2 vectorToDirection(Vector2 next){//現在地からnextの向きを返す
    if(next.x < pos.x) return new Vector2(-1,0); // 右
    else if(pos.x < next.x) return new Vector2(1,0); // 左
    else if(pos.y < next.y)return new Vector2(0,1); // 下
    else return new Vector2(0,-1); // 上
  }

  public void enemyHide(){
    GameObject.Destroy(InstantiatedPointUi);
    InstantiatedPointUi = null;
    visiableEnemy = false;
  }
  public void enemyShow(){
    Debug.Log("enemyShow");
    vTimer = 20;
    if(InstantiatedPointUi == null){
      InstantiatedPointUi = Instantiate(PointUi, main.getCanvas().transform);
      InstantiatedPointUi.GetComponent<EnemyPointer>().set(
        this.gameObject,
        main.getMainCam().gameObject.GetComponent<Camera>()
      );
    }
    visiableEnemy = true;
  }

  private bool visiablePlayer(){//プレイヤーが目の前に居るか判断
    float distance = 6f;
    if(visiableEnemy){
      visiableCountDown();
      distance = 3f;//敵が見える間は敵の視野範囲が短い
    }
    Vector3 realThisPos = this.transform.position;
    Vector3 rayDirection = new Vector3(direction.x, 0, -direction.y);
    Ray ray = new Ray(realThisPos, rayDirection);
    RaycastHit hit;
    if(Physics.Raycast(ray, out hit, distance) && hit.collider.gameObject.tag == "Player"){
      return true;
    }
    return false;
  }
  private void visiableCountDown(){
    vTimer -= Time.deltaTime;
    if(vTimer <= 0){
      visiableEnemy = false;
      main.enemyHide();
    }
  }
  public void setPath(Vector2 p){
      path = new AStar().Exploration(mapScript, nextPos, p);
  }
  protected MapMaster getMap(){
    return mapScript;
  }
  protected IMain getMain(){
    return main;
  }
  protected float getSpeed(){
    //想定外の値にならないようにする
    if(speed > 0)
      return speed;
    //想定外の値だった場合、適当な値を返す
    return 2.5f;
  }
  protected void setDirection(Vector3 nextPos, Vector3 currentPos){
    if(nextPos.x < currentPos.x) 
      direction = new Vector2(-1,0); // 右
    else if(currentPos.x < nextPos.x)
      direction = new Vector2(1,0); // 左
    else if(currentPos.z < nextPos.z)
      direction = new Vector2(0,-1); // 上
    else
      direction = new Vector2(0,1); // 下
  }
  protected Vector2 getDirection()
    {
        return direction;
    }
}
