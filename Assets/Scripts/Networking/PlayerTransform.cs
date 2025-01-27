using System;
using Sfs2X.Entities.Data;
using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    private Vector3 position;
    private Vector3 angleRotation;
    private double timeStamp = 0;

    #region Transform Calls

    public Vector3 Position
    {
        get { return position; }
    }

    public Vector3 AngleRotation
    {
        get { return angleRotation; }
    }

    public Vector3 AngleRotationFPS
    {
        get { return new Vector3(angleRotation.x, angleRotation.y, angleRotation.z); }
    }

    public Quaternion Rotation
    {
        get { return Quaternion.Euler(AngleRotationFPS); }
    }

    #endregion

    /**
    * The Timestamp is added to the Transform Object for Interpolation
    */

    #region TimeStamp Method

    public double TimeStamp
    {
        get { return timeStamp; }
        set { timeStamp = value; }
    }

    #endregion

    /**
    * Add the Characters Transform position, rotation and spoine rotation to an SFS Object
    */

    #region Add Transform to SFSObject

    public void ToSFSObject(ISFSObject data)
    {
        ISFSObject tr = new SFSObject();
        tr.PutDouble("x", Convert.ToDouble(this.position.x));
        tr.PutDouble("y", Convert.ToDouble(this.position.y));
        tr.PutDouble("z", Convert.ToDouble(this.position.z));
        tr.PutDouble("rx", Convert.ToDouble(this.angleRotation.x));
        tr.PutDouble("ry", Convert.ToDouble(this.angleRotation.y));
        tr.PutDouble("rz", Convert.ToDouble(this.angleRotation.z));
        tr.PutLong("t", Convert.ToInt64(this.timeStamp));
        data.PutSFSObject("transform", tr);
    }

    public void Load(PlayerTransform chtransform)
    {
        this.position = chtransform.position;
        this.angleRotation = chtransform.angleRotation;
        this.timeStamp = chtransform.timeStamp;
    }

    #endregion

    /**
    * Extract the Characters Transform position, rotation and spoine rotation from the SFS Object
    */

    #region Extract Transform to SFSObject

    public static PlayerTransform FromSFSObject(ISFSObject data)
    {
        PlayerTransform chtransform = new PlayerTransform();
        ISFSObject transformData = data.GetSFSObject("transform");
        float x = Convert.ToSingle(transformData.GetDouble("x"));
        float y = Convert.ToSingle(transformData.GetDouble("y"));
        float z = Convert.ToSingle(transformData.GetDouble("z"));
        float rx = Convert.ToSingle(transformData.GetDouble("rx"));
        float ry = Convert.ToSingle(transformData.GetDouble("ry"));
        float rz = Convert.ToSingle(transformData.GetDouble("rz"));
        chtransform.position = new Vector3(x, y, z);
        chtransform.angleRotation = new Vector3(rx, ry, rz);
        if (transformData.ContainsKey("t"))
        {
            chtransform.TimeStamp = Convert.ToDouble(transformData.GetLong("t"));
        }
        else
        {
            chtransform.TimeStamp = 0;
        }

        return chtransform;
    }

    public static PlayerTransform FromTransform(Transform transform, Transform spine)
    {
        PlayerTransform trans = new PlayerTransform();
        trans.position = transform.position;
        trans.angleRotation = transform.localEulerAngles;
        return trans;
    }

    public void ResetTransform(Transform trans)
    {
        trans.position = this.Position;
        trans.localEulerAngles = this.AngleRotation;
    }

    #endregion
}