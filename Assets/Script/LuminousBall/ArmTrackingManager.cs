using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class ArmTrackingManager : MonoBehaviour
{
    private KinectSensor kinectSensor;
    private MultiSourceFrameReader _Reader;
    private int bodyCount;
    private Body[] bodies;
    private BodySourceManager bodySourceManager;
    public GameObject armTrackingManager;

    public Camera ConvertCamera;
    private CoordinateMapper _CoordinateMapper;
    private int _KinectWidth = 1920;
    private int _KinectHeight = 1080;

    // 右手と左手のビーム
    private LuminousManage luminousManageR;
    private LuminousManage luminousManageL;
    public GameObject luminousManagerR;
    public GameObject luminousManagerL;

    // HandStat=Openを何フレームか見失っても続けるための定数
    private int noOpenCounterR = 0;
    private int noOpenCounterL = 0;
    private int CounterTh = 10;

    // Start is called before the first frame update
    void Start()
    {
        // one sensor is currently supported
        kinectSensor = KinectSensor.GetDefault();

        // set the maximum number of bodies that would be tracked by Kinect
        bodyCount = kinectSensor.BodyFrameSource.BodyCount;

        // allocate storage to store body objects
        bodies = new Body[bodyCount];
        bodySourceManager = armTrackingManager.GetComponent<BodySourceManager>();

        _Reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color |
                                                          FrameSourceTypes.Depth |
                                                          FrameSourceTypes.Infrared |
                                                          FrameSourceTypes.Body);
        
        luminousManageR = luminousManagerR.GetComponent<LuminousManage>();
        luminousManageL = luminousManagerL.GetComponent<LuminousManage>();
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
                    Windows.Kinect.Joint elbowRight = body.Joints[JointType.ElbowRight];
                    Windows.Kinect.Joint handLeft = body.Joints[JointType.HandLeft];
                    Windows.Kinect.Joint elbowLeft = body.Joints[JointType.ElbowLeft];
                    Vector3 handR = GetVector3FromJoint(handRight);
                    Vector3 handL = GetVector3FromJoint(handLeft);

                    // kinectの座標系はz標高がUnity座標系と逆なので-1をかける
                    Vector3 directionR = GetElboToHand(elbowRight, handRight);
                    directionR.z = directionR.z * -1;
                    Vector3 directionL = GetElboToHand(elbowLeft, handLeft);
                    directionL.z = directionL.z * -1;
                    Debug.Log(body.HandRightState);

                    float ration = Time.deltaTime;
                    // Right hand
                    if (body.HandRightState == HandState.Open)
                    {
                        luminousManageR.Attack(ration, handR, directionR);
                        noOpenCounterR = 0;
                    }
                    else
                    {
                        if (noOpenCounterR >= CounterTh)
                        {
                            luminousManageR.StopBeem();
                        }
                        else
                        {
                            noOpenCounterR += 1;
                        }
                    }

                    // Left hand
                    if (body.HandLeftState == HandState.Open)
                    {   
                        luminousManageL.Attack(ration, handL, directionL);
                        noOpenCounterL = 0;
                    }
                    else
                    {
                        if (noOpenCounterL >= CounterTh)
                        {
                            luminousManageL.StopBeem();
                        }
                        else
                        {
                            noOpenCounterL += 1;
                        }
                    }

                }
            }
        }
    }

    // RGB画面上の座標に変換する
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

    // 肘から手までのベクトルを計算する
    private Vector3 GetElboToHand(Windows.Kinect.Joint elbow, Windows.Kinect.Joint hand)
    {
        return new Vector3(hand.Position.X - elbow.Position.X, hand.Position.Y - elbow.Position.Y, hand.Position.Z - elbow.Position.Z);
    }

    

}
