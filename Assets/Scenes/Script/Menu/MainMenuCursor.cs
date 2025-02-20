using UnityEngine;
using UnityEngine.SceneManagement; //シーン遷移をする為追加
using UnityEngine.UI;//UIを制御する為追加

public class MainMenuCursor : MonoBehaviour
{
  private int cursorPosition;
  private Vector2 pos;
  private const int maxPos = 2;
  [SerializeField] private AudioSource selectSE;
  [SerializeField] private AudioSource decisionSE;
  [SerializeField] private GameObject fadePanel;
  private Image fadePanelImage;
  private bool isDecided = false;

    void Start()
    {
      cursorPosition = 0;
      pos.x = -150;
      fadePanelImage = fadePanel.GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
      if(isDecided){
        if(isFaded()){
          if(cursorPosition == 0){
            SceneManager.LoadScene("OffLineBattle");
          }
          else if(cursorPosition == 1){
            SceneManager.LoadScene("OnlineBattle");
          }
          else if(cursorPosition == 2){
            Application.Quit();
          }
        }
        return;
      }
      if(Input.GetKeyDown(KeyCode.S)){
        selectSE.Play();
        cursorPosition++;
      }
      else if(Input.GetKeyDown(KeyCode.W)){
        selectSE.Play();
        cursorPosition--;
      }

      if(cursorPosition < 0){
        cursorPosition = maxPos;
      }
      else if(cursorPosition > maxPos){
        cursorPosition = 0;
      }

      if(Input.GetKeyDown(KeyCode.Return)){
        decisionSE.Play();
        isDecided = true;        
      }
      pos.y = cursorPosition*(-70)+70;
      GetComponent<RectTransform>().anchoredPosition = pos;
    }

    // フェードアウトが終わったかどうかを返す
    private bool isFaded()
    {
      float alpha = fadePanelImage.color.a;
      alpha += Time.deltaTime/2.0f;
      fadePanelImage.color = new Color(0, 0, 0, alpha);
      if(alpha >= 1){
        return true;
      }
      return false;
    }
}
