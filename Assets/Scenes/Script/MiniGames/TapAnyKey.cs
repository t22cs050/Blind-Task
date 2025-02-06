using UnityEngine;
using UnityEngine.UI; //UIを制御する為追加

public class TapAnyKey : MonoBehaviour,IMiniGame{
  private Text progress;
  private char alphabet;
  private int times;
  private int progressTimes;

  public void Initialize(int n){
    Text taskNum = this.transform.GetChild(0).transform.GetChild(0).GetComponent<Text> ();
    taskNum.text = "Task:" + n;

    alphabet = (char)('a' + Random.Range (0,26));
    times = Random.Range (4,10);
    Text imper = this.transform.GetChild(0).transform.GetChild(1).GetComponent<Text> ();
    imper.text = "'"+ char.ToUpper(alphabet) +"'を" + times + "回押せ!";

    progressTimes = 0;
    progress = this.transform.GetChild(0).transform.GetChild(2).GetComponent<Text> ();
    progress.text = "残り:" + times;
  }

  public bool IsCleared(){
    if(progressTimes == times)
        return true;
    return false;
  }
  public void Play(){
    if(Input.GetKeyDown((KeyCode)alphabet)){
        Debug.Log("GetKeyDown");
        progressTimes++;
    }
    progress.text = "残り:" + (times-progressTimes);
  }
}