using UnityEngine;

public class SyncManager : MonoBehaviour
{
    private readonly float period = 0.1f;

    private static SyncManager instance;
    public static SyncManager Instance
    {
        get
        {
            return instance;
        }
    }

    /**
    * Interpolation back time is set to 50
    * and we store 30 transform states to playback for network lag
    */

    #region  Definitions
    private float lastRequestTime = float.MaxValue;
    private double interpolationBackTime = 50;
    
    public PlayerController thisTransform;
    
    PlayerTransform[] bufferedStates = new PlayerTransform[30];
    int statesCount = 0;
    #endregion
    
    void Awake()
    {
        instance = this;
    }

    public double NetworkTime
    {
        get
        {
            return (Time.time - NetworkController.Instance.lastLocalTime) * 1000 + NetworkController.Instance.lastServerTime;
        }
    }

    /**
    * This method receives the character transform and buffers in reverse order
    */

    #region  ReceivedTransform Method
    public void ReceivedTransform(PlayerTransform chtransform)
    {
        for (int i = bufferedStates.Length - 1; i >= 1; i--)
        {
            bufferedStates[i] = bufferedStates[i - 1];
        }
        bufferedStates[0] = chtransform;
        statesCount = Mathf.Min(statesCount + 1, bufferedStates.Length);
    }
    #endregion
    
    /**
    * On a LateUpdate, we use the SmartFox Lag Monitor to determine the Ping rate
    * This is then used to decide how far back in buffered  states to present
    */

    #region On LateUpdate
    void LateUpdate()
    {
        if (!thisTransform)
            thisTransform = GetComponent<PlayerController>();
        
        if (lastRequestTime > period)
        {
            lastRequestTime = 0;
            NetworkController.Instance.TimeSyncRequest();
        }
        else
        {
            lastRequestTime += Time.deltaTime;
        }
        if (statesCount == 0) return;

        double ping = NetworkController.Instance.clientServerLag;
        if (ping < 40)
        {
            interpolationBackTime = 40;
        }
        else if (ping < 90)
        {
            interpolationBackTime = 90;
        }
        else if (ping < 150)
        {
            interpolationBackTime = 150;
        }
        else if (ping < 200)
        {
            interpolationBackTime = 200;
        }
        else
        {
            interpolationBackTime = 300;
        }

        double currentTime = NetworkTime;
        double interpolationTime = currentTime - interpolationBackTime;
        if (bufferedStates[0].TimeStamp > interpolationTime)
        {
            for (int i = 0; i < statesCount; i++)
            {
                if (bufferedStates[i].TimeStamp <= interpolationTime || i == statesCount - 1)
                {
                    PlayerTransform rhs = bufferedStates[Mathf.Max(i - 1, 0)];
                    PlayerTransform lhs = bufferedStates[i];
                    double length = rhs.TimeStamp - lhs.TimeStamp;
                    float t = 0.0F;
                    if (length > 0.0001)
                    {
                        t = (float)((interpolationTime - lhs.TimeStamp) / length);
                    }
                    //thisTransform.isMoving = true;

                    this.transform.position = Vector3.Lerp(lhs.Position, rhs.Position, t);
                    //this.transform.rotation = Quaternion.Slerp(lhs.Rotation, rhs.Rotation, t);
                    return;
                }
            }
        }

    }
    #endregion

}