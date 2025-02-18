using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]  //スクリプトの実行順序を宣言。数値が小さいほど優先される
public class MapMaster : MonoBehaviour
{
  public int mapWidth;
  public int mapHeight;
  private float originPointX;
  private float originPointY;
  private int seed;
  [SerializeField] private GameObject wall;
  [SerializeField] private GameObject road;
  [SerializeField] private GameObject task;
  [SerializeField] private GameObject mine;
  private IMain main;
  private int[,] map;
  private int TaskQuantity;

  public void create(IMain m, int _seed){
    main = m;
    if(mapWidth % 2 == 0)
      mapWidth -= 1;
    if(mapHeight % 2 == 0)
      mapHeight -= 1;
    map = new int[mapHeight, mapWidth];
    //マップ左上の座標(originPoint)を定義
    float originPointX = (mapWidth /2.0f)-0.5f;//マップサイズが奇数なので0.5引く
    float originPointY = (mapHeight/2.0f)-0.5f;

    Random.InitState(_seed);
    seed = _seed;
    Debug.Log("マップのシード値；" + seed);//シード値をログに出力
    clearMap();
    createMap();
    setMap();//マップをゲーム画面に作成
  }

  private void clearChild(){
    //子オブジェクト（壁など）が既にあれば削除
    foreach ( Transform n in gameObject.transform ){
      GameObject.Destroy(n.gameObject);
    }
  }

  private void clearMap(){
    // マップを初期化
    clearChild();
    for(int y = 0; y < mapHeight; y++){
      for(int x = 0; x < mapWidth; x++){
        if(x == 0 || x == mapWidth-1 || y == 0 || y == mapHeight-1)
          map[y, x] = 1;
        else
          map[y, x] = 0;
      }
    }
  }
  private void createMap(){
    setStick();//棒倒し法を応用して壁を作る
    TaskQuantity = (mapWidth*mapHeight)/100;
    setTask(TaskQuantity);
  }
  public int getTaskQuantity(){
    return TaskQuantity;
  }
  private void setStick(){  // 棒倒し法の手順-1 (棒を立てる)
    for(int y = 2; y < mapHeight-1; y = y+2){
      for(int x = 2; x < mapWidth-1; x = x+2){
        map[y, x] = 1;//指定座標に壁を作る
        dropStick(x, y);//壁を伸ばす方向を決める
      }
    }
  }
  private void dropStick(int x, int y){  // 棒倒し法の手順-2 (棒を倒す)
    if(Random.Range(0,4) == 0)
     return;//一定確率で棒を倒さない(道を作るため)
    if(map[y, x] == 1){
      int[,] d = new int[4,2]{
        {-1, 0},
        { 0, 1},
        { 1, 0},
        { 0,-1},
      };
      int randDirection;
      if(y == 1)
        randDirection = Random.Range(0,4);
      else
        randDirection = Random.Range(0,3);
      map[y+d[randDirection, 1], x+d[randDirection, 0]] = 1;
    }
  }
  private void setMap(){
    for(int y = 0; y < mapHeight; y++){
      for(int x = 0; x < mapWidth; x++){
        if(map[y, x] == 1){
          putWall(x, y);
          continue;
        }
        else if(map[y, x] == 2){
          putTask(x,y);
        }
        putRoad(x, y);
      }
    }
  }
  private void setTask(int limit){
    int count = 0;
    while(true){
      int x = Random.Range(1,mapWidth-1);
      int y = Random.Range(1,mapHeight-1);
      if(isWall(x, y)){
        if(Random.Range(0,4) == 0 && !((isWall(x,y+1)||isWall(x,y-1)) && (isWall(x+1,y)||isWall(x-1,y)))){
          for(int i = 0; i < 9; i++){//周りにすでにタスクがある場合は別の場所を探す
            int _x = x + (-1 + (i % 3));
            int _y = y + (-1 + (i / 3));
            if(isTask(_x, _y)){
              setTask(limit - count);
              return;
            }
          }
          if(++count > limit)
            return;
          map[y, x] = 2;
        }
      }
    }
  }
  private void putRoad(int x, int y){
    Vector3 pos = convertRealPosition(x, y);
    pos.y = -0.5f;
    Instantiate(road, pos, Quaternion.identity, this.transform);
  }
  private void putWall(int x, int y){
    Vector3 pos = convertRealPosition(x, y);
    pos.y = 0.5f;
    Instantiate(wall, pos, Quaternion.identity, this.transform);
  }
  private void putTask(int x, int y){//タスクブロックの配置
    Vector3 pos = convertRealPosition(x, y);
    pos.y = 0.5f;
    GameObject TaskClone = Instantiate(task, pos, Quaternion.identity, this.transform);
    TaskClone.GetComponent<Task>().set(main,new Vector2(x,y));
  }
  public GameObject putMine(Vector3 target, float dir, Player p){
    target.x += Mathf.Sin(dir);
    target.z += Mathf.Cos(dir);
    target = convertMapPosition(target);
    if(!isRoad((int)target.x, (int)target.y) || isMine((int)target.x, (int)target.y)) {
      Debug.Log("[" + (int)target.x + ", " + (int)target.y + "]は設置不可");
      return null;
    }
    GameObject newMine = Instantiate(mine, convertRealPosition(target.x, target.y), Quaternion.identity, this.transform);
    newMine.transform.eulerAngles = new Vector3(0, 45,0);
    map[(int)target.y, (int)target.x] = 3;
    newMine.GetComponent<Mine>().set((int)target.x, (int)target.y, p, this);
    return newMine;
  }
  public void deleteMine(int x, int y){
     map[y, x] = 0;
  }
  //unity上のワールド座標に変換
  public Vector3 convertRealPosition(float x, float y){
    return (new Vector3( x-(originPointX), 0, -(y-(originPointY))));
  }
  public Vector3 convertRealPosition(Vector2 p){
    return convertRealPosition(p.x, p.y);
  }
  //マップ上の座標に変換
  public Vector2 convertMapPosition(Vector3 p){
    float mapX = Rounding(p.x+(originPointX));
    float mapY = -Rounding(p.z-(originPointY));
    return new Vector2(mapX, mapY);
  }
  //最も近い整数に直す（四捨五入に近い）
  private float Rounding(float num){
    if(Mathf.Abs(num)%1 >= 0.5)
      num += Mathf.Sign(num);
    return num - num%1;
  }
  public bool isRoad(int x, int y){
    if(x < 0 || mapWidth <= x || y < 0 || mapHeight <= y)
      return false;
    return (map[y, x] == 0 || map[y, x] == 3);
  }
  public bool isWall(int x, int y){
    if(x < 0 || mapWidth <= x || y < 0 || mapHeight <= y)
      return true;
    return (map[y, x] == 1);
  }
  public bool isTask(int x, int y){
    if(x < 0 || mapWidth <= x || y < 0 || mapHeight <= y)
      return false;
    return (map[y, x] == 2);
  }
  public bool isMine(int x, int y){
    if(x < 0 || mapWidth <= x || y < 0 || mapHeight <= y)
      return false;
    return (map[y, x] == 3);
  }
}
