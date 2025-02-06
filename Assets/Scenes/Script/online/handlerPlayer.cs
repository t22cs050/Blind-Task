//プレイヤー側の処理を記述する。
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class handlerPlayer : IHandler
{
    private onlineMain main;
    private CameraControl cam;
    private onlineController online;
    private GameObject player;
    private GameObject enemy;
    private Player playerScript;

    //通信相手の次の位置
    private Vector3 NextPos;

    void IHandler.objectsSetting(onlineMain m){
        main = m;
        cam = m.getMainCam();
        player = m.getPlayer();
        enemy = m.getEnemy();
        playerScript = player.GetComponent<Player>();
        online = m.getOnline();
        cam.setMainCam(player);
        cam.enemyHide();
        NextPos = enemy.transform.position;
    }
    void IHandler.operate(float d){
        playerScript.move(d);
    }
    void IHandler.stopMove(){
        playerScript.stopMove();
    }
    void IHandler.handlerUpdate(){
        //敵が近づいてきたときの画面周りの赤の量と効果音の音量を設定
        main.setVignetteFromDistance();
        
        serchMine();

        Task runningTask = main.getRunningTask();

        if(runningTask != null){
            //タスクが実行中の場合
            Debug.Log("タスクが実行中:" + runningTask);

            //タスクの処理を実行
            runningTask.runTask();
            
            return;
        }
        // タスクが実行中でない場合
        if(main.ShouldShowTaskKeyUI()){
            //タスク開始を誘導するUIを表示するべきなら表示
            GameObject tarTask = playerScript.performObject();
            GameObject InstedTaskUi = main.InstantiateTask();
            InstedTaskUi.GetComponent<TaskUi>().set(tarTask, cam.gameObject);
        }
        if(main.ShouldDeleteTaskKeyUI()) {//タスク開始を誘導するUIを消すべきなら
            main.destroyTaskUi();
        }
        //プレイヤーの位置を操作
        main.Control();
    }

    //[送信]プレイヤーの位置
    void IHandler.sendPos(){
        online.sendPos(player.transform.position);
    }
    //[受信]プレイヤーの位置
    void IHandler.ReceivedPos(Vector3 pos){
        enemy.transform.position = NextPos;
        NextPos = pos;
    }
    //プレイヤーの位置を更新
    void IHandler.updatePosition()
    {
        Vector3 p = enemy.transform.position;
        p = Vector3.MoveTowards(p, NextPos, 2.5f*Time.deltaTime);
        enemy.transform.position = p;
    }
    
    //[送信]プレイヤーの向き
    void IHandler.sendRot()
    {
        online.sendRot(player.transform.localEulerAngles.y);
    }
    //[受信]プレイヤーの向き
    void IHandler.ReceivedRot(float rot)
    {
        enemy.transform.localEulerAngles = new Vector3(0, rot, 0);
    }

    void IHandler.gameOver()
    {
        if(cam.GameOverAngle(enemy)){
            main.LeaveRoom();
            //ゲームオーバー後の遷移処理
            SceneManager.LoadScene("MainMenu");
        }
    }

    void IHandler.gameClear()
    {       
        // mainが継承しているMonoBehaviourの関数
        // StartCoroutineを使用してコルーチン呼び出し
        main.StartCoroutine(GameClearCoroutine());
    }

    IEnumerator GameClearCoroutine()
    {
        // 同期のため1フレーム待機
        yield return null;
        
        // ルームから退出
        main.LeaveRoom();
        
        // ゲームクリア後の遷移処理
        SceneManager.LoadScene("MainMenu");
    }

    private void serchMine(){//設置されたセンサーを探索
        List<Mine> l = new List<Mine>(playerScript.getMineList());
        // リスト内の各センサーをチェック
        foreach (Mine n in l) {
            // キャラクターがセンサー上にいるか確認
            GameObject onSensor = n.IsCharacterOnSensor();
            if(onSensor != null){
                bool enemyOn = onSensor.name == "Ghost(Clone)";
                // キャラクターがセンサー上にいる場合、センサーの名前を送信
                online.sendOnSensor(enemyOn);
                if(enemyOn){
                    cam.showAll();
                    main.InitializeEnemyPointer(enemy);
                }
                main.lightRedBlinkOn();//ライトの赤点滅を開始
                // センサーを削除
                n.delete();
            }
        }
    }
    void IHandler.hideOther()
    {
        cam.enemyHide();
    }
}