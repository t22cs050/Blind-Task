using System.Collections.Generic;//Listを使用する為追加
using UnityEngine;
using UnityEngine.UI; //UIを制御する為追加

public class CatchTheBall : MonoBehaviour,IMiniGame{
    private Text progress;
    private int targetBallCount;
    [SerializeField] private GameObject ball;
    // ボールを格納するリスト
    private List<Ball> balls = new List<Ball>();
    private Bar bar;

    // クリアされたか
    private bool isClear = false;
    private int createdBallCount = 0;

    void IMiniGame.Initialize(int n)
    {
        Text taskNum = this.transform.GetChild(0).transform.GetChild(0).GetComponent<Text> ();
        taskNum.text = "Task: " + n;

        targetBallCount = Random.Range(3, 6);
        progress = this.transform.GetChild(0).transform.GetChild(3).GetComponent<Text> ();
        progress.text = "あと" + targetBallCount + "個";

        GameObject barObj = this.transform.GetChild(0).transform.GetChild(1).gameObject;
        bar = new Bar(barObj);
    }

    bool IMiniGame.IsCleared()
    {
        return isClear;
    }

    void IMiniGame.Play()
    {
        ballUpdate();
        barUpdate();
    }

    // ボールの更新処理
    private void ballUpdate(){
        if(balls.Count == 0){
            CreateBall(Random.Range(1, 4));
        }
        else{
            for (int i = 0; i < balls.Count; i++){
                balls[i].move();
            }
        }
    }

    // バーの更新処理
    private void barUpdate(){
        int input = 0;
        if(Input.GetKeyDown(KeyCode.A)){
            input = -1;
        }
        else if(Input.GetKeyDown(KeyCode.D)){
            input = 1;
        }
        bar.move(input);
    }

    //ボールの生成
    public void CreateBall(int numberOfBalls){
        // 生成するボールの数を制限
        if (numberOfBalls >= 4) {
            numberOfBalls = 4;
        }
        else if (numberOfBalls <= 0) {
            numberOfBalls = 1;
        }

        // ボールの生成位置を格納するリスト
        List<int> xPositions = new List<int>(numberOfBalls);
        for (int i = 0; i < numberOfBalls; i++) {
            // 生成位置のX座標
            int xPosition;
            // 重複しないようにする
            bool isDuplicate = true;
            do{
                // -2~2の範囲でランダムなX座標を取得
                xPosition = Random.Range(-2, 3);
                // 重複チェック
                if (!xPositions.Contains(xPosition))
                {
                    xPositions.Add(xPosition);
                    isDuplicate = false;
                }
            } while (isDuplicate);
            // 初期位置を設定
            Vector2 initialPosition = new Vector2(xPosition, 70);
            // ボールのゲームオブジェクトを生成
            GameObject newBall = Instantiate(ball, this.transform.GetChild(0).transform);
            Ball ballInstance = new Ball(initialPosition, this, newBall, createdBallCount+i+1, i==0);
            // 生成したボールをリストに追加
            balls.Add(ballInstance);
        }
        //生成したボールの数をカウント
        createdBallCount += numberOfBalls;
    }

    //ボールの削除
    public void DestroyBall(GameObject DestroyBallObj, int id){
        GameObject.Destroy(DestroyBallObj);
        for(int i = 0; i < balls.Count; i++){
            if(balls[i].getId() == id){
                balls.RemoveAt(i);
                break;
            }
        }
    }

    // バーの位置を取得
    public int GetBarXPos(){
        return bar.getXPos();
    }

    public void addScore()
    {
        targetBallCount--;
        progress.text = "あと" + targetBallCount + "個";
        if(targetBallCount == 0){
            isClear = true;
        }
    }
}

class Ball{
    private Vector2 pos;

    private float speed = 60;

    private CatchTheBall parent;

    private GameObject ball;

    // ボールを生成したか
    private bool isCreated = false;
    private int myId;
    private bool isFirstBorn;

    public Ball(Vector2 initialPosition, CatchTheBall _parent, GameObject _ball, int _id, bool _firstBorn)
    {
        // ボールの初期位置を設定
        pos = initialPosition;
        // 親オブジェクトを設定
        parent = _parent;
        // ボールのゲームオブジェクトを設定
        ball = _ball;
        // X座標を調整
        pos.x *= 75;
        // ボールのIDを設定
        myId = _id;
        // 最初に生成されたボールかどうかを設定
        isFirstBorn = _firstBorn;  
        // ボールの速度をランダムで決める
        speed = Random.Range(50, 200);
        // ボールの位置を設定
        ball.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    public void move(){
        ball.GetComponent<RectTransform>().anchoredPosition = pos;
        if(ShouldCreateNewBall()){
            isCreated = true;
            // ボールの数を1~4のランダムで生成
            int randomBallCount = Random.Range(1, 5);
            parent.CreateBall(randomBallCount);
        }
        if(pos.y < -29){
            if(pos.x == parent.GetBarXPos()){
                parent.addScore();
            }
            parent.DestroyBall(ball, myId);
        }
        pos.y -= speed*Time.deltaTime;
    }

    //新しいボールをいま生成するかしないか決める
    private bool ShouldCreateNewBall()
    {
        return pos.y < 0 && !isCreated && isFirstBorn && Random.Range(0, 60) == 0;
    }

    public int getId()
    {
        return myId;
    }
}
class Bar{
    private int xPos;
    private GameObject bar;
    public Bar(GameObject _bar){
        xPos = 0;
        bar = _bar;
    }
    public void move(int input){
        // バーの移動処理
        if(input == 1){
            xPos++;
        }
        else if(input == -1){
            xPos--;
        }
        if(xPos < -2){
            xPos = 2;
        }
        else if(xPos > 2){
            xPos = -2;
        }
        bar.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos*75, -50);
    }
    public int getXPos(){
        return xPos*75;
    }
}
