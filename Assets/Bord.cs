using UnityEngine;
using UnityEngine.Rendering;

public class Bord : MonoBehaviour
{
    
    private Cell[,] cells;
    public Bord(int w, int h)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                cells[x, y] = new Cell(x, y);
            }
        }
    }
    public Cell GetCell(int x, int y)
    {
        return cells[x, y];
    }
    //開発メモ：AIと相談の上コードベースで開発を進めることとした。Unityメインではなくコードベースで。
    //Unityはプレゼンテーション層、コードベースはドメイン層といったイメージ。Unityはあくまで見た目を作るためのツールであって、ゲームのロジックはコードベースで完結させる。
}