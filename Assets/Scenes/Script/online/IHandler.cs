//プレイヤー側と敵側で分岐する処理を定義するインターフェース
using UnityEngine;

interface IHandler{
    public void objectsSetting(onlineMain m);    
    public void operate(float d);
    void sendPos();
    void ReceivedPos(Vector3 pos);
    void sendRot();
    void ReceivedRot(float rot);
    public void stopMove();
    void updatePosition();
    void handlerUpdate();
    void gameOver();
    void gameClear();
    void hideOther();
}