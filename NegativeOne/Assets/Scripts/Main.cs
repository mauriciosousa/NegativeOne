using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable()]
public class NegativeRuntimeException : System.Exception
{
    public NegativeRuntimeException() : base() { }
    public NegativeRuntimeException(string message) : base(message) { }
    public NegativeRuntimeException(string message, System.Exception inner) : base(message, inner) { }
    protected NegativeRuntimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
}

public class Main : MonoBehaviour {

    public Location location;

    public Properties properties;

    private SurfaceRequestListener _surfaceRequestListener;
    private UdpBodiesListener _udpBodiesListener;
    private BodiesManager _bodies;

    private GameObject _localOrigin = null;
    public GameObject LocalOrigin { get { return _localOrigin; } }

    private GameObject _remoteOrigin = null;
    public GameObject RemoteOrigin { get { return _remoteOrigin; } }

    private SurfaceRectangle _localSurface = null;
    private SurfaceRectangle _remoteSurface = null;

    public bool __localSurfaceReceived = false;
    public bool __remoteSurfaceReceived = false;
    private bool __everythingIsNiceAndWellConfigured = false;

    private NegativeSpace _negativeSpace;
    private PerspectiveProjection _projection;

    void Awake()
    {
        Application.runInBackground = true;

        properties = GetComponent<Properties>();
        try
        {
            properties.load(location);
        }
        catch (NegativeRuntimeException e)
        {
            Debug.LogException(e);
            strategicExit();
        }

        _negativeSpace = GetComponent<NegativeSpace>();
        _projection = Camera.main.GetComponent<PerspectiveProjection>();
        _surfaceRequestListener = GetComponent<SurfaceRequestListener>();
        _surfaceRequestListener.StartReceive();
        GetComponent<SurfaceRequest>().request();

        _udpBodiesListener = GameObject.Find("BodiesManager").GetComponent<UdpBodiesListener>();
        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _udpBodiesListener.startListening(int.Parse(properties.localSetupInfo.trackerBroadcastPort));

    }

	void Start ()
    {
    }

    void Update ()
    {

        if (__localSurfaceReceived && __remoteSurfaceReceived)
        {
            if (!__everythingIsNiceAndWellConfigured)
            {
                Debug.Log("XXX  " + this.ToString() + ": Creating the negative world!!!!! XXX");

                GameObject localOrigin = new GameObject("LocalOrigin");
                localOrigin.transform.rotation = Quaternion.identity;
                localOrigin.transform.position = Vector3.zero;

                GameObject remoteOrigin = new GameObject("RemoteOrigin");
                remoteOrigin.transform.rotation = Quaternion.identity;
                remoteOrigin.transform.position = Vector3.zero;

                GameObject localScreenCenter = new GameObject("localScreenCenter");
                localScreenCenter.transform.position = _localSurface.Center;
                localScreenCenter.transform.rotation = _localSurface.Perpendicular;

                Vector3 BLp = _calculateRemoteProxy(_localSurface.SurfaceBottomLeft, localScreenCenter, properties.negativeSpaceLength);
                Vector3 BRp = _calculateRemoteProxy(_localSurface.SurfaceBottomRight, localScreenCenter, properties.negativeSpaceLength);
                Vector3 TRp = _calculateRemoteProxy(_localSurface.SurfaceTopRight, localScreenCenter, properties.negativeSpaceLength);
                Vector3 TLp = _calculateRemoteProxy(_localSurface.SurfaceTopLeft, localScreenCenter, properties.negativeSpaceLength);

                SurfaceRectangle remoteSurfaceProxy = new SurfaceRectangle(BLp, BRp, TLp, TRp);

                GameObject remoteScreenCenter = new GameObject("remoteScreenCenter");
                remoteScreenCenter.transform.position = _remoteSurface.Center;
                remoteScreenCenter.transform.rotation = _remoteSurface.Perpendicular;

                localOrigin.transform.parent = localScreenCenter.transform;
                remoteOrigin.transform.parent = remoteScreenCenter.transform;
                remoteScreenCenter.transform.position = localScreenCenter.transform.position;
                remoteScreenCenter.transform.rotation = Quaternion.LookRotation(-localScreenCenter.transform.forward, localScreenCenter.transform.up);

                remoteScreenCenter.transform.position = remoteSurfaceProxy.Center;

                _localSurface.CenterGameObject = localScreenCenter;
                _remoteSurface.CenterGameObject = remoteScreenCenter;

                _localOrigin = localOrigin;
                _remoteOrigin = remoteOrigin;

                _negativeSpace.create(location, _localSurface, remoteSurfaceProxy, properties.negativeSpaceLength);

                _projection.init(_localSurface);

                __everythingIsNiceAndWellConfigured = true;
            }
        }
	}

    private static void strategicExit()
    {
        if (Application.isEditor)
        {
            Debug.Break();
        }
    }

    public void setLocalSurface(SurfaceRectangle surfaceRectangle)
    {
        Debug.Log("Received Local Surface: " + surfaceRectangle.ToString());
        __localSurfaceReceived = true;
        _localSurface = surfaceRectangle;
    }

    public void setRemoteSurface(SurfaceRectangle surfaceRectangle)
    {
        Debug.Log("Received Remote Surface: " + surfaceRectangle.ToString());
        __remoteSurfaceReceived = true;
        _remoteSurface = surfaceRectangle;
    }

    private Vector3 _calculateRemoteProxy(Vector3 point, GameObject localScreenCenter, float negativeSpaceLength)
    {
        return point + negativeSpaceLength * localScreenCenter.transform.forward;
    }
}
