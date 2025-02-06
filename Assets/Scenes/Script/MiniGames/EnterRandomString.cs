using UnityEngine;
using UnityEngine.UI; // UIを制御する為追加

public class EnterRandomString : MonoBehaviour, IMiniGame {
  private Text progress; // 進捗を表示するテキスト
  private char[] targetString; // 目標のランダム文字列
  private string progressString; // 現在の入力進捗

  // 初期化メソッド
  public void Initialize(int n) {
    // タスク番号を設定
    Text taskNum = this.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
    taskNum.text = "Task: " + n;
    
    // ランダムな文字列を生成
    targetString = new char[Random.Range(3, 5)];
    for (int i = 0; i < targetString.Length; i++)
        targetString[i] = GenerateRandomAlphabet();
    
    // 指示文を設定
    Text imper = this.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>();
    imper.text = "'" + ToUpper(targetString) + "'と入力しろ！";

    // 進捗を初期化
    progressString = "";
    progress = this.transform.GetChild(0).transform.GetChild(2).GetComponent<Text>();
  }  

  // ゲームクリア判定
  public bool IsCleared() {
    return progressString.Equals(new string(targetString));
  }

  // ゲームのプレイロジック
  public void Play() {
    // 正しいキーが押されたかチェック
    if (Input.GetKeyDown( (KeyCode)targetString[progressString.Length] )) {
        Debug.Log("GetKeyDown");
        progressString += targetString[progressString.Length];
    }
    // 進捗を更新
    progress.text = ">" + progressString.ToUpper() + "<";
  }

  // ランダムなアルファベットを生成
  private char GenerateRandomAlphabet() {
    return (char)('a' + Random.Range(0, 26));
  }

  // 文字列を大文字に変換
  private string ToUpper(char[] c) {
    string s = "";
    for (int i = 0; i < c.Length; i++)
        s += char.ToUpper(c[i]);
    return s;
  }
}