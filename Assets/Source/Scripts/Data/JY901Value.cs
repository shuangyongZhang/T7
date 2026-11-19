using System.Linq;
using LB;

public class KeyResolveValue
{
    public string key;
    public double value;
    public bool isDegree;
    public double RawValue { get; private set; }
    public double LastRawValue { get; private set; }

    private bool _isInit;
    private double _targetValue;

    public KeyResolveValue(string _key, bool _isDegree)
    {
        key = _key;
        isDegree = _isDegree;
    }

    public void SetValue(double newVal, bool isReverse)
    {
        if (double.IsNaN(newVal) || double.IsInfinity(newVal))
            return;

        if (!_isInit)
        {
            LastRawValue = RawValue = newVal;
            _isInit = true;
            return;
        }

        LastRawValue = RawValue;
        RawValue = newVal;

        if (isDegree)
        {
            _targetValue = PreventValueSkipping(_targetValue, LastRawValue, RawValue, isReverse);
        }
        else
        {
            int sign = isReverse ? 1 : -1;
            float dist = (float)(RawValue - LastRawValue);
            _targetValue += dist * sign;
        }
        value = _targetValue;
    }

    public float GetValue()
    {
        return (float)value;
    }

    public void Recalibration()
    {
        _isInit = false;
        _targetValue = 0;
    }

    private double PreventValueSkipping(double current, double last, double now, bool r)
    {
        double sign = r ? 1 : -1;
        double delta = now - last;

        if (delta >= 180) delta -= 360;
        else if (delta <= -180) delta += 360;

        var value = current + delta * sign;
        if (value >= 360)
            value %= 360;
        else if (value <= -360)
            value %= -360;

        return value;
    }
}

public class JY901Value
{
    public KeyResolveValue[] values;
    public JY901Value()
    {
        values = new KeyResolveValue[3];
    }
    public float GetValue(int index)
    {
        if (!CheckIfPortValueExist(index))
            return 0f;

        return values[index].GetValue();
    }
    protected bool CheckIfPortValueExist(int index)
    {
        return values != null && index < values.Length;
    }
    public float GetXValue()
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].key == "x")
            {
                return (float)values[i].value;
            }
        }
        return 0;
    }
    public float GetYValue()
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].key == "y")
            {
                return (float)values[i].value;
            }
        }
        return 0;
    }
    public float GetZValue()
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].key == "z")
            {
                return (float)values[i].value;
            }
        }
        return 0;
    }
    public string GetValues()
    {
        return string.Join(",", values.Select(v => v.key + "=" + v.GetValue()));
    }
}