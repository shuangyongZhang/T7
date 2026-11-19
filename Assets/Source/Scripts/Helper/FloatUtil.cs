using System;
using UnityEngine;

internal class FloatUtil
{
    public static bool Equals(float value1, float value2)
    {
        return (value1 == value2)
                 || Mathf.Abs(value1 - value2) < Mathf.Epsilon;
    }

    public static bool GreaterThan(float value1, float value2)
    {
        return ((value1 > value2) && !Equals(value1, value2));
    }

    public static bool GreaterThanOrEqual(float value1, float value2)
    {
        return (value1 > value2 || Equals(value1, value2));
    }

    public static bool IsZero(float value)
    {
        return (Mathf.Abs(value) < Mathf.Epsilon);
    }

    public static bool LessThan(float value1, float value2)
    {
        return ((value1 < value2) && !Equals(value1, value2));
    }

    public static bool LessThanOrEqual(float value1, float value2)
    {
        return (value1 < value2) || Equals(value1, value2);
    }
}
