using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultDrawMode : DrawLineMode
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private LayerMask _ignoreDrawing;

    private List<Vector3> _pointsForMovement = new List<Vector3>();
    private List<Vector3> _points = new List<Vector3>();

    private Vector3 _previousFramePointForDraw = new Vector3();
    private Vector3 _previousFramePointForMove = new Vector3();
    private Vector3 _previousFrameMousePoint = new Vector3();
    private Vector3 _currentFrameMousePoint = new Vector3();
    private float _currentDistanceBetweenPointsForDraw;
    private float _currentDistanceBetweenPointsForMove;
    private float _minDistanceBetweenPointForDraw = 0.05f;
    private float _minDistanceBetweenPointForMove = 0.7f;
    private Vector3 _lastPointOnLine = new Vector3();

    public List<Vector3> Points => _points;
    public List<Vector3> PointsForMovement => _pointsForMovement;

    public event Action<Vector3> BarrierHasAppeared;

    public override void Draw(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, 100))
        {
            AddPointToDrawLine(hit.point);
            AddPointToMovementLine(hit.point);
            _previousFrameMousePoint = hit.point;
        }
    }

    public void AddPointToDrawLine(Vector3 newPoint)
    {
        newPoint.z = PositionLineZ;

        Vector3 startPoint = new Vector3();

        if (_points.Count == 0)
        {
            startPoint = newPoint;
        }

        else
        {
            startPoint = _points[_points.Count - 1];
        }

        startPoint.z = PositionLineZ;
        Debug.DrawLine(startPoint, newPoint, Color.green);

        _currentDistanceBetweenPointsForDraw = Vector3.Distance(newPoint, _previousFramePointForDraw);

        if (_currentDistanceBetweenPointsForDraw < _minDistanceBetweenPointForDraw)
        {
            return;
        }

        if (Physics.Linecast(startPoint, newPoint, out RaycastHit hitInfo, _ignoreDrawing))
        {
            BarrierHasAppeared?.Invoke(startPoint);
            return;
        }

        _points.Add(newPoint);
        _lineRenderer.positionCount = _points.Count;
        _lineRenderer.SetPositions(_points.ToArray());
        _previousFramePointForDraw = newPoint;
    }

    public void AddPointToMovementLine(Vector3 newPoint)
    {
        _currentDistanceBetweenPointsForMove = Vector3.Distance(newPoint, _previousFramePointForMove);

        if (_currentDistanceBetweenPointsForMove > _minDistanceBetweenPointForMove)
        {
            newPoint.z = PositionLineZ;
            _pointsForMovement.Add(newPoint);
            _previousFramePointForMove = new Vector3(newPoint.x, newPoint.y, -0.2f);
        }
    }

    public void SetColorToLine(Color colorLine)
    {
        _lineRenderer.material.color = colorLine;
    }
}
