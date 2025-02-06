using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IMain{
  void setRunningTask(Task t);

   void clearTaskOn(Vector2 taskPos);

   int getClearedTaskQuantity();
  
   void GameOver();
   bool getGameOver();

   CameraControl getMainCam();
   Canvas getCanvas();
}