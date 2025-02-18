using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// A-star algorithm
public class AStar {
    private List<AStarNode> openList = new List<AStarNode>(); //これから探索するノードのリスト
    private List<AStarNode> exploredList = new List<AStarNode>(); //探索済みリスト
    AStarNode goalNode;

    //探索を実行
    public List<Vector2> Exploration(MapMaster map, Vector2 start, Vector2 goal) {
        openList.Add(new AStarNode(start, goal, 0, null));// 初めのノード(現在地)を追加
        AStarNode best = null;
        do {
            best = openList[0];
            openList.RemoveAt(0);
            exploredList.Add(best);
            openList.AddRange(best.Open(map, exploredList));
            openList = SortList(openList);
        } while (openList.Count > 0 && !isGoal(best));
        return GetPath();
    }

    private bool isGoal(AStarNode best) {
        if (best == null)
            return false;
        if (best.IsGoal()) {
            goalNode = best;
            return true;
        }
        return false;
    }

    private List<Vector2> GetPath() {
        List<Vector2> path = new List<Vector2>(); //経路
        AStarNode n = goalNode;
        while (n != null && n.GetParent() != null){
            path.Insert(0, n.GetPosition());
            n = n.GetParent();
        }
        return path;
    }

    //ノードのリストを整列
    private List<AStarNode> SortList(List<AStarNode> l) {
        //要素数が１以下なら並び替える必要がないためそのまま返す。
        if (l.Count <= 1)
            return l;
        //中心点（pivot）,pivotより評価値が小さい要素を入れるleft,大きい要素を入れるrightをつくる
        AStarNode pivot = l[0];
        l.RemoveAt(0);
        List<AStarNode> left = new List<AStarNode>();
        List<AStarNode> right = new List<AStarNode>();
        //引数リストlが空になるまで繰り返す
        while (l.Count > 0) {
            AStarNode n = l[0];
            l.RemoveAt(0);
            if (n.GetEvaluation() <= pivot.GetEvaluation())
                left.Add(n);
            else
                right.Add(n);
        }
        List<AStarNode> sorted = SortList(left);
        sorted.Add(pivot);
        sorted.AddRange(SortList(right));
        return sorted;
    }
}

//Aスターノードクラス
public class AStarNode {
    //Nodeが担当する位置
    private Vector2 position;
    //ゴールの位置
    private Vector2 goal;
    //スタート地点からの歩数
    private int step;
    //Nodeを呼び出した親Node
    private AStarNode parent;

    public AStarNode(Vector2 pos, Vector2 g, int _step, AStarNode _parent) {
        position = pos;
        goal = g;
        step = _step;
        parent = _parent;
    }

    public List<AStarNode> Open(MapMaster map, List<AStarNode> exploredList) {
        List<AStarNode> opened = new List<AStarNode>();
        int[][] d = {
            new int[] { 1, 0},
            new int[] {-1, 0},
            new int[] { 0, 1},
            new int[] { 0, -1}
        };

        for (int i = 0; i < 4; i++) {
            //四方を探索
            Vector2 newPos = new Vector2(position.x + d[i][0], position.y + d[i][1]);
            //探索可能ならOpenリストに追加
            if (CanGo(map, newPos)&& !AlreadyExplored(newPos, exploredList))
                opened.Add(new AStarNode(newPos, goal, step + 1, this));
        }
        return opened;
    }

    /**リストl内にpが入ってるか**/
    private bool AlreadyExplored(Vector2 p, List<AStarNode> l) {
        foreach (AStarNode n in l) {
            if (p == n.GetPosition()) {
                return true;
            }
        }
        return false;
    }

    private bool CanGo(MapMaster map, Vector2 p) {
        int x = (int)p.x;
        int y = (int)p.y;
        return map.isRoad(x, y);
    }

    /**評価値を取得**/
    public int GetEvaluation() {
        return GetDistance() + GetStep();
    }

    /**歩数を取得**/
    private int GetStep() {
        return step;
    }

    /**距離を取得**/
    private int GetDistance() {
        int dx = (int)Mathf.Abs(goal.x - this.position.x);
        int dy = (int)Mathf.Abs(goal.y - this.position.y);
        return dx + dy;
    }

    public bool IsGoal() {
        return GetDistance() <= 1;
    }

    public AStarNode GetParent() {
        return parent;
    }

    public Vector2 GetPosition() {
        return position;
    }
}
