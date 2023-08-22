using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardinalDirection { None, North, West, South, East }
public static class DirectionExtensions
{
    public static Vector3 AsVector(this CardinalDirection faceDirection, MoveAction moveAction)
    {
        switch (moveAction)
        {

            case MoveAction.Forward: 
                return faceDirection.AsVector();
            case MoveAction.Back: 
                return faceDirection.Invert().AsVector();
            case MoveAction.Left:
                return faceDirection.RotateCCW().AsVector();
            case MoveAction.Right:
                return faceDirection.RotateCW().AsVector();
        }
        return Vector3.zero;

    }

    public static Vector3 AsVector(this CardinalDirection faceDirection)
    {
        switch (faceDirection)
        {
            case CardinalDirection.North:
                return Vector3.forward;
            case CardinalDirection.South:
                return Vector3.back;
            case CardinalDirection.West:
                return Vector3.left;
            case CardinalDirection.East:
                return Vector3.right;
            default:
                throw new System.ArgumentException($"{faceDirection} is not a known as a vector");
        }
    }
    public static CardinalDirection Invert(this CardinalDirection faceDirection)
    {
        switch (faceDirection)
        {
            case CardinalDirection.North:
                return CardinalDirection.South;
            case CardinalDirection.South:
                return CardinalDirection.North;
            case CardinalDirection.West:
                return CardinalDirection.East;
            case CardinalDirection.East:
                return CardinalDirection.West;
            case CardinalDirection.None:
                return CardinalDirection.None;
            default:
                throw new System.ArgumentException($"{faceDirection} has no inverse");
        }
    }


    public static CardinalDirection RotateCW(this CardinalDirection faceDirection)
    {
        switch (faceDirection)
        {
            case CardinalDirection.North:
                return CardinalDirection.East;
            case CardinalDirection.East:
                return CardinalDirection.South;
            case CardinalDirection.South:
                return CardinalDirection.West;
            case CardinalDirection.West:
                return CardinalDirection.North;
            case CardinalDirection.None:
                return CardinalDirection.None;
            default:
                throw new System.ArgumentException($"{faceDirection} can't be rotated");
        }
    }

    public static CardinalDirection RotateCCW(this CardinalDirection faceDirection)
    {
        switch (faceDirection)
        {
            case CardinalDirection.North:
                return CardinalDirection.West;
            case CardinalDirection.West:
                return CardinalDirection.South;
            case CardinalDirection.South:
                return CardinalDirection.East;
            case CardinalDirection.East:
                return CardinalDirection.North;
            case CardinalDirection.None:
                return CardinalDirection.None;
            default:
                throw new System.ArgumentException($"{faceDirection} can't be rotated");
        }
    }

}
