using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    #region singleton

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion
    
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject molePrefab;

    private bool _hasGameStarted = false;
    private List<Transform> _moles;
    
    #region event
    
    public delegate void GameEventHandler();
    public GameEventHandler OnGameStart;
    public GameEventHandler OnGameOver;
    
    #endregion

    public void GameStart()
    {
        SpawnMoles();
        OnGameStart?.Invoke();
        
        _hasGameStarted = true;
        timerSpringUp = 0f;
    }

    public void GameOver()
    {
        OnGameOver?.Invoke();
    }

    private void SpawnMoles()
    {
        _moles = new List<Transform>();
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector3 pos = new Vector3((-4.5f + (i * 3f)), -3f, (-4.5f + (j * 3f)));
                Transform mole = Instantiate(molePrefab).GetComponent<Transform>();
                mole.position = pos;
                
                _moles.Add(mole);
            }
        }
    }

    public float DelaySpringUp = 1f;
    private float timerSpringUp = 1f;
    private void Update()
    {
        if (_hasGameStarted)
        {
            timerSpringUp += Time.deltaTime;
            if (timerSpringUp >= DelaySpringUp)
            {
                RandomSpringUpMole();
                timerSpringUp = 0f;
            }
        }
    }

    private int _previousRandom = -1;
    private void RandomSpringUpMole()
    {
        int index = Random.Range(0, _moles.Count);
        while (_previousRandom == index)
        {
            index = Random.Range(0, _moles.Count);
        }
        _moles[index].GetComponent<Mole>().SpringUp();
        _previousRandom = index;
    }
}