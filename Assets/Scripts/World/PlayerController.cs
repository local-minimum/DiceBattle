using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DeCrawl.Utils;

public enum MoveAction { None, Forward, Left, Right, Back, RotateClockWise, RotateCounterClockWise }

public class PlayerController : DeCrawl.Primitives.FindingSingleton<PlayerController>
{
    [SerializeField]
    LayerMask traversibilityMask;

    [SerializeField, Range(0, 3)]
    float playerHeight = 1.7f;

    [SerializeField, WithAction("SnapToFloorGrid", "Snap", "Snap position to ground")]
    bool snap;

    CardinalDirection _direction = CardinalDirection.None;
    CardinalDirection Direction { 
        get => _direction; 
        set { 
            if (_direction != value)
            {
                _direction = value;

                if (value != CardinalDirection.None) Orient();
            }
        } 
    }

    private void Start()
    {
        Direction = CardinalDirection.North;
    }

    void Orient()
    {
        switch (Direction)
        {
            case CardinalDirection.North:
                transform.rotation = Quaternion.identity;
                break;
            case CardinalDirection.West:
                transform.rotation = Quaternion.Euler(new Vector3(0, -90));
                break;
            case CardinalDirection.East:
                transform.rotation = Quaternion.Euler(new Vector3(0, +90));
                break;
            case CardinalDirection.South:
                transform.rotation = Quaternion.Euler(new Vector3(0, -180));
                break;
        }
    }
    public void SnapToFloorGrid()
    {
        transform.position = GameSettings.Grid.GridGroundCenter(transform.position);
    }

    MoveAction _moveAction = MoveAction.None;
    MoveAction MoveAction { 
        get => _moveAction;
        set { 
            if (_moveAction != value) { 
                _moveAction = value;
                QueueAction();
            } 
        } 
    }

    void ClearAction(MoveAction action)
    {
        if (MoveAction == action)
        {
            Debug.Log($"Released {action}");
            _moveAction = MoveAction.None;
        }
    }

    void HandleAction(InputAction.CallbackContext value, MoveAction action)
    {
        if (value.control.IsPressed())
        {
            MoveAction = action;
        } else if (!value.control.IsPressed())
        {
            ClearAction(action);
        }
    }

    public void OnForward(InputAction.CallbackContext value) => HandleAction(value, MoveAction.Forward);
    public void OnBack(InputAction.CallbackContext value) => HandleAction(value, MoveAction.Back);
    public void OnLeft(InputAction.CallbackContext value) => HandleAction(value, MoveAction.Left);
    public void OnRight(InputAction.CallbackContext value) => HandleAction(value, MoveAction.Right);
    public void OnRotateClockWise(InputAction.CallbackContext value) => HandleAction(value, MoveAction.RotateClockWise);
    public void OnRotateCounterClockWise(InputAction.CallbackContext value) => HandleAction(value, MoveAction.RotateCounterClockWise);

    List<MoveAction> ActionQueue = new List<MoveAction>(2) { MoveAction.None, MoveAction.None};

    void QueueAction()
    {
        if (ActionQueue[0] == MoveAction.None)
        {
            ActionQueue[0] = MoveAction;
        } else
        {
            ActionQueue[1] = MoveAction;
        }
    }

    bool PopActionFromQueue(out MoveAction action) {
        action = ActionQueue[0];
        if (action == MoveAction.None)
        {
            return false;
        }
        ActionQueue[0] = ActionQueue[1];
        ActionQueue[1] = MoveAction.None;

        return true;
    }

    [SerializeField, Range(0, 2)]
    float actionSamplingFrequency = 0.8f;
    float nextAction = 0;

    static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    bool DoAction(MoveAction action) 
    {
        switch (action)
        {
            case MoveAction.Left:
            case MoveAction.Right:
            case MoveAction.Back:
            case MoveAction.Forward:
                var direction = Direction.ByMovement(action);
                if (CanMove(direction))
                {
                    transform.position += Scale(direction.AsVector(), GameSettings.Grid.GridSize);
                    SnapToFloorGrid();
                }
                break;
            case MoveAction.RotateClockWise:
                Direction = Direction.RotateCW();
                break;
            case MoveAction.RotateCounterClockWise:
                Direction = Direction.RotateCCW();
                break;
            case MoveAction.None:
                return false;

        }
        return true;
    }

    bool DoAction() => DoAction(MoveAction);

    void WaitForNextTick()
    {
        nextAction = Time.timeSinceLevelLoad + actionSamplingFrequency;
    }

    [SerializeField, Range(4, 20)]
    int horizontalRays = 12;

    [SerializeField, Range(0, 1)]
    float rayOvershootNeeded = 0.5f;

    [SerializeField, Range(1, 10)]
    int neededRaysToPass = 7;

    [SerializeField, Range(0, 90)]
    float maxWalkInclination = 45f;

    [SerializeField, Range(0, 90)]
    float minClimbInclination = 80f;

    [SerializeField]
    float maxClimbHeight = 1f;

    [SerializeField, Range(-90, 0)]
    float maxJumpInclination = -75f;

    [SerializeField]
    float maxJumpHeight = 5f;

    Vector3 Feet => transform.position - Vector3.up * GameSettings.Grid.HalfHeight;
    Vector3 Head => Feet + Vector3.up * playerHeight;

    IEnumerable<Ray> TestTraversibilityRays(CardinalDirection direction)
    {
        var directionVector = direction.AsVector();
        var feet = Feet;
        var head = Head;

        for (int i = 0; i<horizontalRays; i++)
        {
            yield return new Ray(Vector3.Lerp(feet, head, i / (float)horizontalRays), directionVector);
        }
    }

    IEnumerable<Ray> TestTraversibilityRays(Vector3 directionVector)
    {
        var feet = Feet;
        var head = Head;

        for (int i = 0; i<horizontalRays; i++)
        {
            yield return new Ray(Vector3.Lerp(feet, head, i / (float)horizontalRays), directionVector);
        }
    }

    private struct TraversibilityInfo
    {
        public readonly bool Passable;
        public readonly Vector3 RayOrigin;
        public readonly Vector3 RayTerminus;
        public readonly float Distance;

        public TraversibilityInfo(bool passable, Vector3 rayOrigin, Vector3 rayTerminus, float distance)
        {
            Passable = passable;
            RayOrigin = rayOrigin;
            RayTerminus = rayTerminus;
            Distance = distance;
        }
    }

    IEnumerable<TraversibilityInfo> TestTraversibility(CardinalDirection direction)
    {
        var directionVector = direction.AsVector();
        var gridSizeAlongDirection = Vector3.Project(GameSettings.Grid.GridSize, directionVector).magnitude;
        var targetGridCenter = GameSettings.Grid.GridCenter(transform.position + directionVector * gridSizeAlongDirection, out var height);

        var fullDistance = gridSizeAlongDirection + rayOvershootNeeded;
        foreach (var ray in TestTraversibilityRays(targetGridCenter - transform.position))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, fullDistance, traversibilityMask))
            {
                yield return new TraversibilityInfo(false, ray.origin, hit.point, hit.distance);
            } else
            {
                yield return new TraversibilityInfo(true, ray.origin, ray.GetPoint(gridSizeAlongDirection), fullDistance);
            }
        }
    }

    public bool CanMove(CardinalDirection direction)
    {
        int passers = 0;
        var currentDistance = 0;
        var infos = new List<TraversibilityInfo>();
        infos.Add(new TraversibilityInfo(true, Feet, Feet, 0));

        foreach (var info in TestTraversibility(direction))
        {
            // TODO: Should interpolate down on previous vector and update it
            // to make vertical step.
            if (info.Distance < currentDistance)
            {
                int infoCount = infos.Count;
                int truncateStart = infoCount - 1;
                while (truncateStart > 1)
                {
                    if (infos[truncateStart].Distance <= info.Distance)
                    {
                        truncateStart++;
                        break;
                    }
                    truncateStart--;
                }
                infos.RemoveRange(truncateStart, infoCount - truncateStart);
            }

            // TODO: After vertical step really need to realign and do new traveseribility test with
            // new ray based on current elevation

            // TODO: Should compare check walks, jumps and falls against heights and inclinations
            var segmentVector = info.RayTerminus - infos[infos.Count - 1].RayTerminus;
            var inclination = segmentVector.Inclination() * Mathf.Rad2Deg;
            Debug.Log(inclination);
            if (info.Passable)
            {
                passers += 1;
                if (passers >= neededRaysToPass) return true;
            } else
            {
                passers = 0;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (Direction == CardinalDirection.None) return;

        foreach (var info in TestTraversibility(Direction))
        {
            Gizmos.color = info.Passable ? Color.green : Color.red;
            Gizmos.DrawLine(info.RayOrigin, info.RayTerminus);
        }
    }

    private void Update()
    {
        if (Time.timeSinceLevelLoad > nextAction)
        {
            if (PopActionFromQueue(out MoveAction action))
            {
                if (DoAction(action))
                {
                    WaitForNextTick();
                }
            } else if (DoAction())
            {
                WaitForNextTick();
            }
        }
    }
}
