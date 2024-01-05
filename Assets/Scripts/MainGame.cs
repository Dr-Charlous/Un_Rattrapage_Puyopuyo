using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "ScriptableObjects/Puyo", order = 1)]
public class PuyoSprites : ScriptableObject
{
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
    public GameObject ActualPuyo;
    public GameObject[,] PuyoGridPosition;
    [Header("")]
    public bool IsMoved = false;
    public bool[,] ComboGridPosition;
    public int Height = 12;
    public int Width = 8;
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
        PuyoGridFace = new PuyoSprites[Width, Height];
        PuyoGridPosition = new GameObject[Width, Height];
        ComboGridPosition = new bool[Width, Height];

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                ComboGridPosition[i, j] = false;
            }
        }
        #endregion

        CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y);
        StartCoroutine(LoopFall(timeBetweenFall));
    }

    private void Update()
    {
        if (IsMoved == false)
        {
            int xPos = (int)(Mathf.Abs(ActualPuyo.transform.position.x / ValueBetweenBlocks));
            int yPos = (int)(Mathf.Abs(ActualPuyo.transform.position.y / ValueBetweenBlocks));

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (xPos + 1 < Width && PuyoGridFace[xPos + 1, yPos] == null)
                {
                    Debug.Log("start right");
                    PuyoGridFace[xPos + 1, yPos] = PuyoGridFace[xPos, yPos];
                    PuyoGridFace[xPos, yPos] = null;

                    PuyoGridPosition[xPos + 1, yPos] = PuyoGridPosition[xPos, yPos];
                    ActualPuyo.transform.position = new Vector3((xPos + 1) * ValueBetweenBlocks, yPos * ValueBetweenBlocks, 0);
                    PuyoGridPosition[xPos, yPos] = null;
                }
                Debug.Log("end right");
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (xPos - 1 >= 0 && PuyoGridFace[xPos - 1, yPos] == null)
                {
                    Debug.Log("start left");
                    PuyoGridFace[xPos - 1, yPos] = PuyoGridFace[xPos, yPos];
                    PuyoGridFace[xPos, yPos] = null;

                    PuyoGridPosition[xPos - 1, yPos] = PuyoGridPosition[xPos, yPos];
                    ActualPuyo.transform.position = new Vector3((xPos - 1) * ValueBetweenBlocks, yPos * ValueBetweenBlocks, 0);
                    PuyoGridPosition[xPos, yPos] = null;
                }
                Debug.Log("end left");
            }
        }
    }

    void CreatePuyo(int x, int y)
    {
        PuyoGridFace[x, y] = sprites[0];
        ActualPuyo = Instantiate(PuyoPrefab, new Vector3(x * ValueBetweenBlocks, y * ValueBetweenBlocks, 0), Quaternion.identity, PuyoPrefabParent);
        PuyoGridPosition[x, y] = ActualPuyo;
    }

    public int ComboCheck(int x, int y, int width, int height, PuyoSprites sprites, int iteration)
    {
        if (x < width && x >= 0 && y >= 0 && y < height)
        {
            if (PuyoGridFace[x, y] != null)
            {
                if (ComboGridPosition[x, y] == false && sprites == PuyoGridFace[x, y])
                {
                    ComboGridPosition[x, y] = true;
                    iteration++;

                    iteration = ComboCheck(x + 1, y, width, height, sprites, iteration);
                    iteration = ComboCheck(x - 1, y, width, height, sprites, iteration);
                    iteration = ComboCheck(x, y + 1, width, height, sprites, iteration);
                    iteration = ComboCheck(x, y - 1, width, height, sprites, iteration);
                }
            }
        }
        return iteration;
    }

    public void Combo()
    {
        for (float j = 0; j <= Height * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= Width * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                int xPos = (int)(Mathf.Abs(i / ValueBetweenBlocks));
                int yPos = (int)(Mathf.Abs(j / ValueBetweenBlocks));

                if ((PuyoGridFace[xPos, yPos] != null))
                {
                    int combo = ComboCheck(xPos, yPos, Width, Height, PuyoGridFace[xPos, yPos], 0);

                    if (combo > 2)
                    {
                        for (float jj = 0; jj < Height * ValueBetweenBlocks; jj += ValueBetweenBlocks)
                        {
                            for (float ii = 0; ii <= Width * ValueBetweenBlocks; ii += ValueBetweenBlocks)
                            {
                                int xPoss = (int)(Mathf.Abs(ii / ValueBetweenBlocks));
                                int yPoss = (int)(Mathf.Abs(jj / ValueBetweenBlocks));

                                if (ComboGridPosition[xPoss, yPoss] == true)
                                {
                                    PuyoGridFace[xPoss, yPoss] = null;
                                    Destroy(PuyoGridPosition[xPoss, yPoss]);
                                    PuyoGridPosition[xPoss, yPoss] = null;
                                    ComboGridPosition[xPoss, yPoss] = false;
                                }
                            }
                        }

                    }
                }
            }
        }

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                ComboGridPosition[i, j] = false;
            }
        }
    }

    public void Fall()
    {
        bool NoOneMove = true;

        for (float j = 0; j <= Height * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= Width * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                int xPos = (int)(Mathf.Abs(i / ValueBetweenBlocks));
                int yPos = (int)(Mathf.Abs(j / ValueBetweenBlocks));

                if ((PuyoGridFace[xPos, yPos] != null) && (yPos - 1 >= 0) && PuyoGridFace[xPos, yPos - 1] == null)
                {
                    PuyoGridFace[xPos, yPos - 1] = PuyoGridFace[xPos, yPos];

                    PuyoGridFace[xPos, yPos] = null;


                    PuyoGridPosition[xPos, yPos - 1] = PuyoGridPosition[xPos, yPos];

                    PuyoGridPosition[xPos, yPos].transform.position += Vector3.down * ValueBetweenBlocks;

                    PuyoGridPosition[xPos, yPos] = null;

                    //Debug.Log($"x: {xPos} / y: {yPos}");
                    NoOneMove = false;
                }

            }
        }

        if (NoOneMove == true && PuyoGridFace[(int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y] == null)
        {
            CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y);
        }
    }

    public IEnumerator LoopFall(float timeBetweenFall)
    {
        IsMoved = true;
        Combo();
        Fall();
        IsMoved = false;

        yield return new WaitForSeconds(timeBetweenFall);
        StartCoroutine(LoopFall(timeBetweenFall));
    }
}
