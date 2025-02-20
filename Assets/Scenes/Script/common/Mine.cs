using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
  private Player plaSc;
  private MapMaster mapSc;
  private int x;
  private int y;
  public void set(int _x,int _y, Player p, MapMaster m){
    x = _x;
    y = _y;
    mapSc = m;
    plaSc = p;
  }
  public GameObject IsCharacterOnSensor(){
    Ray ray = new Ray(this.transform.position, new Vector3(0,1,0));
    RaycastHit hit;
    if(Physics.Raycast(ray, out hit) && (hit.collider.gameObject.tag == "Player" || hit.collider.gameObject.tag == "Enemy")){
      return hit.collider.gameObject;
    }
    return null;
  }
  public void delete(){
    plaSc.RemoveMine(this);
    mapSc.deleteMine(x, y);
    GameObject.Destroy(this.gameObject);
  }
}
