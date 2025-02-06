using UnityEngine;

public class onlineEnemy : MonoBehaviour
{
    private GameObject cam;
    private MapMaster map;
    private Vector3 nextPos;
    private bool isMoving = false;
    onlineMain main;
    private bool visiableEnemy = false;
    private float vTimer;
    [SerializeField] private float speed = 2.5f;

    public void setting(onlineMain _main, MapMaster _map, Player p, GameObject _cam)
    {
        main = _main;
        cam = _cam;
        map = _map;
        isMoving = false;
    }

    public void stopMove()
    {
        if(!checkVisiablePlayer() && isMoving){
            moving();
        }
    }

    public void move(float input)
    {
        // 移動中は操作を受け付けない
        if(isMoving){
            return;
        }
        setNextPos(input);
    }

    private bool checkVisiablePlayer(){
        if(visiablePlayer()){//もしプレイヤーを見つけたら
            main.SendGameOver();
            Debug.Log("GameOver");
            return true;
        }
        return false;
    }

    private bool visiablePlayer(){//プレイヤーが目の前に居るか判断
        float distance = 6f;
        if(visiableEnemy){
            visiableCountDown();
            distance = 3f;//敵が見える間は敵の視野範囲が短い
        }
        Vector3 realThisPos = this.transform.position;
        Vector3 rayDirection = transform.forward;
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
            main.getOnline().sendEnemyHide();
        }
    }

    private void setNextPos(float input){        
        //カメラの向きを取得
        float camRotation = cam.transform.localEulerAngles.y;
        // カメラの向きと入力内容から移動する角度を決定
        float moveAngle = -(input-camRotation);
        //90度刻みで補正
        moveAngle = Mathf.Round(moveAngle / 90.0f) * 90.0f;

        // Debug.Log("moveAngle: "+ moveAngle);

        // 移動方向に合わせてキャラを回転
        transform.rotation = Quaternion.Euler(0, moveAngle, 0);
        //移動先を決定
        float radAngle = moveAngle * Mathf.Deg2Rad;
        Vector2 newNextPos = new Vector2(Mathf.Sin(radAngle), -Mathf.Cos(radAngle));
        newNextPos += map.convertMapPosition(this.transform.position);

        if(map.isRoad((int)newNextPos.x, (int)newNextPos.y)){
            // 移動可能なら移動開始 
            nextPos = map.convertRealPosition(newNextPos);
            isMoving = true;
        }
        // else{
        //     Debug.LogError(map.convertRealPosition(newNextPos)+"に移動できません。"+newNextPos);
        // }
    }

    private void moving(){
        //このフレームでの移動量(移動がかくかくするのを防ぐため)
        float delaPos = speed*Time.deltaTime;
        // 現在地を取得
        Vector3 pos = this.transform.position;
        // 移動する
        pos = Vector3.MoveTowards(pos, nextPos, delaPos);
        // 現在地を変更
        this.transform.position = pos;
        if(pos == nextPos)
            isMoving = false;
    }

    public void setStartPos()
    {
        while(true){
            int x = Random.Range(0,map.mapWidth);
            int y = Random.Range(0,map.mapHeight);
            Vector2 pp = map.convertMapPosition(main.getPlayer().transform.position);//Player Position
            if(map.isRoad(x,y) && (Mathf.Abs(x-pp.x) > 3) && (Mathf.Abs(y-pp.y) > 3)){
                transform.position = map.convertRealPosition(new Vector2(x, y));
                break;
            }
        }
    }
}