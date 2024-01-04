using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "ScriptableObjects/Puyo", order = 1)]
public class PuyoSprites : ScriptableObject
{
    public Color color;
    public Sprite[] sprite;
}


public class MainGame : MonoBehaviour
{
    public PuyoSprites[] sprites;
    public PuyoSprites[,] PuyoGridFace;
    [Header("")]
    public Transform BlockPrefabParent;
    public Transform PuyoPrefabParent;
    [Header("")]
    public Vector2 SpawnPointPuyo = new Vector2(4, 11);
    [Header("")]
    public GameObject BlockPrefab;
    public GameObject PuyoPrefab;
    public GameObject[,] PuyoGridPosition;
    [Header("")]
    public bool[,] ComboGridPosition;
    public float ValueBetweenBlocks = 0.32f;
    public float timeBetweenFall = 3f;

    private void Start()
    {
        #region drawGrid
        for (float i = 0; i <= 9 * ValueBetweenBlocks; i += ValueBetweenBlocks)
        {
            for (float j = 0; j <= 13 * ValueBetweenBlocks; j += ValueBetweenBlocks)
            {
                if (i == 0 || i >= 8 * ValueBetweenBlocks || j == 0 || j >= 12 * ValueBetweenBlocks)
                {
                    Instantiate(BlockPrefab, new Vector3(i - ValueBetweenBlocks, j - ValueBetweenBlocks, 0), Quaternion.identity, BlockPrefabParent);
                }
            }
        }
        #endregion

        #region setData
        PuyoGridFace = new PuyoSprites[8, 12];
        PuyoGridPosition = new GameObject[8, 12];
        ComboGridPosition = new bool[8, 12];

        for (int i = 0;i <= 7;i++)
        {
            for(int j = 0;j <= 11;j++)
            {
                ComboGridPosition[i, j] = false;
            }
        }
        #endregion

        StartCoroutine(LoopFall(timeBetweenFall));
    }

    void CreatePuyo(int x, int y)
    {
        PuyoGridFace[x, y] = sprites[0];
        PuyoGridPosition[x, y] = Instantiate(PuyoPrefab, new Vector3(x * ValueBetweenBlocks, y * ValueBetweenBlocks, 0), Quaternion.identity, PuyoPrefabParent);
    }

    public int ComboCheck(int x, int y, int width, int height, Color color, int iteration)
    {
        if (x < width && x >= 0 && y >= 0 && y < height)
        {
            if (PuyoGridFace[x, y] != null)
            {
                if (ComboGridPosition[x, y] == false && color == PuyoGridFace[x, y].color)
                {
                    ComboGridPosition[x, y] = true;
                    iteration++;

                    iteration = ComboCheck(x + 1, y, width, height, color, iteration);
                    iteration = ComboCheck(x - 1, y, width, height, color, iteration);
                    iteration = ComboCheck(x, y + 1, width, height, color, iteration);
                    iteration = ComboCheck(x, y - 1, width, height, color, iteration);
                }
            }
        }
        return iteration;
    }

    public IEnumerator LoopFall(float timeBetweenFall)
    {
        bool NoOneMove = true;

        for (float j = 0; j <= 12 * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= 8 * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                int xPos = (int)(i / ValueBetweenBlocks);
                int yPos = (int)(j / ValueBetweenBlocks);

                if ((PuyoGridFace[xPos, yPos] != null)) {

                    if ((yPos - 1 >= 0) && PuyoGridFace[xPos, yPos - 1] == null)
                    {
                        PuyoGridFace[xPos, yPos - 1] = PuyoGridFace[xPos, (int)(j / ValueBetweenBlocks)];

                        PuyoGridFace[xPos, yPos] = null;


                        PuyoGridPosition[xPos, yPos - 1] = PuyoGridPosition[xPos, yPos];

                        PuyoGridPosition[xPos, yPos].transform.position += Vector3.down * ValueBetweenBlocks;

                        PuyoGridPosition[xPos, yPos] = null;

                        //Debug.Log($"x: {xPos} / y: {yPos}");
                        NoOneMove = false;
                    }
                    else
                    {
                        int combo = ComboCheck(xPos, yPos, 8, 12, PuyoGridFace[xPos, yPos].color, 0);

                        Debug.Log(combo);

                        if (combo > 1)
                        {
                            for (float jj = 0; jj <= 11 * ValueBetweenBlocks; jj += ValueBetweenBlocks)
                            {
                                for (float ii = 0; ii <= 8 * ValueBetweenBlocks; ii += ValueBetweenBlocks)
                                {
                                    int xPoss = (int)(ii / ValueBetweenBlocks);
                                    int yPoss = (int)(jj / ValueBetweenBlocks);

                                    if (ComboGridPosition[xPoss, yPoss] == true)
                                    {
                                        PuyoGridFace[xPos, yPos] = null;
                                        Destroy(PuyoGridPosition[xPos, yPos]);
                                        PuyoGridPosition[xPos, yPos] = null;
                                        ComboGridPosition[xPoss, yPoss] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (NoOneMove == true && PuyoGridFace[(int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y] == null)
        {
            CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y);
        }

        yield return new WaitForSeconds(timeBetweenFall);
        StartCoroutine(LoopFall(timeBetweenFall));
    }
}
