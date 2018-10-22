using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject cylinderPrefab;
    [SerializeField] Transform tower;
    [SerializeField] Transform poolContainerTransform;
    [SerializeField] Material missBlockMaterial;
    [SerializeField] GameSettings settings;

    bool isPlaying;
    #region Camera 
    Camera mainCamera;
    Vector3 cameraStartPosition;
    float zoomOutPerBlock;
    float zoomOutSpeed;
    #endregion
    #region Blocks 
    List<Tower> towerBlocks;
    Vector3 startPosition;
    Vector3 rangeBetweenBlocks;
    GameObject prevBlock;
    GameObject currentBlock;
    #endregion
    #region Scaling 
    float scaleSpeed;
    float blockMaxScaling;
    Vector3 blockScaling;
    Vector3 blockMaxSizeVector;
    Vector3 blockMinSizeVector;
    #endregion
    #region Perfect move
    float perfectInaccuracy;
    float timeBetweenWaves;
    float waveAnimationTime;
    Vector3 buildedBlockResizeUp;
    Vector3 buildedBlockResizeDown;
    Vector3 othersBlockResizeUp;
    Vector3 othersBlockResizeScale;
    #endregion

    struct Tower
    {
        public GameObject towerObject;
        public bool isPerfectMove;

        public Tower(GameObject towerObject)
        {
            this.towerObject = towerObject;
            isPerfectMove = false;
        }
        public void PerfectMove()
        {
            isPerfectMove = true;
        }
    }

    void Start()
    {
        LoadGameSettings();
        FieldsInitialization();
        StartGame();
    }

    void LoadGameSettings()
    {
        zoomOutSpeed = settings.ZoomOutSpeed;
        scaleSpeed = settings.ScaleSpeed;
        blockMaxSizeVector = new Vector3(settings.BlockMaxSize, 0.25f, settings.BlockMaxSize);
        blockMinSizeVector = new Vector3(settings.BlockMinSize, 0.25f, settings.BlockMinSize);
        perfectInaccuracy = settings.PerfectInaccuracy;
        timeBetweenWaves = settings.TimeBetweenWaves;
    }

    void FieldsInitialization()
    {
        PoolManager.Initialization(poolContainerTransform);

        mainCamera = Camera.main;
        cameraStartPosition = mainCamera.transform.position;
        zoomOutPerBlock = 0.25f;

        startPosition = new Vector3(0, 0, 0);
        rangeBetweenBlocks = new Vector3(0f, cylinderPrefab.transform.localScale.y, 0f);

        blockScaling = new Vector3(scaleSpeed, 0, scaleSpeed);
        blockMaxScaling = 1.1f;

        waveAnimationTime = timeBetweenWaves / 2;
        buildedBlockResizeUp = new Vector3(0.4f, 0, 0.4f);
        buildedBlockResizeDown = new Vector3(-0.2f, 0, -0.2f);
        othersBlockResizeUp = new Vector3(0.3f, 0, 0.3f);
        othersBlockResizeScale = new Vector3(0.8f, 1, 0.8f);
    }

    void StartGame()
    {
        towerBlocks = new List<Tower>();
        isPlaying = true;
        mainCamera.transform.position = cameraStartPosition;
        SpawnBlock();

    }

    void Update()
    {
        if (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                SpawnBlock();
            }
            if (Input.GetKey(KeyCode.Mouse0) && currentBlock != null)
            {
                if (currentBlock.transform.localScale.x >= prevBlock.transform.localScale.x * blockMaxScaling) GameOver(); //if scaling block alrdy is too big
                BlockScaling();
            }
            if (Input.GetKeyUp(KeyCode.Mouse0) && currentBlock != null)
            {
                if (currentBlock.transform.localScale.x > prevBlock.transform.localScale.x) GameOver(); //if curr block bigger than prev
                else
                {
                    if (IsPerfectMove())
                    {
                        towerBlocks[towerBlocks.Count - 1].PerfectMove();
                        StartCoroutine(TowerWave());
                    }
                    prevBlock = currentBlock;
                    currentBlock = null;
                    mainCamera.transform.position += rangeBetweenBlocks;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) Restart();
        }
    }

    void SpawnBlock()
    {
        if (prevBlock != null) // if there is start point
        {
            currentBlock = PoolManager.GetObject(cylinderPrefab);
            currentBlock.GetComponent<Renderer>().material = cylinderPrefab.GetComponent<Renderer>().sharedMaterial;
            currentBlock.transform.localScale = cylinderPrefab.transform.localScale;
            currentBlock.transform.position = prevBlock.transform.position + rangeBetweenBlocks;
            currentBlock.transform.parent = tower.transform;
            towerBlocks.Add(new Tower(currentBlock));
        }
        else //Creating start point
        {
            prevBlock = PoolManager.GetObject(cylinderPrefab);
            prevBlock.transform.localScale = blockMaxSizeVector;
            prevBlock.transform.position = startPosition;
            prevBlock.transform.parent = tower.transform;
            towerBlocks.Add(new Tower(prevBlock));
        }
    }

    void BlockScaling()
    {
        currentBlock.transform.localScale += blockScaling * Time.deltaTime;
    }

    bool IsPerfectMove()
    {
        return prevBlock.transform.localScale.x - currentBlock.transform.localScale.x <= perfectInaccuracy;
    }

    IEnumerator TowerWave()
    {
        for (int i = towerBlocks.Count - 1; i >= 0; i--)
        {
            Vector3 resizeTo;
            Vector3 initialSize = towerBlocks[i].towerObject.transform.localScale;

            if (i == towerBlocks.Count - 1) //scalling just placed block
            {
                resizeTo = initialSize + buildedBlockResizeUp;
                StartCoroutine(ResizeAnimation(towerBlocks[i].towerObject, resizeTo, waveAnimationTime));//scaling it up
                yield return new WaitForSeconds(waveAnimationTime);

                resizeTo = towerBlocks[i].towerObject.transform.localScale + buildedBlockResizeDown;
                StartCoroutine(ResizeAnimation(towerBlocks[i].towerObject, resizeTo, waveAnimationTime));//scaling it down
                yield return new WaitForSeconds(waveAnimationTime);
            }
            else
            {
                resizeTo = initialSize + othersBlockResizeUp;
                StartCoroutine(ResizeAnimation(towerBlocks[i].towerObject, resizeTo, waveAnimationTime)); //scaling up others blocks
                yield return new WaitForSeconds(waveAnimationTime);

                if (!towerBlocks[i].isPerfectMove)
                {
                    resizeTo = Vector3.Scale(initialSize, othersBlockResizeScale);
                    StartCoroutine(ResizeAnimation(towerBlocks[i].towerObject, resizeTo, waveAnimationTime)); //scaling down block wo perfectmove
                    yield return new WaitForSeconds(waveAnimationTime);
                }
                else
                {
                    resizeTo = initialSize;
                    StartCoroutine(ResizeAnimation(towerBlocks[i].towerObject, resizeTo, waveAnimationTime)); //scaling down block with perfectmove
                    yield return new WaitForSeconds(waveAnimationTime);
                }
            }
        }
        yield break;
    }

    IEnumerator ResizeAnimation(GameObject block, Vector3 sizeTo, float timeToSize)
    {
        float elapsedTime = 0;

        Vector3 startingSize = block.transform.localScale;
        if (sizeTo.magnitude > blockMaxSizeVector.magnitude) sizeTo = blockMaxSizeVector;
        if (sizeTo.magnitude < blockMinSizeVector.magnitude) sizeTo = blockMinSizeVector;

        while (elapsedTime < timeToSize)
        {
            block.transform.localScale = Vector3.Lerp(startingSize, sizeTo, (elapsedTime / timeToSize));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        block.transform.localScale = sizeTo;
    }

    void GameOver()
    {
        isPlaying = false;
        currentBlock.GetComponent<MeshRenderer>().material = missBlockMaterial;
        StartCoroutine(CameraZoomOut());
    }

    IEnumerator CameraZoomOut()
    {
        float elapsedTime = 0;
        Vector3 startingPos = mainCamera.transform.position;
        Vector3 endPos = startingPos + new Vector3(0, 0, mainCamera.transform.position.z - zoomOutPerBlock * tower.childCount);
        while (elapsedTime < zoomOutSpeed)
        {
            mainCamera.transform.position = Vector3.Lerp(startingPos, endPos, (elapsedTime / zoomOutSpeed));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        mainCamera.transform.position = endPos;
    }

    void Restart()
    {
        StopAllCoroutines();
        ClearingVariables();
        MoveObjectsToPool();
        StartGame();
    }

    void ClearingVariables()
    {
        towerBlocks.Clear();
        prevBlock = null;
        currentBlock = null;
    }

    void MoveObjectsToPool()
    {
        int towerLength = tower.childCount;
        while (towerLength != 0)
        {
            PoolManager.PutGameObject(tower.GetChild(towerLength - 1).gameObject);
            towerLength--;
        }
    }
}
