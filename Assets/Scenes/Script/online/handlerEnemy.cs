//敵側の処理を記述する。
using UnityEngine;
using UnityEngine.SceneManagement;

public class handlerEnemy : IHandler
{
    onlineMain main;
    private CameraControl cam;
    private GameObject player;
    private GameObject enemy;
    private onlineEnemy enemyScript;
    private onlineController online;

    //通信相手の次の位置
    private Vector3 NextPos;

    void IHandler.objectsSetting(onlineMain m){
        main = m;
        cam = m.getMainCam();
        player = m.getPlayer();
        enemy = m.getEnemy();
        enemyScript = enemy.GetComponent<onlineEnemy>();
        online = m.getOnline();
        cam.setMainCam(enemy);
        cam.showAll();
        cam.playerHide();
        NextPos = player.transform.position;
    }
    void IHandler.operate(float d){
        enemyScript.move(d);
    }
    void IHandler.stopMove(){
        enemyScript.stopMove();
    }
    void IHandler.handlerUpdate()
    {
        main.controlVignette(0f);
        //敵の位置を操作
        main.Control();
    }

    //[送信]プレイヤーの位置
    void IHandler.sendPos()
    {
        online.sendPos(enemy.transform.position);
    }
    //[受信]プレイヤーの位置
    void IHandler.ReceivedPos(Vector3 pos)
    {
        player.transform.position = NextPos;
        NextPos = pos;
    }
    //プレイヤーの位置を更新
    void IHandler.updatePosition()
    {
        Vector3 p = player.transform.position;
        p = Vector3.MoveTowards(p, NextPos, 5.5f*Time.deltaTime);
        player.transform.position = p;
    }

    //[送信]プレイヤーの向き
    void IHandler.sendRot()
    {
        online.sendRot(enemy.transform.localEulerAngles.y);
    }
    //[受信]プレイヤーの向き
    void IHandler.ReceivedRot(float rot)
    {
        player.transform.localEulerAngles = new Vector3(0, rot, 0);
    }
    void IHandler.gameOver()
    {
        if(cam.GameOverAngle(player)){
            main.LeaveRoom();
            //ゲームオーバー後の遷移処理
            SceneManager.LoadScene("MainMenu");
        }
    }

    void IHandler.gameClear()
    {
        //プレイヤー側にゲームクリアされたら退出
        main.LeaveRoom();
        //ゲームオーバー後の遷移処理
        SceneManager.LoadScene("MainMenu");
    }

    public void hideOther()
    {
        cam.playerHide();
    }
}