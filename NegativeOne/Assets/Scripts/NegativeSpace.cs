using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegativeSpace : MonoBehaviour {

    private Main _main;
    private Properties _properties;
    private BodiesManager _bodiesManager;

    private bool _spaceCreated = false;
    private Location _location;
    private SurfaceRectangle _localSurface;
    private SurfaceRectangle _remoteSurfaceProxy;
    private float _negativeSpaceLength;

    public Material negativeSpaceMaterial;
    

    private UDPHandheldListener _handheldListener;

    public GameObject NegativeSpaceCenter { get; private set;}

    private Dictionary<string, GameObject> _negativeSpaceObjects;
    private Dictionary<string, GameObject> negativeSpaceObjects { get { return _negativeSpaceObjects; } }

    private GameObject _handCursor;
    public Vector3 bottomCenterPosition { get; private set; }

    private AdaptiveDoubleExponentialFilterVector3 _filteredHandPosition;

    public SurfaceRectangle LocalSurface
    {
        get
        {
            return _localSurface;
        }
    }

    void Awake()
    {
        _negativeSpaceObjects = new Dictionary<string, GameObject>();
    }

    void Start ()
    {
        _main = GetComponent<Main>();
        _properties = GetComponent<Properties>();
        _filteredHandPosition = new AdaptiveDoubleExponentialFilterVector3();
        _bodiesManager = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
	}

    internal void create(Location location, SurfaceRectangle localSurface, SurfaceRectangle remoteSurfaceProxy, float length)
    {
        _handheldListener = new UDPHandheldListener(int.Parse(_properties.localSetupInfo.receiveHandheldPort));
        Debug.Log(this.ToString() + ": Receiving Handheld data in " + _properties.localSetupInfo.receiveHandheldPort);

        _location = location;
        _localSurface = localSurface;
        _remoteSurfaceProxy = remoteSurfaceProxy;
        _negativeSpaceLength = length;

        //_createNegativeSpaceMesh();
        _negativeSpaceWalls("bottomWall", _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomLeft);
        _negativeSpaceWalls("leftWall", _localSurface.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceTopLeft, _localSurface.SurfaceTopLeft);
        _negativeSpaceWalls("rightWall", _remoteSurfaceProxy.SurfaceBottomRight, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopRight);
        _negativeSpaceWalls("topWall", _remoteSurfaceProxy.SurfaceTopLeft, _remoteSurfaceProxy.SurfaceTopRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft);


        NegativeSpaceCenter = new GameObject("NegativeSpaceCenter");
        NegativeSpaceCenter.transform.position = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceTopRight) * 0.5f;
        NegativeSpaceCenter.transform.rotation = GameObject.Find("localScreenCenter").transform.rotation;

        bottomCenterPosition = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceBottomRight) * 0.5f;

        _handCursor = new GameObject("HandCursor");
        _handCursor.transform.position = Vector3.zero;
        _handCursor.transform.rotation = Quaternion.identity;
        _handCursor.transform.parent = _main.LocalOrigin.transform;
        _handCursor.AddComponent<HandCursor>();

        GameObject projector = GameObject.Find("Projector");
        projector.transform.parent = _handCursor.transform;
        projector.transform.localPosition = Vector3.zero;
        projector.transform.rotation = _handCursor.transform.rotation;

        _spaceCreated = true;
    }

    private void _negativeSpaceWalls(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        GameObject o = new GameObject(name);
        o.transform.position = Vector3.zero;
        o.transform.rotation = Quaternion.identity;
        o.transform.parent = transform;

        MeshFilter meshFilter = (MeshFilter)o.AddComponent(typeof(MeshFilter));
        Mesh m = new Mesh();
        m.name = "NegativeSpaceMesh";
        m.vertices = new Vector3[] { a, b, c, d };
        m.triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

        Vector2[] uv = new Vector2[m.vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(m.vertices[i].x, m.vertices[i].z);
        }
        m.uv = uv;

        m.RecalculateNormals();
        m.RecalculateBounds();

        meshFilter.mesh = m;
        MeshRenderer renderer = o.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = negativeSpaceMaterial;
        MeshCollider collider = o.AddComponent(typeof(MeshCollider)) as MeshCollider;

    }

    private void _createNegativeSpaceMesh()
    {
        MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        Mesh m = new Mesh();
        m.name = "NegativeSpaceMesh";
        m.vertices = new Vector3[]
            {
                _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft,
                _remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopLeft
            };

        m.triangles = new int[]
            {
                    0, 4, 3,
                    0, 1, 4,
                    1, 5, 4,
                    1, 2, 5,
                    2, 6, 5,
                    2, 7, 6,
                    3, 7, 2,
                    3, 4, 7
            };

        Vector2[] uv = new Vector2[m.vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(m.vertices[i].x, m.vertices[i].z);
        }
        m.uv = uv;

        m.RecalculateNormals();
        m.RecalculateBounds();

        meshFilter.mesh = m;
        MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = negativeSpaceMaterial;
        MeshCollider collider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
    }

    void Update()
    {
        if (_spaceCreated)
        {
            CommonUtils.drawSurface(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft, Color.red);
            CommonUtils.drawSurface(_remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopLeft, Color.green);

            if (_bodiesManager.human != null)
            {
                Vector3 head = _bodiesManager.human.body.Joints[BodyJointType.head];
                Vector3 leftHand = _bodiesManager.human.body.Joints[BodyJointType.leftWrist];
                Vector3 rightHand = _bodiesManager.human.body.Joints[BodyJointType.rightWrist];

                //_handCursor.transform.position = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
                _filteredHandPosition.Value = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
                _handCursor.transform.position = _filteredHandPosition.Value;

                if (_main.location == Location.Assembler) // Instructor cannot interact yolo
                {
                    _handCursor.GetComponent<HandCursor>().Update(_handheldListener.Message);
                }
                Camera.main.transform.position = head;

                GameObject lb =  GameObject.Find("LocalBody");
                if (Application.isEditor && lb != null && lb.activeSelf)
                {

                    GameObject.Find("HEAD").transform.position = head;
                    GameObject.Find("LEFTHAND").transform.position = leftHand;
                    GameObject.Find("RIGHTHAND").transform.position = rightHand;
                    GameObject.Find("RIGHTHAND").transform.position = rightHand;

                    GameObject.Find("NECK").transform.position = _bodiesManager.human.body.Joints[BodyJointType.neck];
                    GameObject.Find("SPINESHOULDER").transform.position = _bodiesManager.human.body.Joints[BodyJointType.spineShoulder];
                    GameObject.Find("LEFTSHOULDER").transform.position = _bodiesManager.human.body.Joints[BodyJointType.leftShoulder];
                    GameObject.Find("RIGHTSHOULDER").transform.position = _bodiesManager.human.body.Joints[BodyJointType.rightShoulder];
                    GameObject.Find("LEFTELBOW").transform.position = _bodiesManager.human.body.Joints[BodyJointType.leftElbow];
                    GameObject.Find("RIGHTELBOW").transform.position = _bodiesManager.human.body.Joints[BodyJointType.rightElbow];
                    GameObject.Find("LEFTWRIST").transform.position = _bodiesManager.human.body.Joints[BodyJointType.leftWrist];
                    GameObject.Find("RIGHTWRIST").transform.position = _bodiesManager.human.body.Joints[BodyJointType.rightWrist];
                    GameObject.Find("SPINEBASE").transform.position = _bodiesManager.human.body.Joints[BodyJointType.spineBase];
                    GameObject.Find("SPINEMID").transform.position = _bodiesManager.human.body.Joints[BodyJointType.spineMid];
                    GameObject.Find("LEFTHIP").transform.position = _bodiesManager.human.body.Joints[BodyJointType.leftHip];
                    GameObject.Find("RIGHTHIP").transform.position = _bodiesManager.human.body.Joints[BodyJointType.rightHip];
                    GameObject.Find("LEFTKNEE").transform.position = _bodiesManager.human.body.Joints[BodyJointType.leftKnee];
                    GameObject.Find("RIGHTKNEE").transform.position = _bodiesManager.human.body.Joints[BodyJointType.rightKnee];
                    GameObject.Find("LEFTFOOT").transform.position = _bodiesManager.human.body.Joints[BodyJointType.leftFoot];
                    GameObject.Find("RIGHTFOOT").transform.position = _bodiesManager.human.body.Joints[BodyJointType.rightFoot];



                }
            }
        }
    }  
}
