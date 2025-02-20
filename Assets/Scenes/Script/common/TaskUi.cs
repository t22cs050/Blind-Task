using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskUi : MonoBehaviour
{
  private GameObject cam;
  public void set(GameObject task, GameObject _cam){
    this.transform.position = task.transform.position + new Vector3(0f, 2f, 0f);
    cam = _cam;
  }
}
