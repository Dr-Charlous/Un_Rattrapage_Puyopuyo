using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using DG.Tweening;
using static UnityEditor.ShaderData;
using TMPro;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "Object", menuName = "ScriptableObjects/Puyo", order = 1)]
public class PuyoSprites : ScriptableObject
{
    public Sprite[] sprite;

    public void ChangeSprite(int xPos, int yPos, int width, int height, PuyoSprites sprites, PuyoSprites[,] PuyoGridFace, GameObject[,] PuyoGridPosition)
    {
        int visu = 0;

        if (xPos - 1 >= 0 && PuyoGridFace[xPos - 1, yPos] != null)
        {
            if (sprites == PuyoGridFace[xPos - 1, yPos])
            {
                visu += 8;
            }
        }

        if (xPos + 1 < width && PuyoGridFace[xPos + 1, yPos] != null)
        {
            if (sprites == PuyoGridFace[xPos + 1, yPos])
            {
                visu += 4;
            }
        }

        if (yPos - 1 >= 0 && PuyoGridFace[xPos, yPos - 1] != null)
        {
            if (sprites == PuyoGridFace[xPos, yPos - 1])
            {
                visu += 1;
            }
        }

        if (yPos + 1 < height && PuyoGridFace[xPos, yPos + 1] != null)
        {
            if (sprites == PuyoGridFace[xPos, yPos + 1])
            {
                visu += 2;
            }
        }

        PuyoGridPosition[xPos, yPos].gameObject.GetComponent<SpriteRenderer>().sprite = sprites.sprite[visu];
        PuyoGridPosition[xPos, yPos].gameObject.transform.position = new Vector3(xPos * 0.32f, yPos * 0.32f, 0);
    }
}


public class MainGame : MonoBehaviour
{
    #region Variables
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
    public GameObject ComboFeedBack;
    public GameObject[,] PuyoGridPosition;
    [Header("")]
    public bool IsMoved = false;
    public bool[,] ComboGridPosition;
    public int Height = 12;
    public int Width = 8;
    [Range(1, 7)]
    public float ValueBetweenBlocks = 0.32f;
    public int Range = 7;
    public int ComboRange = 4;
    public int ComboCount = 0;
    [Range(1, 10)]
    public int timeBetweenFall = 5;
    #endregion Variables

    void Start()
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
        #endregion drawGrid

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

        ComboFeedBack.SetActive(false);
        #endregion setData

        CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y, Range);
        StartCoroutine(LoopFall(timeBetweenFall / 10f));
    }

    void Update()
    {
        if (IsMoved == false && ActualPuyo != null)
        {
            int xPos = (int)(Mathf.Abs(ActualPuyo.transform.position.x / ValueBetweenBlocks));
            int yPos = (int)(Mathf.Abs(ActualPuyo.transform.position.y / ValueBetweenBlocks));

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(xPos, yPos, 1);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(xPos, yPos, -1);
            }
        }
    }

    #region Puyo
    void CreatePuyo(int x, int y, int range)
    {
        int valueSprite = 0;

        if (range <= sprites.Length)
        {
            valueSprite = Random.Range(0, range);
        }
        else
        {
            valueSprite = Random.Range(0, sprites.Length);
        }

        PuyoGridFace[x, y] = sprites[valueSprite];
        ActualPuyo = Instantiate(PuyoPrefab, new Vector3(x * ValueBetweenBlocks, y * ValueBetweenBlocks, 0), Quaternion.identity, PuyoPrefabParent);
        PuyoGridPosition[x, y] = ActualPuyo;

        ActualPuyo.GetComponent<SpriteRenderer>().sprite = sprites[valueSprite].sprite[0];
    }

    void Move(int xPos, int yPos, int value)
    {
        if (xPos + value < Width && xPos + value >= 0 && PuyoGridFace[xPos + value, yPos] == null)
        {
            PuyoGridFace[xPos + value, yPos] = PuyoGridFace[xPos, yPos];
            PuyoGridFace[xPos, yPos] = null;

            PuyoGridPosition[xPos + value, yPos] = PuyoGridPosition[xPos, yPos];
            //ActualPuyo.transform.position = new Vector3((xPos + value) * ValueBetweenBlocks, yPos * ValueBetweenBlocks, 0);
            ActualPuyo.transform.DOLocalMoveX((xPos + value) * ValueBetweenBlocks, timeBetweenFall / 10f);
            PuyoGridPosition[xPos, yPos] = null;
        }
    }
    #endregion Puyo

    #region Combo
    int ComboCheck(int x, int y, int width, int height, PuyoSprites sprites, int iteration)
    {
        if (x < width && x >= 0 && y >= 0 && y < height)
        {
            if (PuyoGridFace[x, y] != null)
            {
                if (ComboGridPosition[x, y] == false && sprites == PuyoGridFace[x, y])
                {
                    PuyoGridFace[x, y].ChangeSprite(x, y, Width, Height, PuyoGridFace[x, y], PuyoGridFace, PuyoGridPosition);

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

    bool Combo()
    {
        bool Combo = false;

        for (float j = 0; j <= Height * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= Width * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                int xPos = (int)(Mathf.Abs(i / ValueBetweenBlocks));
                int yPos = (int)(Mathf.Abs(j / ValueBetweenBlocks));

                if ((PuyoGridFace[xPos, yPos] != null))
                {
                    int combo = ComboCheck(xPos, yPos, Width, Height, PuyoGridFace[xPos, yPos], 1);

                    if (combo > ComboRange)
                    {
                        Combo = true;

                        StartCoroutine(ComboEffectFeedBack(Mathf.Abs(combo - 1 - ComboCount)));

                        for (float jj = 0; jj < Height * ValueBetweenBlocks; jj += ValueBetweenBlocks)
                        {
                            for (float ii = 0; ii <= Width * ValueBetweenBlocks; ii += ValueBetweenBlocks)
                            {
                                int xPoss = (int)(Mathf.Abs(ii / ValueBetweenBlocks));
                                int yPoss = (int)(Mathf.Abs(jj / ValueBetweenBlocks));

                                if (ComboGridPosition[xPoss, yPoss] == true)
                                {
                                    StartCoroutine(ComboEffectKill(xPoss, yPoss, PuyoGridPosition[xPoss, yPoss].transform));
                                }
                            }
                        }
                    }

                    for (int iii = 0; iii < Width; iii++)
                    {
                        for (int jjj = 0; jjj < Height; jjj++)
                        {
                            ComboGridPosition[iii, jjj] = false;
                        }
                    }
                }
            }
        }

        if (!Combo)
        {
            ComboCount = 0;
        }

        return Combo;
    }

    IEnumerator ComboEffectKill(int xPoss, int yPoss, Transform puyo)
    {
        puyo.DOPunchScale(Vector3.one, timeBetweenFall / 10f);

        yield return new WaitForSeconds(timeBetweenFall / 10f);
        ComboCount++;

        PuyoGridFace[xPoss, yPoss] = null;
        Destroy(PuyoGridPosition[xPoss, yPoss]);
        PuyoGridPosition[xPoss, yPoss] = null;
        ComboGridPosition[xPoss, yPoss] = false;
    }

    IEnumerator ComboEffectFeedBack(int combo)
    {
        ComboFeedBack.SetActive(true);
        ComboFeedBack.GetComponentInChildren<TextMeshProUGUI>().text = $"x{combo}";
        ComboFeedBack.transform.DOPunchScale(Vector3.one, timeBetweenFall / 10f);

        yield return new WaitForSeconds(timeBetweenFall / 2);

        ComboFeedBack.SetActive(false);
    }
    #endregion Combo

    #region Fall
    void Fall()
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

                    //PuyoGridPosition[xPos, yPos].transform.position += Vector3.down * ValueBetweenBlocks;
                    PuyoGridPosition[xPos, yPos].transform.DOLocalMoveY(PuyoGridPosition[xPos, yPos].transform.position.y - ValueBetweenBlocks, timeBetweenFall / 10f);

                    PuyoGridPosition[xPos, yPos] = null;

                    //Debug.Log($"xPos: {xPos} / yPos: {yPos}");
                    NoOneMove = false;
                }
            }
        }

        if (NoOneMove == true && PuyoGridFace[(int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y] == null)
        {
            bool isCombo = Combo();

            if (isCombo == false)
            {
                CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y, Range);
            }
        }
    }

    IEnumerator LoopFall(float timeBetweenFalling)
    {
        IsMoved = true;
        Fall();
        IsMoved = false;

        yield return new WaitForSeconds(timeBetweenFalling);

        if (timeBetweenFalling != timeBetweenFall / 10f)
        {
            timeBetweenFalling = timeBetweenFall / 10f;
        }

        StartCoroutine(LoopFall(timeBetweenFalling));
    }
    #endregion Fall
}
