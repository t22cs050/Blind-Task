using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //UIを制御する為追加
using System; //DateTimeを使用する為追加
using UnityEngine.Rendering.PostProcessing;
using System.Collections; //画面の周りを赤くする為追加

public class onlineMain : MonoBehaviour,IMain
{
  private IHandler handler;
  //ゲームオブジェクトの宣言
  [SerializeField] private GameObject mapObj;
  private MapMaster mapSc;
  [SerializeField] private GameObject playerObj;
  private Player plaSc;
  [SerializeField] private GameObject enemyObj;
  private onlineEnemy eneSc;
  [SerializeField] private Canvas canvas;
  [SerializeField] private GameObject mainCam;
  private CameraControl mainCamSc;
  private Task runningTask = null;
  [SerializeField] private List<GameObject> Mines;
  [SerializeField] private GameObject taskUi;
  private GameObject InstedTaskUi = null;

  [SerializeField] private UserInputManager UserInput;
  [SerializeField] private onlineController online;
  [SerializeField] private GameObject RemainTaskQuantityUI;

  [SerializeField] private PostProcessProfile postProcessProfile;
  [SerializeField] private GameObject lightObject;

  //サウンド
  [SerializeField] private AudioSource enemyClosingInSE;//敵が近づくと鳴る効果音
  [SerializeField] private AudioSource sirenSE;//敵に居場所がバレると鳴る効果音

  private bool isLightRedBlinking;
  private float lightBlinkStart;

  private bool isGameStart = false;
  private bool isGameOver = false;
  private bool isGameClear = false;
  private int clearedTaskQuantity;

  //位置を知らせるUI
  [SerializeField] private GameObject PointUi;
  //生成されたPointUi
  private GameObject InstantiatedPointUi;
  //PointUiが存在している時間
  private float pointUiTimer;

  //位置を知らせるUIの表示時間
  private const int vTimer = 15;
  private float gameStartTime;
  [SerializeField]private GameObject otherLeftMessage;

    void Start()
  {
    isGameStart = false;
    isGameOver = false;
    isGameClear = false;
    //オブジェクトを生成
    mapObj = Instantiate(mapObj, this.transform);
    playerObj = Instantiate(playerObj, this.transform);
    enemyObj = Instantiate(enemyObj, this.transform);
    mainCam = Instantiate(mainCam, this.transform);
    //各オブジェクトのクラスを取得
    mapSc = mapObj.GetComponent<MapMaster>();
    plaSc = playerObj.GetComponent<Player>();
    eneSc = enemyObj.GetComponent<onlineEnemy>();
    mainCamSc = mainCam.GetComponent<CameraControl>();
  }

  /*
  マップのシード値を設定。
  * マスタークライアントならシード値を自身で決定
  * そうでなければシード値をマスタークライアントから受信する
  */
  public void setMap(){
    if(!online.isMaster())
      return;
    //シード値として現在時刻のミリ秒を指定
    int seed = DateTime.Now.Millisecond;
    //シード値を送信
    online.sendSeed(seed);
    //シード値をもとにマップを生成
    mapSc.create(this, seed);
    //その他のオブジェクトも各初期設定をする
    objectsSetting();
  }
  public void ReceivedSeed(int seed){
    //シード値をもとにマップを生成
    mapSc.create(this, seed);
    //その他のオブジェクトも各初期設定をする
    objectsSetting();
  }

  //各オブジェクトのクラスを初期化
  private void objectsSetting(){//各オブジェクトの初期設定を行う
    isGameStart = true;
    clearedTaskQuantity = 0;
    plaSc.setting(mapSc, mainCam);
    eneSc.setting(this, mapSc, plaSc, mainCam);
    eneSc.setStartPos();

    if(online.isMaster())
      handler = new handlerPlayer();
    else
      handler = new handlerEnemy();
    handler.objectsSetting(this);

    Text text = RemainTaskQuantityUI.GetComponent<Text> ();
    text.text = "残りタスク:" + mapSc.getTaskQuantity();
    
    gameStartTime = Time.time;
  }
  public void Update(){
    //各初期設定が終わるまでは処理しない
    if(!isGameStart)
      return;
    //操作キャラクターの動きを停止
    handler.stopMove();

    if(isGameOver){
      //ゲームオーバー時の処理
      handler.gameOver();
      return;
    }
    if(isGameClear){
      //ゲームクリア時の処理
      handler.gameClear();
      return;
    }

    //ゲーム本編での処理

    //各プレイヤーの処理を実行
    handler.handlerUpdate(); 

    //点滅中ならライトの赤点滅を行う
    if(isLightRedBlinking){
        lightRedBlinking();
        if( Time.time-lightBlinkStart >= 3f)
          lightRedBlinkOff();
    }
    if(InstantiatedPointUi != null){
      //時間をカウント
      UpdatePointUiTimer();
    }
    
    handler.updatePosition();
    handler.sendPos();
    handler.sendRot();
    
    //カメラの動きを更新
    mainCamSc.move();
  }

    public bool ShouldShowTaskKeyUI(){//タスク開始を誘導するUIを表示するべきなら
      if(plaSc.performObject() != null && plaSc.performObject().name == "Task(Clone)"){
        Task t = plaSc.performObject().GetComponent<Task>();
        //タスクがまだ起動していない　かつ　UIがまだ表示されていない
        if(!t.isCleared() && InstedTaskUi == null)
          return true;
      }
    return false;
  }
  public bool ShouldDeleteTaskKeyUI(){//タスク開始を誘導するUIを消すべきなら
    if(InstedTaskUi == null)//消すUIがそもそも無いならfalse
      return false;

    if(isGameOver || isGameClear)
      return true;

    GameObject p = plaSc.performObject();//プレイヤーの目の前にあるオブジェクトを取得
    if(p == null || p.name != "Task(Clone)")//目の前にタスクが無いもしくはタスク以外なら消す
      return true;
    else if(p != null &&  p.name == "Task(Clone)"){//目の前にタスクがあるがクリア済みなら消す
      Task t = p.GetComponent<Task>();
      if(t.isCleared()){//タスクがクリアされたら
        return true;
      }
    }
    return false;
  }

    //敵が近づいてきたときの画面周りの赤の量と効果音の音量をfにする
    public void controlVignette(float f){
    Vignette vignette;
    foreach (PostProcessEffectSettings item in postProcessProfile.settings){
      if (item as Vignette){
        vignette = item as Vignette;
        vignette.color.value = Color.red;
        vignette.intensity.value = f;
        enemyClosingInSE.volume = f;
      }
    }
  }

  public void setVignetteFromDistance(){//プレイヤーと敵の距離でvignetteの量を決める
    //dにプレイヤーと敵の距離を格納
    float d = Vector3.Distance(playerObj.transform.position, enemyObj.transform.position);
    if(d <= 6f){//敵の最大視野範囲が6
      controlVignette(0.5f);
    }
    else if(d <= 10f){
      controlVignette((10f-d)/8f);
    }
    else
      controlVignette(0f);
  }

  public void Control(){
    //入力から操作キャラが移動する方向を決定
    Vector2 moveDirection = Vector2.zero;
    moveDirection = PlayerControl();
    //入力からカメラの方向を変更
    CameraControl();
    //入力がなければ操作キャラの座標を変更しない
    if(moveDirection != Vector2.zero)
      handler.operate(Mathf.Atan2(moveDirection.x, moveDirection.y)*Mathf.Rad2Deg);
    //座標変更に合わせてアニメーションを切り替える
    plaSc.Animation();
  }
  private Vector2 PlayerControl(){
    //オフラインメインと処理同じなので継承などで統一したい
    Vector2 moveDirection = Vector2.zero;
    if(UserInput.PressMoveUp()){
      moveDirection.y = 1;
    }
    if(UserInput.PressMoveDown()){
      moveDirection.y = -1;
    }
    if(UserInput.PressMoveRight()){
      moveDirection.x = -1;
    }
    if(UserInput.PressMoveLeft()){
      moveDirection.x = 1;
    }

    if(!online.isMaster())
      return moveDirection;
    // マスタークライアントの場合のみ以下の処理を行う 

    if(UserInput.PressDownConfirm()){
      GameObject tarTask = plaSc.performObject();//プレイヤーの目の前にあるオブジェクトを取得
      if(tarTask != null && tarTask.name == "Task(Clone)"){//目の前にタスクがあれば
        Task taskScript = tarTask.GetComponent<Task>();//タスクのスクリプトを取得
        if(!taskScript.isCleared())//クリアされていないタスクであれば
          taskScript.startTask();//タスクの準備
      }
    }
    if(UserInput.PressDownMine()){
      plaSc.SetMine();
    }
    return moveDirection;
  }

  private void CameraControl(){
    if(Input.GetKey (KeyCode.LeftArrow))
      mainCamSc.aroundPlayer(1.0f);
    else if(Input.GetKey (KeyCode.RightArrow))
      mainCamSc.aroundPlayer(-1.0f);
    else
      mainCamSc.aroundPlayer(0.0f);
  }
  
  public void setRunningTask(Task t){
    runningTask = t;
  }
  public Task getRunningTask() {
    if(runningTask == null)
      return null;
    return runningTask;
  }

  // タスクがクリアしたたびに呼ばれる関数(プレイヤー側)
  public void clearTaskOn(Vector2 taskPos){
    runningTask = null;
    // eneSc.setPath(taskPos);
    clearedTaskQuantity++;
    if(clearedTaskQuantity == mapSc.getTaskQuantity()){
      isGameClear = true;
      //クリアしたタスクの位置を敵に知らせる
      online.sendTaskClear(taskPos);
      //リザルトシーンにデータを格納
      GameEnd(true);
      return;
    }
    lightRedBlinkOn();//ライトの赤点滅を開始
    Text text = RemainTaskQuantityUI.GetComponent<Text> ();
    text.text = "残りタスク:" + (mapSc.getTaskQuantity()-clearedTaskQuantity);

    //クリアしたタスクの位置を敵に知らせる
    online.sendTaskClear(taskPos);
  }
  
  public void lightRedBlinkOn(){//ライトの赤点滅を開始
    lightBlinkStart = Time.time;
    isLightRedBlinking = true;
    sirenSE.Play();
  }
  private void lightRedBlinking(){//ライトが赤く点滅をする
    float t = (Time.time-lightBlinkStart)*5;
    Color lightColor = new Color(1f, Mathf.Sin(t), Mathf.Sin(t));
    lightObject.GetComponent<Light>().color = lightColor;
  }
  private void lightRedBlinkOff(){//ライトの赤点滅を終了
    isLightRedBlinking = false;
    lightObject.GetComponent<Light>().color = new Color(1f,1f,1f);
  }  

  public int getClearedTaskQuantity(){
    return clearedTaskQuantity;
  }
  public void SendGameOver(){
    handler.sendPos();
    handler.sendRot();
    online.sendGameOver();
    GameOver();
  }
  public void ReceivedGameOver(){
    GameOver();
  }
  public void GameOver(){
    isGameOver = true;
    mainCamSc.showAll();
    lightRedBlinkOff();
    if(runningTask != null){
      runningTask.destroyMiniGame();
      runningTask = null;
    }
    GameEnd(false);
  }

  private void GameEnd(bool isPlayerWon){
    Result r = new Result();
    r.setTime(Time.time - gameStartTime);
    r.setIsPlayerWon(isPlayerWon);
  }

  public bool getGameOver(){
    return isGameOver;
  }
  public CameraControl getMainCam(){
    return mainCamSc;
  }
  public Canvas getCanvas(){
    return canvas;
  }
  public GameObject getPlayer(){
    return playerObj;
  }
  public GameObject getEnemy(){
    return enemyObj;
  }

  public onlineController getOnline(){
    return online;
  }
  public void ReceivedPos(Vector3 pos){
    handler.ReceivedPos(pos);
  }

  public void ReceivedRot(float rot){
      handler.ReceivedRot(rot);
  }

  /// プレイヤー側がタスクをクリアした（敵側専用）
  public void ReceivedTaskClear(Vector2 taskPos)
  {
    //念のため、敵側の処理であることを明記する。
    if(online.isMaster())
      return;

    clearedTaskQuantity++;
    if(clearedTaskQuantity == mapSc.getTaskQuantity()){
      isGameClear = true;
      GameEnd(true);
      return;
    }
    lightRedBlinkOn();//ライトの赤点滅を開始
    //クリアしたタスクの位置を敵に知らせる
    InitializeEnemyPointer(mapSc.convertRealPosition(taskPos.x, taskPos.y));
    Text text = RemainTaskQuantityUI.GetComponent<Text> ();
    text.text = "残りタスク:" + (mapSc.getTaskQuantity()-clearedTaskQuantity);
  }

  public void InitializeEnemyPointer(Vector3 target){
    pointUiTimer = 0;
    if(InstantiatedPointUi == null){
      InstantiatedPointUi = Instantiate(PointUi, canvas.transform);
    }
    InstantiatedPointUi.GetComponent<EnemyPointer>().set(
      target,
      getMainCam().gameObject.GetComponent<Camera>()
    );
  }
  public void InitializeEnemyPointer(GameObject obj){
    pointUiTimer = 0;
    if(InstantiatedPointUi == null){
      InstantiatedPointUi = Instantiate(PointUi, canvas.transform);
    }
    InstantiatedPointUi.GetComponent<EnemyPointer>().set(
      obj,
      getMainCam().gameObject.GetComponent<Camera>()
    );
  }
  private void UpdatePointUiTimer() {
    pointUiTimer += Time.deltaTime;
    if (pointUiTimer >= vTimer) {
        Destroy(InstantiatedPointUi);
        InstantiatedPointUi = null;
        handler.hideOther();
    }
  }
  public GameObject InstantiateTask(){
    InstedTaskUi = Instantiate(taskUi);
    return InstedTaskUi;
  }

  public void destroyTaskUi()
  {
      Destroy(InstedTaskUi);
      InstedTaskUi = null;
  }

  public void LeaveRoom()
  {
    online.LeaveRoom();
  }

  //センサーを踏んだ時の処理(敵側のみの処理)
  public void ReceivedOnSensor(bool isEnemy)
  {
    //自分が踏んでしまったのであれば
    if(isEnemy){
      // 時間があれば
      // 「センサーを踏んでしまいました」
      // みたいなカットイン処理を実装したい
      return;
    }
    //そうでなければ（相手が踏んだのなら）

    // プレイヤーの位置を矢印で表示
    InitializeEnemyPointer(playerObj);
    lightRedBlinkOn();//ライトの赤点滅を開始
    // プレイヤーの姿を見せる
    mainCamSc.showAll();
  }
  
  public void ReceivedEnemyHide()
  {
    mainCamSc.enemyHide();
  }

  public void ReceivedPlayerHide()
  {
    mainCamSc.playerHide();
  }

  public void ReceivedShowAll()
  {
    mainCamSc.showAll();
  }

  public void LeftRoom(){
    if(isGameClear || isGameClear)
      return;
    otherLeftMessage.SetActive(true);
    StartCoroutine(WaitAndChangeScene());
  }

  private IEnumerator WaitAndChangeScene(){
    yield return new WaitForSeconds(3);
    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
  }
}
