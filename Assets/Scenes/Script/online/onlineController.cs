using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// MonoBehaviourPunCallbacksを継承して、PUNのコールバックを受け取れるようにする
public class onlineController : MonoBehaviourPunCallbacks
{
    [SerializeField] private onlineMain main;
    private void Start() {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }
    
    //PUNのコールバック定義
    public override void OnConnectedToMaster() {
        // ランダムなルームに参加する
        PhotonNetwork.JoinRandomRoom();
    }

    // ランダムで参加できるルームが存在しないなら、新規でルームを作成する
    public override void OnJoinRandomFailed(short returnCode, string message) {
        // ルームの参加人数を2人に設定する
        Debug.Log("新規でルームを作成");
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }
    
    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom() {
        Debug.Log("サーバーへの接続に成功");
        // ルームが満員になったら、以降そのルームへの参加を不許可にする
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    // 他プレイヤーがルームへ参加した時に呼ばれるコールバック
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer){
        Debug.Log("プレイヤーが参加");
        //プレイヤーが参加したらマップを生成
        main.setMap();
    }

    // 他プレイヤーがルームから退出した時に呼ばれるコールバック
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        Debug.Log("プレイヤーが退出");
        // 必要な処理をここに追加
    }

    //PUNのコールバック定義終了

    //以下自作関数

    
    public bool isMaster(){
        return PhotonNetwork.IsMasterClient;
    }

    //ルームから退出する
    public void LeaveRoom() {
        PhotonNetwork.Disconnect();
    }

    // RPCでシード値を受信
    public void sendSeed(int seed){
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedSeed", RpcTarget.Others, seed);
    }
    // RPCでシード値を受信
    [PunRPC]
    void ReceivedSeed(int seed)
    {
        main.ReceivedSeed(seed);
    }

    // RPCでプレイヤーの位置を送信
    public void sendPos(Vector3 pos){
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedPos", RpcTarget.Others, pos);
        Debug.Log("sendPos");
    }

    // RPCでプレイヤーの位置を受信
    [PunRPC]
    void ReceivedPos(Vector3 pos)
    {
        Debug.Log("ReceivedPos");
        main.ReceivedPos(pos);
    }

    // RPCでプレイヤーの回転座標を送信
    public void sendRot(float rot){
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedRot", RpcTarget.Others, rot);
    }

    // RPCでプレイヤーの回転座標を受信
    [PunRPC]
    void ReceivedRot(float rot)
    {
        main.ReceivedRot(rot);
    }

    public void sendTaskClear(Vector2 taskPos)
    {
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedTaskClear", RpcTarget.Others, taskPos);
    }

    [PunRPC]
    void ReceivedTaskClear(Vector2 taskPos)
    {
        main.ReceivedTaskClear(taskPos);
    }
    
    public void sendGameOver()
    {
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedGameOver", RpcTarget.Others);
    }

    [PunRPC]
    void ReceivedGameOver()
    {
        main.ReceivedGameOver();
    }

    public void sendOnSensor(bool isEnemy)
    {
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedOnSensor", RpcTarget.Others, isEnemy);
    }

    [PunRPC]
    void ReceivedOnSensor(bool isEnemy)
    {
        main.ReceivedOnSensor(isEnemy);
    }

    public void sendEnemyHide()
    {
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedEnemyHide", RpcTarget.Others);
    }
    public void ReceivedEnemyHide(){
        main.ReceivedEnemyHide();
    }
    public void sendPlayerHide()
    {
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedPlayerHide", RpcTarget.Others);
    }
    public void ReceivedPlayerHide(){
        main.ReceivedPlayerHide();
    }
    public void sendShowAll()
    {
        //PhotonViewコンポーネントを取得
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("ReceivedShowAll", RpcTarget.Others);
    }
    public void ReceivedShowAll(){
        main.ReceivedShowAll();
    }
}