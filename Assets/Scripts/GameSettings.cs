using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Settings")]

public class GameSettings : ScriptableObject
{
    [SerializeField] float zoomOutSpeed = 1f;
    [SerializeField] float scaleSpeed = 0.5f;
    [SerializeField] float blockMaxSize = 1f;
    [SerializeField] float blockMinSize = 0.1f;
    [SerializeField] float perfectInaccuracy = 0.05f;
    [SerializeField] float timeBetweenWaves = 0.2f;
    #region Getters
    public float ZoomOutSpeed { get { return zoomOutSpeed; } }

    public float ScaleSpeed { get { return scaleSpeed; } }

    public float BlockMaxSize { get { return blockMaxSize; } }

    public float BlockMinSize { get { return blockMinSize; } }

    public float PerfectInaccuracy { get { return perfectInaccuracy; } }

    public float TimeBetweenWaves { get { return timeBetweenWaves; } }
    #endregion
}
