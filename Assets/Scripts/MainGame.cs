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
    public GameObject BlockPrefab;
    public GameObject PuyoPrefab;
    public GameObject[,] PuyoGridPosition;
    [Header("")]
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

        for (int i = 0; i < 8; i++)
        {
            PuyoGridFace[i, 5] = sprites[0];
            PuyoGridPosition[i, 5] = Instantiate(PuyoPrefab, new Vector3(i * ValueBetweenBlocks, 5 * ValueBetweenBlocks, 0), Quaternion.identity, PuyoPrefabParent);
        }
        #endregion

        StartCoroutine(LoopFall(timeBetweenFall));
    }

    public IEnumerator LoopFall(float timeBetweenFall)
    {
        for (float j = 0; j <= 11 * ValueBetweenBlocks; j += ValueBetweenBlocks)
        {
            for (float i = 0; i <= 8 * ValueBetweenBlocks; i += ValueBetweenBlocks)
            {
                if ((PuyoGridFace[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks)] != null) && ((int)(j / ValueBetweenBlocks) - 1 >= 0) && PuyoGridFace[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks) - 1] == null)
                {
                    PuyoGridFace[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks) - 1] = PuyoGridFace[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks)];

                    PuyoGridFace[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks)] = null;

                    PuyoGridPosition[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks) - 1] = PuyoGridPosition[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks)];

                    PuyoGridPosition[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks)].transform.position += Vector3.down * ValueBetweenBlocks;

                    PuyoGridPosition[(int)(i / ValueBetweenBlocks), (int)(j / ValueBetweenBlocks)] = null;


                    Debug.Log($"x: {(int)(i / ValueBetweenBlocks)} / y: {(int)(j / ValueBetweenBlocks)}");
                }
            }
        }

        yield return new WaitForSeconds(timeBetweenFall);
        StartCoroutine(LoopFall(timeBetweenFall));
    }
}
