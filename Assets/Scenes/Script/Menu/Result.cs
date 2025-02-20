using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;//UIを制御する為追加

public class Result : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Enemy;
    [SerializeField] private GameObject ResultText;
    [SerializeField] private GameObject TimeText;
    [SerializeField] private GameObject fadePanel;
    [SerializeField] private GameObject cursor;
    private Image fadePanelImage;
    private int selectNum;

    private static int time;
    private static bool isPlayerWon;
    private static bool isOnline;

    // Start is called before the first frame update
    void Start()
    {
        fadePanelImage = fadePanel.GetComponentInChildren<Image>();
        fadePanelImage.color = new Color(0, 0, 0, 1);
        selectNum = 0;

        TextMeshProUGUI resultTextComp = ResultText.GetComponent<TextMeshProUGUI>();
        if(isPlayerWon){
            GameObject Instanted = Instantiate(Player);
            Instanted.transform.position = new Vector3(0, 0, 0); // 位置を指定
            Instanted.transform.rotation = Quaternion.Euler(0, -180, 0); // 向きを指定
            Instanted.transform.localScale = new Vector3(2, 2, 2); // 大きさを指定

            resultTextComp.text = "プレイヤーの勝ち";
            resultTextComp.color = new Color(0.0f, 0.5f, 1.0f, 1.0f);
        }
        else{
            GameObject Instanted = Instantiate(Enemy);
            Instanted.transform.position = new Vector3(0, 0, 0); // 位置を指定
            Instanted.transform.rotation = Quaternion.Euler(0, -160, 0); // 向きを指定
            Instanted.transform.localScale = new Vector3(3, 3, 3); // 大きさを指定

            resultTextComp.text = "ゴーストの勝ち";
            resultTextComp.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }

        TextMeshProUGUI timeTextComp = TimeText.GetComponent<TextMeshProUGUI>();
        timeTextComp.text = (time / 60).ToString("D2") + " : " + (time % 60).ToString("D2");
    }

    // Update is called once per frame
    void Update()
    {
        if(isFaded()){
            select();
        }
    }

    private void select()
    {
        if(Input.GetKeyDown(KeyCode.A)){
            selectNum--;
            if(selectNum < 0)
                selectNum = 1;
        }
        else if(Input.GetKeyDown(KeyCode.D)){
            selectNum++;
            if(selectNum > 1)
                selectNum = 0;
        }

        int cursorPos = (selectNum*415)-360;
        cursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(cursorPos,-171);

        if(Input.GetKey(KeyCode.Return)){
            if(selectNum == 0)
                SceneManager.LoadScene("MainMenu");
            else{
                if(isOnline){
                    SceneManager.LoadScene("OnLineBattle");
                }
                else{
                    SceneManager.LoadScene("OffLineBattle");
                }
            }
        }
    }

    private bool isFaded()
    {
      float alpha = fadePanelImage.color.a;
      alpha -= Time.deltaTime/2.0f;
      fadePanelImage.color = new Color(0, 0, 0, alpha);
      if(alpha <= 0){
        return true;
      }
      return false;
    }

    public void setTime(float t){
        time = Mathf.FloorToInt(t);//小数点以下切り捨て
    }
    public void setIsPlayerWon(bool b){
        isPlayerWon = b;
    }

    public void setIsOnline(bool v){
        isOnline = v;
    }
}
