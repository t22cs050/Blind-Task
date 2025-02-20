using System;
using UnityEngine;

public class EnemyPointer : MonoBehaviour
{
    [SerializeField]private GameObject Enemy;  
    [SerializeField]private Camera maincamera;
    [SerializeField]private RectTransform rootRectTransform;
    private RectTransform thisRectTransform;

    private float xMin, xMax, yMin, yMax;

    public void set(GameObject e, Camera c){
        Enemy = e;
        maincamera = c;
        rootRectTransform = this.transform.parent.GetChild(0).GetComponent<RectTransform>();
        thisRectTransform = this.GetComponent<RectTransform>();
        Rect thisRect = thisRectTransform.rect;

        xMin = rootRectTransform.rect.xMin+(thisRect.width/2);
        xMax = rootRectTransform.rect.xMax-(thisRect.width/2);
        yMin = rootRectTransform.rect.yMin+(thisRect.height/2);
        yMax = rootRectTransform.rect.yMax-(thisRect.height/2);
        
        Debug.Log(rootRectTransform.gameObject.name);
        Update();
    }  
    internal void set(Vector3 target, Camera camera)
    {
        GameObject newObject = Instantiate(new GameObject("Pointer"));
        newObject.transform.position = target;
        set(newObject, camera);
    }

    void Update()
    {
        // 敵の画面上の位置を取得
        Vector3 enemyPos = maincamera.WorldToScreenPoint(Enemy.transform.position);

        // 敵の位置をRectTransform座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRectTransform,
            enemyPos,
            null,
            out Vector2 localPoint);

        // 位置を敵の少し上になるように調整
        localPoint.y += 50f;

        // ポインターが画面外いるかどうかをチェック
        if(isScreenOut(localPoint)){
            // 画面外の場合、位置を画面内に収める
            Vector2 newLocalPoint = insideScreen(localPoint);
            // ポインターを敵の方向に回転させる
            thisRectTransform.rotation = Quaternion.Euler(0, 0, getAngleTowardTarget(newLocalPoint, localPoint));
            // 画面内に収めた位置に更新
            localPoint = newLocalPoint;
        }
        else{
            // ローカルポイントが画面内の場合、回転をリセット
            thisRectTransform.rotation = Quaternion.Euler(0, 0, 0);
        }    

        // ポインターの位置を更新
        thisRectTransform.anchoredPosition = localPoint;
    }

    //引数pointが画面外かどうか判定
    private bool isScreenOut(Vector2 point){
        return(
            point.x < xMin ||
            point.x > xMax ||
            point.y < yMin ||
            point.y > yMax
            );
    }

    //引数pointを画面内で最も近い座標に変換
    private Vector2 insideScreen(Vector2 point){
        //point.xをxMin～xMax内にする
        point.x = Mathf.Clamp(point.x, xMin, xMax);
        //point.yをyMin～yMax内にする
        point.y = Mathf.Clamp(point.y, yMin, yMax);
        return point;
    }

    //currentからtargetを向いた時の角度を取得
    private float getAngleTowardTarget(Vector2 current, Vector2 target){
        Vector2 direction = target - current;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += 90;
        return angle;
    }

}
