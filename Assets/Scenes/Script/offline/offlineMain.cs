
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //UIを制御する為追加
using System; //DateTimeを使用する為追加
using UnityEngine.SceneManagement; //シーン遷移をする為追加
using UnityEngine.Rendering.PostProcessing; //画面の周りを赤くする為追加

public class offlineMain : MonoBehaviour, IMain
{
  //ゲームオブジェクトの宣言
  [SerializeField] private GameObject mapObj;
  private MapMaster mapSc;
  [SerializeField] private GameObject playerObj;
  private Player plaSc;
  [SerializeField] private GameObject enemyObj;
  private EnemyCpu eneSc;
  [SerializeField] private Canvas canvas;
  [SerializeField] private GameObject mainCam;
  private CameraControl mainCamSc;
  private Task runningTask = null;
  [SerializeField] private List<GameObject> Mines;
  [SerializeField] private GameObject taskUi;
  private GameObject InstedTaskUi = null;

  [SerializeField] private UserInputManager UserInput;
  [SerializeField] private GameObject RemainTaskQuantityUI;

  [SerializeField] private PostProcessProfile postProcessProfile;
  [SerializeField] private GameObject lightObject;

  //サウンド
  [SerializeField] private AudioSource enemyClosingInSE;//敵が近づくと鳴る効果音
  [SerializeField] private AudioSource sirenSE;//敵に居場所がバレると鳴る効果音

  private bool isLightRedBlinking;
  private float lightBlinkStart;

  private bool isGameOver = false;
  private bool isGameClear = false;
  private int clearedTaskQuantity;
  // Start is called before the first frame update
  void Start()
  {
    isGameOver = false;
    //オブジェクトを生成
    mapObj = Instantiate(mapObj, this.transform);
    playerObj = Instantiate(playerObj, this.transform);
    enemyObj = Instantiate(enemyObj, this.transform);
    mainCam = Instantiate(mainCam, this.transform);
    //各オブジェクトのクラスを取得
    mapSc = mapObj.GetComponent<MapMaster>();
    plaSc = playerObj.GetComponent<Player>();
    eneSc = enemyObj.GetComponent<EnemyCpu>();
    mainCamSc = mainCam.GetComponent<CameraControl>();
    //各オブジェクトのクラスを初期化
    objectsSetting();
  }

  private void objectsSetting(){//各オブジェクトの初期設定を行う
    clearedTaskQuantity = 0;
    mapSc.create(this, DateTime.Now.Millisecond);//シード値として現在時刻のミリ秒を指定
    // mapSc.create(this, 1);
    plaSc.setting(mapSc, mainCam);
    eneSc.setting(this, mapSc, plaSc);
    eneSc.setStartPos();
    mainCamSc.setMainCam(playerObj);
    Text text = RemainTaskQuantityUI.GetComponent<Text> ();
    text.text = "残りタスク:" + mapSc.getTaskQuantity();
  }

  private void Control(){
    //入力からプレイヤーが移動する方向を決定
    Vector2 moveDirection = Vector2.zero;
    moveDirection = PlayerControl();
    //入力からカメラの方向を変更
    CameraControl();
    //入力がなければプレイヤーの座標を変更しない
    if(moveDirection != Vector2.zero)
      plaSc.move(Mathf.Atan2(moveDirection.x, moveDirection.y)*Mathf.Rad2Deg);
    //座標変更に合わせてアニメーションを切り替える
    plaSc.Animation();
  }

  private Vector2 PlayerControl(){
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

  public void clearTaskOn(Vector2 taskPos){//タスクをクリアしたときに呼び出される
    runningTask = null;
    eneSc.setPath(taskPos);
    clearedTaskQuantity++;
    if(clearedTaskQuantity == mapSc.getTaskQuantity()){
      isGameClear = true;
      return;
    }
    lightRedBlinkOn();//ライトの赤点滅を開始
    Text text = RemainTaskQuantityUI.GetComponent<Text> ();
    text.text = "残りタスク:" + (mapSc.getTaskQuantity()-clearedTaskQuantity);
  }

  public int getClearedTaskQuantity(){
    return clearedTaskQuantity;
  }

  public void enemyShow(){
    eneSc.enemyShow();
    mainCamSc.showAll();
  }
  public void enemyHide(){
    eneSc.enemyHide();
    mainCamSc.enemyHide();
  }

  public void GameOver(){//ゲームオーバーになった瞬間に呼び出される関数
    isGameOver = true;
    enemyShow();
    lightRedBlinkOff();
    if(runningTask != null){
      runningTask.destroyMiniGame();
      runningTask = null;
    }
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

  //敵が近づいてきたときの画面周りの赤の量と効果音の音量をfにする
  private void controlVignette(float f){
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

  private void setVignetteFromDistance(){//プレイヤーと敵の距離でvignetteの量を決める
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

  private void lightRedBlinkOn(){//ライトの赤点滅を開始
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

  private void serchMine(){//設置されたセンサーを探索
    List<Mine> l = new List<Mine>(plaSc.getMineList());
    foreach (Mine n in l) {
      GameObject onSensor = n.IsCharacterOnSensor();
      if(onSensor != null){
        if(onSensor.name == "Ghost(Clone)"){
          //敵の姿が見えるようにする
          enemyShow();
        }
        else if(onSensor.name == "Player(Clone)"){
          eneSc.GoToPlayer();
          lightRedBlinkOn();//ライトの赤点滅を開始
        }
        n.delete();
      }
    }
  }

  private bool ShouldShowTaskKeyUI(){//タスク開始を誘導するUIを表示するべきなら
    if(plaSc.performObject() != null && plaSc.performObject().name == "Task(Clone)"){
      Task t = plaSc.performObject().GetComponent<Task>();
      //タスクがまだ起動していない　かつ　UIがまだ表示されていない
      if(!t.isCleared() && InstedTaskUi == null)
        return true;
    }
    return false;
  }
  private bool ShouldDeleteTaskKeyUI(){//タスク開始を誘導するUIを消すべきなら
    if(InstedTaskUi == null)//消すUIがそもそも無いならfalse
      return false;

    if(isGameOver || isGameClear)
      return true;

    GameObject p = plaSc.performObject();//プレイヤーの目の前にあるオブジェクトを取得
    if((p == null || p.name != "Task(Clone)"))
      return true;
    if(p != null &&  p.name == "Task(Clone)"){
      Task t = p.GetComponent<Task>();
      if(t.isCleared()){//タスクがクリアされたら削除
        return true;
      }
    }
    return false;
  }

  // Update is called once per frame
  void Update()
  {
    plaSc.stopMove();
    serchMine();
    if(isGameOver){
      if(mainCamSc.GameOverAngle(enemyObj)){
        isGameOver = false;
        enemyHide();
        objectsSetting();
      }
    }
    else if(isGameClear){
      SceneManager.LoadScene("OffLineClear");
    }
    else{
      mainCamSc.move();
      //プレイヤーと敵の距離からvignetteを調整
      setVignetteFromDistance();
      //実行中のタスクがあるときはプレイヤーを操作できない
      if(runningTask == null){
        //タスク開始を誘導するUIを表示するべきなら
        if(ShouldShowTaskKeyUI()){
          GameObject tarTask = plaSc.performObject();
          InstedTaskUi = Instantiate(taskUi);
          InstedTaskUi.GetComponent<TaskUi>().set(tarTask, mainCam);
        }
        Control();
      }
      else{
        runningTask.runTask();
      }
      if(isLightRedBlinking){
        lightRedBlinking();
        if( Time.time-lightBlinkStart >= 3f)
          lightRedBlinkOff();
      }
      eneSc.move();//敵はタスク中でも移動できる
    }
    if(ShouldDeleteTaskKeyUI()) {//タスク開始を誘導するUIを消すべきなら
      GameObject.Destroy(InstedTaskUi);
      InstedTaskUi = null;
    }
  }
}
