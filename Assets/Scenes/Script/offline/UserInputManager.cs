using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{
  private KeyCode MoveUpKey = KeyCode.W;
  public bool PressMoveUp(){
    return Input.GetKey(MoveUpKey);
  }

  private KeyCode MoveDownKey = KeyCode.S;
  public bool PressMoveDown(){
    return Input.GetKey(MoveDownKey);
  }

  private KeyCode MoveLeftKey = KeyCode.A;
  public bool PressMoveLeft(){
    return Input.GetKey(MoveLeftKey);
  }

  private KeyCode MoveRightKey = KeyCode.D;
  public bool PressMoveRight(){
    return Input.GetKey(MoveRightKey);
  }

  private KeyCode MoveConfirmKey = KeyCode.E;
  public bool PressDownConfirm(){//決定ボタン(タスク実行ボタン)を押しているか
    return Input.GetKeyDown(MoveConfirmKey);
  }

  private KeyCode MoveSetMineKey = KeyCode.Q;
  public bool PressDownMine(){//地雷設置ボタンを押しているか
    return Input.GetKeyDown(MoveSetMineKey);
  }
}
