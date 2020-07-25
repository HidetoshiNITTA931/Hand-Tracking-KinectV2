using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class HandTrackingManager : MonoBehaviour
{
    private KinectSensor kinectSensor;
    private MultiSourceFrameReader _Reader;
    private int bodyCount;
    private Body[] bodies;
    private BodySourceManager bodySourceManager;
    public GameObject bodyManager;
    
    public Camera ConvertCamera;
    private CoordinateMapper _CoordinateMapper;
    private int _KinectWidth = 1920;
    private int _KinectHeight = 1080;

    public GameObject handDot;


    // Start is called before the first frame update
    void Start()
    {
        // one sensor is currently supported
        kinectSensor = KinectSensor.GetDefault();

        // set the maximum number of bodies that would be tracked by Kinect
        bodyCount = kinectSensor.BodyFrameSource.BodyCount;

        // allocate storage to store body objects
        bodies = new Body[bodyCount];
        bodySourceManager = bodyManager.GetComponent<BodySourceManager>();

        _Reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color |
                                                          FrameSourceTypes.Depth |
                                                          FrameSourceTypes.Infrared |
                                                          FrameSourceTypes.Body);
    }

    void LateUpdate()
    {
        bodies = bodySourceManager.GetData();
        if (bodies == null)
        {
            Debug.Log("No Bodies");
            return;
        }

        if (_CoordinateMapper == null)
        {
            _CoordinateMapper = bodySourceManager.Sensor.CoordinateMapper;
        }

        // iterate through each body and update face source
        for (int i = 0; i < bodyCount; i++)
        {
            var body = bodies[i];
            if (body != null)
            {
                if (body.IsTracked)
                {
                    Windows.Kinect.Joint handRight = body.Joints[JointType.HandRight];
                    Windows.Kinect.Joint thumbRight = body.Joints[JointType.ThumbRight];
                    Windows.Kinect.Joint handLeft = body.Joints[JointType.HandLeft];
                    Windows.Kinect.Joint thumbLeft = body.Joints[JointType.ThumbLeft];
                    Vector3 hr = GetVector3FromJoint(handRight);
                    generatePoint(hr);
                    Debug.LogFormat("{0} {1}", body.HandLeftState, body.HandRightState);
                }
            }
        }
    }

    void generatePoint(Vector3 vec)
    {
        GameObject newHandDot = Instantiate(handDot, vec, Quaternion.identity) as GameObject;
    }


    private Vector3 GetVector3FromJoint(Windows.Kinect.Joint joint)
    {
        var valid = joint.TrackingState != Windows.Kinect.TrackingState.NotTracked;
        if (ConvertCamera != null || valid)
        {
            // KinectのCamera座標系(3次元)をColor座標系(2次元)に変換する
            var point = _CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
            var point2 = new Vector3(point.X, point.Y, 0);
            if ((0 <= point2.x) && (point2.x < _KinectWidth) &&
                 (0 <= point2.y) && (point2.y < _KinectHeight))
            {

                // スクリーンサイズで調整(Kinect->Unity)
                point2.x = point2.x * Screen.width / _KinectWidth;
                point2.y = point2.y * Screen.height / _KinectHeight;

                // Unityのワールド座標系(3次元)に変換
                var colorPoint3 = ConvertCamera.ScreenToWorldPoint(point2);

                // 座標の調整
                // Y座標は逆、Z座標は-1にする(Xもミラー状態によって逆にする必要あり)
                colorPoint3.y *= -1;
                colorPoint3.z = 0;

                return colorPoint3;
            }
        }

        return new Vector3(joint.Position.X * 10,
                            joint.Position.Y * 10,
                            0);
    }
}
