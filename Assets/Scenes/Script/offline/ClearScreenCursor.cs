using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //シーン遷移をする為追加

public class ClearScreenCursor : MonoBehaviour
{
  private int cursorPosition;
  private Vector2 pos;
  private const int maxPos = 1;
    // Start is called before the first frame update
    void Start()
    {
      cursorPosition = 0;
      pos.x = -150;
    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.S)){
        cursorPosition++;
      }
      else if(Input.GetKeyDown(KeyCode.W)){
        cursorPosition--;
      }

      if(cursorPosition < 0){
        cursorPosition = maxPos;
      }
      else if(cursorPosition > maxPos){
        cursorPosition = 0;
      }

      if(Input.GetKeyDown(KeyCode.Return)){
        if(cursorPosition == 0){
          SceneManager.LoadScene("OffLineBattle");
        }
        else if(cursorPosition == 1){
          SceneManager.LoadScene("MainMenu");
        }
      }
      pos.y = cursorPosition*(-70);
      GetComponent<RectTransform>().anchoredPosition = pos;
    }
}
