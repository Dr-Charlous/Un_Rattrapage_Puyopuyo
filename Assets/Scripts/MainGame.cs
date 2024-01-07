using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

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

[Serializable]
public class Puyo
{
    public PuyoSprites sprites;
    public int PourcentageSpawn = 1;
}

public class MainGame : MonoBehaviour
{
    #region Variables
    public Color ColorGrid = Color.white;
    public Puyo[] sprites;

    [Header("Parents for instances : ")]
    public Transform BlockPrefabParent;
    public Transform PuyoPrefabParent;

    [Header("Objects reference : ")]
    public GameObject BlockPrefab;
    public GameObject PuyoPrefab;
    public GameObject ComboFeedBack;
    public GameObject EndGame;
    public GameObject EndGameWin;
    public GameObject EndGameLose;
    public GameObject ButtonNextLevel;

    [Header("Ui references : ")]
    public Image Preview;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI ScoreTextEnd;

    [Header("Values : ")]
    public float ValueBetweenBlocks = 0.32f;
    public float Timer = 0;
    public int TimerEnd = 60;
    public int Score = 0;
    public int ScoreEnd = 100;
    public int ComboRange = 4;
    [Range(1, 10)]
    public int timeBetweenFall = 5;

    [Header("")]
    protected PuyoSprites[,] PuyoGridFace;
    protected Vector2 SpawnPointPuyo;
    protected GameObject ActualPuyo;
    protected GameObject[,] PuyoGridPosition;
    protected bool IsMoved = false;
    protected bool Lose = false;
    protected bool[,] ComboGridPosition;
    protected int Height = 12;
    protected int Width = 8;
    protected int ComboCount = 0;
    protected int RandomValue = 0;
    #endregion Variables

    void Start()
    {
        #region drawGrid
        for (float i = 0; i <= (Width + 1) * ValueBetweenBlocks; i += ValueBetweenBlocks)
        {
            for (float j = 0; j <= (Height + 1) * ValueBetweenBlocks; j += ValueBetweenBlocks)
            {
                if (i == 0 || i >= Width * ValueBetweenBlocks || j == 0 || j >= Height * ValueBetweenBlocks)
                {
                    GameObject wall = Instantiate(BlockPrefab, new Vector3(i - ValueBetweenBlocks, j - ValueBetweenBlocks, 0), Quaternion.identity, BlockPrefabParent);
                    wall.GetComponent<SpriteRenderer>().color = ColorGrid;
                }
            }
        }
        #endregion drawGrid

        #region setData
        SpawnPointPuyo = new Vector2((int)(Mathf.Abs(Width / 2f)), (int)(Mathf.Abs(Height - 1)));

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
        EndGame.SetActive(false);
        EndGameWin.SetActive(false);
        EndGameLose.SetActive(false);
        ScoreText.text = $"Score = 0";
        #endregion setData

        randomPuyo();
        CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y);
        StartCoroutine(LoopFall(timeBetweenFall / 10f));
    }

    void Update()
    {
        if (Timer % 3600f <= TimerEnd && Lose == false)
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

            Timer += Time.deltaTime;

            TimerText.text = $"Timer =  {(int)(Timer / 60f)} : {(int)(Timer % 60f)}s";
        }
        else
        {
            StopAllCoroutines();
            EndGame.SetActive(true);

            ScoreTextEnd.text = $"Score = {Score}";

            if (Score >= ScoreEnd && Lose == false)
            {
                EndGameWin.SetActive(true);
                ButtonNextLevel.GetComponent<Button>().enabled = true;
            }
            else
            {
                EndGameLose.SetActive(true);
                ButtonNextLevel.GetComponent<Button>().enabled = false;
            }
        }
    }

    #region Puyo
    void CreatePuyo(int x, int y)
    {
        PuyoGridFace[x, y] = sprites[RandomValue].sprites;
        ActualPuyo = Instantiate(PuyoPrefab, new Vector3(x * ValueBetweenBlocks, y * ValueBetweenBlocks, 0), Quaternion.identity, PuyoPrefabParent);
        PuyoGridPosition[x, y] = ActualPuyo;

        ActualPuyo.GetComponent<SpriteRenderer>().sprite = sprites[RandomValue].sprites.sprite[0];

        randomPuyo();
    }

    void randomPuyo()
    {
        int totalValues = 0;

        for (int i = 1; i < sprites.Length; i++)
        {
            totalValues += sprites[i].PourcentageSpawn;
        }

        int randomValue = UnityEngine.Random.Range(1, totalValues+1);

        for (int i = 0; i < sprites.Length; i++)
        {
            randomValue -= sprites[i].PourcentageSpawn;

            if (randomValue == 0)
            {
                RandomValue = i;
                Preview.sprite = sprites[i].sprites.sprite[0];
                break;
            }
        }
    }

    void Move(int xPos, int yPos, int value)
    {
        if (xPos + value < Width && xPos + value >= 0 && PuyoGridFace[xPos + value, yPos] == null)
        {
            PuyoGridFace[xPos + value, yPos] = PuyoGridFace[xPos, yPos];
            PuyoGridFace[xPos, yPos] = null;

            PuyoGridPosition[xPos + value, yPos] = PuyoGridPosition[xPos, yPos];
            ActualPuyo.transform.DOComplete();
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
                                    if (combo > (ComboRange + 1))
                                        Score += 10 + (combo - ComboRange);
                                    else
                                        Score += 10;

                                    ScoreText.text = $"Score : {Score}";
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
        //ComboFeedBack.transform.DOPunchScale(Vector3.one, timeBetweenFall / 10f);

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

                if (yPos - 1 >= 0 && PuyoGridFace[xPos, yPos] != null && PuyoGridFace[xPos, yPos - 1] == null)
                {
                    PuyoGridFace[xPos, yPos - 1] = PuyoGridFace[xPos, yPos];

                    PuyoGridFace[xPos, yPos] = null;


                    PuyoGridPosition[xPos, yPos - 1] = PuyoGridPosition[xPos, yPos];

                    //PuyoGridPosition[xPos, yPos].transform.DOComplete();
                    PuyoGridPosition[xPos, yPos].transform.DOLocalMoveY(PuyoGridPosition[xPos, yPos].transform.position.y - ValueBetweenBlocks, timeBetweenFall / 10f);

                    PuyoGridPosition[xPos, yPos] = null;


                    NoOneMove = false;
                }
                else if (PuyoGridFace[xPos, yPos] != null && yPos == (int)SpawnPointPuyo.y)
                {
                    Lose = true;
                    break;
                }
            }
        }

        if (NoOneMove == true && PuyoGridFace[(int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y] == null)
        {
            bool isCombo = Combo();

            if (isCombo == false)
            {
                CreatePuyo((int)SpawnPointPuyo.x, (int)SpawnPointPuyo.y);
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