using System;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Util;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour
{
	#region singleton

	public static NetworkController Instance { get; private set; }

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
	
	//----------------------------------------------------------
	// Editor public properties
	//----------------------------------------------------------

	[Tooltip("IP address or domain name of the SmartFoxServer instance")]
	public string host = "127.0.0.1";

	[Tooltip("TCP listening port of the SmartFoxServer instance, used for TCP socket connection")]
	public int tcpPort = 9933;

	[Tooltip("UDP listening port of the SmartFoxServer instance, used for UDP communication")]
	public int UdpPort = 9933;

	[Tooltip("Name of the SmartFoxServer Zone to join")]
	public string zone = "BasicExamples";

	[Tooltip("Display SmartFoxServer client debug messages")]
	public bool debug = false;
	
    private GlobalManager gm;

    //----------------------------------------------------------
    // properties
    //----------------------------------------------------------

    private SmartFox sfs;

    public int clientServerLag;
    public double lastServerTime = 0;
    public double lastLocalTime = 0;
    
    #region event
    
    public delegate void NetworkError(string message);
    public NetworkError onError;

    public delegate void NetworkStatus();
    public NetworkStatus onConnected;
    public NetworkStatus onDisconnected;

    #endregion
    
    
    /**
	 * On start, set a reference to Global Manager.
	 */
    private void Start()
    {
	    // Get Global Manager instance
	    gm = GlobalManager.Instance;
    }
    
    private void OnDestroy()
    {
	    // Remove SFS2X listeners
	    RemoveSmartFoxListeners();
    }

	/**
	 * Connect to SmartFoxServer.
	 */
	public void Connect(string iHost, int iPort)
	{
		this.host = iHost;
		this.tcpPort = iPort;
		this.UdpPort = iPort;

		// Set connection parameters
		ConfigData cfg = new ConfigData();
		cfg.Host = host;
		cfg.Port = tcpPort;
        cfg.UdpHost = host;
        cfg.UdpPort = UdpPort;
        
        cfg.Zone = zone;
		cfg.Debug = debug;

		Debug.Log("Connecting SmartFoxServer...");
		
		// Initialize SmartFox client
		// The singleton class GlobalManager holds a reference to the SmartFox class instance,
		// so that it can be shared among all the scenes
		sfs = gm.CreateSfsClient();

		// Configure SmartFox internal logger
		sfs.Logger.EnableConsoleTrace = debug;

		// Add event listeners
		AddSmartFoxListeners();

		// Connect to SmartFoxServer
		sfs.Connect(cfg);
	}

	/**
	 * Add all SmartFoxServer-related event listeners required by the scene.
	 */
	private void AddSmartFoxListeners()
	{
		sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
        sfs.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);
        
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
        sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
        //sfs.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
        //sfs.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
    }

	/**
	 * Remove all SmartFoxServer-related event listeners added by the scene.
	 * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
	 */
	private void RemoveSmartFoxListeners()
	{
		// NOTE
		// If this scene is stopped before a connection is established, the SmartFox client instance
        // could still be null, causing an error when trying to remove its listeners

		if (sfs != null)
		{
			sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
			sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
			sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
			sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            sfs.RemoveEventListener(SFSEvent.UDP_INIT, OnUdpInit);
            
            sfs.RemoveEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
            sfs.RemoveEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
            //sfs.RemoveEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
            //sfs.RemoveEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
        }
	}

	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------
	#region
	private void OnConnection(BaseEvent evt)
	{
		// Check if the conenction was established or not
		if ((bool)evt.Params["success"])
		{
			Debug.Log("SFS2X API version: " + sfs.Version);
			Debug.Log("Connection mode is: " + sfs.ConnectionMode);

			// Login
			sfs.Send(new LoginRequest(""));
		}
		else
		{
			// Show error message
			onError?.Invoke("Connection failed; is the server running at all?");
			//errorText.text = "Connection failed; is the server running at all?";

			// Enable user interface
			onDisconnected?.Invoke();
			//EnableUI(true);
		}
	}

	private void OnConnectionLost(BaseEvent evt)
	{
		// Remove SFS listeners
		RemoveSmartFoxListeners();

		// Show error message
		string reason = (string)evt.Params["reason"];
		
		//if (reason != ClientDisconnectionReason.MANUAL)
		onError?.Invoke("Connection lost; reason is: " + reason);
		//	errorText.text = "Connection lost; reason is: " + reason;

		// Enable user interface
		onDisconnected?.Invoke();
		//EnableUI(true);
    }

    private void OnLogin(BaseEvent evt)
    {
	    // Display username in footer
	    Debug.Log("Logged in as " + sfs.MySelf.Name);
	    
        // Initialize UDP communication
        sfs.InitUDP();
    }

	private void OnLoginError(BaseEvent evt)
	{
		// Disconnect
		// NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
		sfs.Disconnect();

		// Show error message
		onError?.Invoke("Login failed due to the following error:\n" + (string)evt.Params["errorMessage"]);
		//errorText.text = "Login failed due to the following error:\n" + (string)evt.Params["errorMessage"];

		// Enable user interface
		onDisconnected?.Invoke();
		//EnableUI(true);
	}

	private void OnUdpInit(BaseEvent evt)
	{
		if ((bool)evt.Params["success"])
		{
            // Set invert mouse Y option
            //OptionsManager.InvertMouseY = invertMouseToggle.isOn;
            //SF2X_GameManager.invertMouseY = invertMouseToggle.isOn;

            // Load lobby scene
            //SceneManager.LoadScene("Lobby");
            onError?.Invoke("Joining game");

            JoinOrCreateRoom();
		}
		else
		{
			// Disconnect
			// NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
			sfs.Disconnect();

			// Show error message
			onError?.Invoke("UDP initialization failed due to the following error:\n" + (string)evt.Params["errorMessage"]);
			//errorText.text = "UDP initialization failed due to the following error:\n" + (string)evt.Params["errorMessage"];

			// Enable user interface
			onDisconnected?.Invoke();
			//EnableUI(true);
		}
	}
	
	private void OnRoomCreationError(BaseEvent evt)
	{
		// Show Warning Panel prefab instance
		onError?.Invoke("Room creation failed: " + (string)evt.Params["errorMessage"]);
		//warningPanel.Show("Room creation failed: " + (string)evt.Params["errorMessage"]);
		sfs.Disconnect();
		// Enable user interface
		onDisconnected?.Invoke();
	}

	private void OnRoomAdded(BaseEvent evt)
	{
		//AutoJoinRoom();
		//Room room = (Room)evt.Params["room"];

		// Display game list item
		//AddGameListItem(room);
	}

	private void OnRoomJoin(BaseEvent evt)
	{
		// Load game scene
		//SceneManager.LoadScene("Game");

		Debug.Log("On Join room");
		onError?.Invoke("Room Joined. Waiting for game start signal");
		
		onConnected?.Invoke();
	}

	private void OnRoomJoinError(BaseEvent evt)
	{
		// Show Warning Panel prefab instance
		onError?.Invoke("Room join failed: " + (string)evt.Params["errorMessage"]);
		//warningPanel.Show("Room join failed: " + (string)evt.Params["errorMessage"]);
		sfs.Disconnect();
		// Enable user interface
		onDisconnected?.Invoke();
	}
	
    #endregion

    /**
     * Create and join room ========================
     */
    private void JoinOrCreateRoom()
    {
	    bool isFoundRoom = AutoJoinRoom();

	    if (!isFoundRoom)
	    {
		    // Configure Room
		    RoomSettings settings = new RoomSettings(sfs.MySelf.Name + "'s game");
		    //settings.GroupId = "WhackMole";
		    settings.IsGame = true;
		    settings.MaxUsers = 2;
		    settings.MaxSpectators = 0;

		    Debug.Log("Create new room "+sfs.MySelf.Name + "'s game");
		    // Request Room creation to server
		    sfs.Send(new CreateRoomRequest(settings, true, sfs.LastJoinedRoom));
	    }
    }

    private bool AutoJoinRoom()
    {
	    List<Room> rooms = sfs.RoomManager.GetRoomList();
	    for (int i = 0; i < rooms.Count; i++)
	    {
		    if (rooms[i].UserCount < 2)
		    {
			    // Request Room creation to server
			    sfs.Send(new Sfs2X.Requests.JoinRoomRequest(rooms[i].Id));
			    return true;
		    }
	    }

	    return false;
    }

    // =================================================================
    
    // ================ game manager part ===================
    
    public void TimeSyncRequest()
    {
	    Room room = sfs.LastJoinedRoom;
	    ExtensionRequest request = new ExtensionRequest("getTime", new SFSObject(), room);
	    sfs.Send(request);
    }
}