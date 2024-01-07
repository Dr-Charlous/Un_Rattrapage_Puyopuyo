using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

[Serializable]
public class Puyo
{
    public string Name;
    public int PourcentageSpawn = 1;
    public Sprite[] Sprites;

    public void ChangeSprite(int xPos, int yPos, int width, int height, Puyo sprites, Puyo[,] PuyoGridFace, GameObject[,] PuyoGridPosition)
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

        PuyoGridPosition[xPos, yPos].gameObject.GetComponent<SpriteRenderer>().sprite = sprites.Sprites[visu];
        PuyoGridPosition[xPos, yPos].gameObject.transform.position = new Vector3(xPos * 0.32f, yPos * 0.32f, 0);
    }
}

public class MainGame : MonoBehaviour
{
    #region Variables
    public Color ColorGrid = Color.white;
    public Puyo[] Puyos;

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
    protected Puyo[,] puyoGridFace;
    protected Vector2 spawnPointPuyo;
    protected GameObject actualPuyo;
    protected GameObject[,] puyoGridPosition;
    protected bool isMoved = false;
    protected bool lose = false;
    protected bool[,] comboGridPosition;
    protected int height = 12;
    protected int width = 8;
    protected int comboCount = 0;
    protected int randomValue = 0;
    #endregion Variables

    void Start()
    {
        #region drawGrid
        for (float i = 0; i <= (width + 1) * ValueBetweenBlocks; i += ValueBetweenBlocks)
        {
            for (float j = 0; j <= (height + 1) * ValueBetweenBlocks; j += ValueBetweenBlocks)
            {
                if (i == 0 || i >= width * ValueBetweenBlocks || j == 0 || j >= height * ValueBetweenBlocks)
                {
                    GameObject _wall = Instantiate(BlockPrefab, new Vector3(i - ValueBetweenBlocks, j - ValueBetweenBlocks, 0), Quaternion.identity, BlockPrefabParent);
                    _wall.GetComponent<SpriteRenderer>().color = ColorGrid;
                }
            }
        }
        #endregion drawGrid

        #region setData
        spawnPointPuyo = new Vector2((int)(Mathf.Abs(width / 2f)), (int)(Mathf.Abs(height - 1)));

        puyoGridFace = new Puyo[width, height];
        puyoGridPosition = new GameObject[width, height];
        comboGridPosition = new bool[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                comboGridPosition[i, j] = false;
            }
        }

        ComboFeedBack.SetActive(false);
        EndGame.SetActive(false);
        EndGameWin.SetActive(false);
        EndGameLose.SetActive(false);
        ScoreText.text = $"Score = 0";
        #endregion setData

        randomPuyo();
        CreatePuyo((int)spawnPointPuyo.x, (int)spawnPointPuyo.y);
        StartCoroutine(LoopFall(timeBetweenFall / 10f));
    }

    void Update()
    {
        if (Timer % 3600f <= TimerEnd && lose == false)
        {
            if (isMoved == false && actualPuyo != null)
            {
                int _xPos = (int)(Mathf.Abs(actualPuyo.transform.position.x / ValueBetweenBlocks));
                int _yPos = (int)(Mathf.Abs(actualPuyo.transform.position.y / ValueBetweenBlocks));

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    Move(_xPos, _yPos, 1);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    Move(_xPos, _yPos, -1);
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

            if (Score >= ScoreEnd && lose == false)
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
        puyoGridFace[x, y] = Puyos[randomValue];
        actualPuyo = Instantiate(PuyoPrefab, new Vector3(x * ValueBetweenBlocks, y * ValueBetweenBlocks, 0), Quaternion.identity, PuyoPrefabParent);
        puyoGridPosition[x, y] = actualPuyo;

        actualPuyo.GetComponent<SpriteRenderer>().sprite = Puyos[randomValue].Sprites[0];

        randomPuyo();
    }

    void randomPuyo()
    {
        int _totalValues = 0;

        for (int i = 1; i < Puyos.Length; i++)
        {
            _totalValues += Puyos[i].PourcentageSpawn;
        }

        int _randomValue = UnityEngine.Random.Range(1, _totalValues + 1);

        for (int i = 0; i < Puyos.Length; i++)
        {
            _randomValue -= Puyos[i].PourcentageSpawn;

            if (_randomValue == 0)
            {
                randomValue = i;
                Preview.sprite = Puyos[i].Sprites[0];
                break;
            }
        }
    }

    void Move(int _xPos, int _yPos, int _value)
    {
        if (_xPos + _value < width && _xPos + _value >= 0 && puyoGridFace[_xPos + _value, _yPos] == null)
        {
            puyoGridFace[_xPos + _value, _yPos] = puyoGridFace[_xPos, _yPos];
            puyoGridFace[_xPos, _yPos] = null;

            puyoGridPosition[_xPos + _value, _yPos] = puyoGridPosition[_xPos, _yPos];
            actualPuyo.transform.DOComplete();
            actualPuyo.transform.DOLocalMoveX((_xPos + _value) * ValueBetweenBlocks, timeBetweenFall / 10f);
            puyoGridPosition[_xPos, _yPos] = null;
        }
    }
    #endregion Puyo

    #region Combo
    int ComboCheck(int _x, int _y, int _width, int _height, Puyo _sprites, int _iteration)
    {
        if (_x < _width && _x >= 0 && _y >= 0 && _y < _height)
        {
            if (puyoGridFace[_x, _y] != null)
            {
                if (comboGridPosition[_x, _y] == false && _sprites == puyoGridFace[_x, _y])
                {
                    puyoGridFace[_x, _y].ChangeSprite(_x, _y, this.width, this.height, puyoGridFace[_x, _y], puyoGridFace, puyoGridPosition);

                    comboGridPosition[_x, _y] = true;
                    _iteration++;

                    _iteration = ComboCheck(_x + 1, _y, _width, _height, _sprites, _iteration);
                    _iteration = ComboCheck(_x - 1, _y, _width, _height, _sprites, _iteration);
                    _iteration = ComboCheck(_x, _y + 1, _width, _height, _sprites, _iteration);
                    _iteration = ComboCheck(_x, _y - 1, _width, _height, _sprites, _iteration);
                }
            }
        }
        return _iteration;
    }

    bool Combo()
    {
        bool _Combo = false;

        for (float j = 0; j <= height * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= width * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                int _xPos = (int)(Mathf.Abs(i / ValueBetweenBlocks));
                int _yPos = (int)(Mathf.Abs(j / ValueBetweenBlocks));

                if ((puyoGridFace[_xPos, _yPos] != null))
                {
                    int _combo = ComboCheck(_xPos, _yPos, width, height, puyoGridFace[_xPos, _yPos], 1);

                    if (_combo > ComboRange)
                    {
                        _Combo = true;

                        StartCoroutine(ComboEffectFeedBack(Mathf.Abs(_combo - 1 - comboCount)));

                        for (float jj = 0; jj < height * ValueBetweenBlocks; jj += ValueBetweenBlocks)
                        {
                            for (float ii = 0; ii <= width * ValueBetweenBlocks; ii += ValueBetweenBlocks)
                            {
                                int xPoss = (int)(Mathf.Abs(ii / ValueBetweenBlocks));
                                int yPoss = (int)(Mathf.Abs(jj / ValueBetweenBlocks));

                                if (comboGridPosition[xPoss, yPoss] == true)
                                {
                                    if (_combo > (ComboRange + 1))
                                        Score += 10 + (_combo - ComboRange);
                                    else
                                        Score += 10;

                                    ScoreText.text = $"Score : {Score}";
                                    StartCoroutine(ComboEffectKill(xPoss, yPoss, puyoGridPosition[xPoss, yPoss].transform));
                                }
                            }
                        }
                    }

                    for (int iii = 0; iii < width; iii++)
                    {
                        for (int jjj = 0; jjj < height; jjj++)
                        {
                            comboGridPosition[iii, jjj] = false;
                        }
                    }
                }
            }
        }

        if (!_Combo)
        {
            comboCount = 0;
        }

        return _Combo;
    }

    IEnumerator ComboEffectKill(int _xPoss, int _yPoss, Transform _puyo)
    {
        _puyo.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), timeBetweenFall / 10f);

        yield return new WaitForSeconds(timeBetweenFall / 10f);
        comboCount++;

        puyoGridFace[_xPoss, _yPoss] = null;
        Destroy(puyoGridPosition[_xPoss, _yPoss]);
        puyoGridPosition[_xPoss, _yPoss] = null;
        comboGridPosition[_xPoss, _yPoss] = false;
    }

    IEnumerator ComboEffectFeedBack(int _combo)
    {
        ComboFeedBack.SetActive(true);
        ComboFeedBack.GetComponentInChildren<TextMeshProUGUI>().text = $"x{_combo}";
        //ComboFeedBack.transform.DOPunchScale(Vector3.one, timeBetweenFall / 10f);

        yield return new WaitForSeconds(timeBetweenFall / 2);

        ComboFeedBack.SetActive(false);
    }
    #endregion Combo

    #region Fall
    void Fall()
    {
        bool _NoOneMove = true;

        for (float j = 0; j <= height * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= width * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                int _xPos = (int)(Mathf.Abs(i / ValueBetweenBlocks));
                int _yPos = (int)(Mathf.Abs(j / ValueBetweenBlocks));

                if (_yPos - 1 >= 0 && puyoGridFace[_xPos, _yPos] != null && puyoGridFace[_xPos, _yPos - 1] == null)
                {
                    puyoGridFace[_xPos, _yPos - 1] = puyoGridFace[_xPos, _yPos];

                    puyoGridFace[_xPos, _yPos] = null;


                    puyoGridPosition[_xPos, _yPos - 1] = puyoGridPosition[_xPos, _yPos];

                    //puyoGridPosition[_xPos, _yPos].transform.DOComplete();
                    puyoGridPosition[_xPos, _yPos].transform.DOLocalMoveY(puyoGridPosition[_xPos, _yPos].transform.position.y - ValueBetweenBlocks, timeBetweenFall / 10f);

                    puyoGridPosition[_xPos, _yPos] = null;


                    _NoOneMove = false;
                }
                else if (puyoGridFace[_xPos, _yPos] != null && _yPos == (int)spawnPointPuyo.y)
                {
                    lose = true;
                    break;
                }
            }
        }

        if (_NoOneMove == true && puyoGridFace[(int)spawnPointPuyo.x, (int)spawnPointPuyo.y] == null)
        {
            bool _isCombo = Combo();

            if (_isCombo == false)
            {
                CreatePuyo((int)spawnPointPuyo.x, (int)spawnPointPuyo.y);
            }
        }
    }

    IEnumerator LoopFall(float _timeBetweenFalling)
    {
        isMoved = true;
        Fall();
        isMoved = false;

        yield return new WaitForSeconds(_timeBetweenFalling);

        if (_timeBetweenFalling != timeBetweenFall / 10f)
        {
            _timeBetweenFalling = timeBetweenFall / 10f;
        }

        StartCoroutine(LoopFall(_timeBetweenFalling));
    }
    #endregion Fall
}