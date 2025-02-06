using UnityEngine;

[DefaultExecutionOrder(3)]  //スクリプトの実行順序を宣言。数値が小さいほど優先される
public class Task : MonoBehaviour
{
  private IMain main;
  private bool isStarted = false;
  private bool cleared = false;
  private Vector2 pos;//マップ上の座標（クリアされたタスクの位置を敵に知らせるため）
  private int miniGameNum;
  [SerializeField] private GameObject[] miniGames;

  //ミニゲームオブジェクトを定義
  private IMiniGame runingMiniGame;

  public void set(IMain m,Vector2 p){
    GetComponent< Renderer>().material.color = Color.red;
    main = m;
    pos = p;
  }

  public void startTask(){
    if(cleared)//一度クリアしたタスクは二度と起動できない
      return;
    main.setRunningTask(this);
    miniGameNum = Random.Range(0,miniGames.Length);
    runingMiniGame = Instantiate(miniGames[miniGameNum], this.transform).GetComponent<IMiniGame>();
    runingMiniGame.Initialize(main.getClearedTaskQuantity()+1);
  }

  public void runTask(){
    if(!isCleared()){
      if(!isStarted){
        isStarted = true;
        return;
      }
      runingMiniGame.Play();
      if(main.getGameOver())
        destroyMiniGame();
      return;
    }
    destroyMiniGame();//ミニゲームウィンドウを削除
    cleared = true;
    ChangeColorToBlue();//タスクの色を青に変更
    main.clearTaskOn(pos);
  }
  //タスクの色を青に変更
  public void ChangeColorToBlue(){
    GetComponent< Renderer>().material.color = Color.blue;
  }
  public bool isCleared(){
    if(!isStarted){
      return false;
    }
    return runingMiniGame.IsCleared();
  }
  public void destroyMiniGame(){
    //生成したミニゲームオブジェクトを削除
    foreach ( Transform n in gameObject.transform ){
      Destroy(n.gameObject);
    }
  }
}

interface IMiniGame{
  bool IsCleared();
  void Initialize(int n);
  void Play();
}
