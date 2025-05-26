using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Surfaces;
using Oculus.Interaction;

[RequireComponent(typeof(RayInteractor))]
[RequireComponent(typeof(LineRenderer))]
public class RayPathDrawer : MonoBehaviour
{
    [Header("Which OVR Controller is this?")]
    public OVRInput.Controller ovrController = OVRInput.Controller.LTouch;

    [Header("Drawing Settings")]
    [Tooltip("Min distance (m) between sampled points")]
    public float pointSpacing = 0.02f;
    [Tooltip("Max raycast distance (m)")]
    public float maxRayDistance = 5f;
    [Tooltip("Layer mask for your table mesh")]
    public LayerMask tableMask;

    private RayInteractor _rayInteractor;
    private LineRenderer   _lineRenderer;
    private List<Vector3>  _points = new List<Vector3>();
    private bool           _isDrawing;
    private Vector3        _lastPoint;

    void Awake()
    {
        _rayInteractor  = GetComponent<RayInteractor>();
        _lineRenderer   = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = 0;
    }

    void Update()
    {
        // 1) Read the physical trigger
        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, ovrController);

        // 2) Toggle drawing state
        if (!_isDrawing && trigger > 0.5f) BeginDraw();
        if (_isDrawing  && trigger <= 0.5f) EndDraw();

        // 3) While drawing, sample the surface under your ray
        if (_isDrawing && TrySamplePoint(out Vector3 pt))
        {
            if (_points.Count == 0 || Vector3.Distance(_lastPoint, pt) > pointSpacing)
            {
                _points.Add(pt);
                _lastPoint = pt;
                RefreshLine();
            }
        }
    }

    void BeginDraw()
    {
        _isDrawing = true;
        _points.Clear();
        _lineRenderer.positionCount = 0;
    }

    void EndDraw()
    {
        _isDrawing = false;
        // Optional: smooth _points with Chaikin here
    }

    bool TrySamplePoint(out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;

        // Build a physics ray from the interactor’s Origin & Forward
        var origin  = _rayInteractor.Origin;
        var dir     = _rayInteractor.Forward;
        var ray     = new Ray(origin, dir);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tableMask))
            return false;

        // If it’s a RayInteractable surface, refine with its Surface.Raycast
        var ri = hit.collider.GetComponent<RayInteractable>();
        if (ri != null)
        {
            if (ri.Raycast(ray, out SurfaceHit surfHit, maxRayDistance, false))
            {
                worldPoint = surfHit.Point;
                return true;
            }
        }
        else
        {
            // fallback to raw mesh hit
            worldPoint = hit.point;
            return true;
        }

        return false;
    }

    void RefreshLine()
    {
        _lineRenderer.positionCount = _points.Count;
        _lineRenderer.SetPositions(_points.ToArray());
    }
}
