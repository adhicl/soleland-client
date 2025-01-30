using System;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Unity.Cinemachine;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private GameObject playerObj;   //local player
    private List<Transform> _moles;
    public Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    
    #region event
    
    public delegate void GameEventHandler();
    public GameEventHandler OnGameStart;

    public delegate void GameOverHandler(int position);
    public GameOverHandler OnGameOver;

    public delegate void GameScoreHandler(int position, int score);
    public GameScoreHandler OnGameScore;
    
    #endregion

    public void GameStart()
    {
        UIController.Instance.SwitchMenu(UIController.MENU_STATE.GAME);
        
        SpawnMoles();
        OnGameStart?.Invoke();

        foreach(KeyValuePair<int, PlayerController> player in players)
        {
            player.Value.StartGame();
        }
        
        _hasGameStarted = true;
        timerSpringUp = 0f;
    }

    public void GameOver(int position)
    {
        OnGameOver?.Invoke(position);
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
                mole.GetComponent<Mole>().index = ((i * 4) + j);
                _moles.Add(mole);
            }
        }
    }

    public float DelaySpringUp = 1f;
    private float timerSpringUp = 1f;
    private void Update()
    {
        /*
        if (_hasGameStarted)
        {
            timerSpringUp += Time.deltaTime;
            if (timerSpringUp >= DelaySpringUp)
            {
                RandomSpringUpMole();
                timerSpringUp = 0f;
            }
        }
        //*/
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
    
    public void SpringUpMole(int index)
    {
        _moles[index].GetComponent<Mole>().SpringUp();
        _previousRandom = index;
    }
    
    public void HandleInstantiatePlayer(ISFSObject sfsobject, SmartFox sfs)
    {
        ISFSObject playerData = sfsobject.GetSFSObject("player");
        int userId = playerData.GetInt("id");
        int score = playerData.GetInt("score");
        int position = playerData.GetInt("position");
        
        PlayerTransform chtransform = PlayerTransform.FromSFSObject(playerData);
        User user = sfs.UserManager.GetUserById(userId);
        string name = user.Name;
        if (userId == sfs.MySelf.Id)
        {
            if (playerObj == null)
            {
                playerObj = GameObject.Instantiate(playerPrefab) as GameObject;
                playerObj.transform.position = chtransform.Position;
                playerObj.transform.localEulerAngles = chtransform.AngleRotationFPS;
                playerObj.name = user.Name;
                PlayerController localPlayerController = playerObj.GetComponent<PlayerController>();
                localPlayerController.isPlayer = true;
                localPlayerController.score = score;
                localPlayerController.position = position;
                
                players[userId] = playerObj.GetComponent<PlayerController>();
            }
            else
            {
                PlayerController localPlayerController = playerObj.GetComponent<PlayerController>();
                localPlayerController.transform.position = chtransform.Position;
                //NetworkController.Instance.SendTransform(localPlayerController.lastState);
            }
        }
        else
        {
            GameObject playerObj = GameObject.Instantiate(playerPrefab) as GameObject;
            playerObj.transform.position = chtransform.Position;
            playerObj.transform.localEulerAngles = chtransform.AngleRotationFPS;
            PlayerController remotePlayerController = playerObj.GetComponent<PlayerController>();
            remotePlayerController.isPlayer = false;
            remotePlayerController.score = score;
            remotePlayerController.position = position;
            
            playerObj.name = user.Name;
            
            players[userId] = playerObj.GetComponent<PlayerController>();
        }
    }

    public void ReceivePlayerScore(int position, int score)
    {
        OnGameScore?.Invoke(position, score);
    }
}