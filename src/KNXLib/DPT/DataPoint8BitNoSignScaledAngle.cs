using System.Globalization;
using KNXLib.Log;

namespace KNXLib.DPT;

internal sealed class DataPoint8BitNoSignScaledAngle : DataPoint
{
    public override string[] Ids { get; } = { "5.003" };

    public override object FromDataPoint(string data)
    {
        var dataConverted = new byte[data.Length];
        for (var i = 0; i < data.Length; i++)
            dataConverted[i] = (byte) data[i];

        return FromDataPoint(dataConverted);
    }

    public override object FromDataPoint(byte[] data)
    {
        if (data == null || data.Length != 1)
            return 0;

        var value = (int) data[0];

        return (decimal)value * 360 / 255;
    }

    public override byte[] ToDataPoint(string value)
    {
        return ToDataPoint(float.Parse(value, CultureInfo.InvariantCulture));
    }

    public override byte[] ToDataPoint(object val)
    {
        var dataPoint = new byte[1];
        dataPoint[0] = 0x00;
            
        decimal input;
        switch (val)
        {
            case int i:
                input = i;
                break;
            case float f:
                input = (decimal) f;
                break;
            case long l:
                input = l;
                break;
            case double d:
                input = (decimal) d;
                break;
            case decimal val1:
                input = val1;
                break;
            default:
                Logger.Error("5.003", "input value received is not a valid type");
                return dataPoint;
        }

        if (input < 0 || input > 360)
        {
            Logger.Error("5.003", "input value received is not in a valid range");
            return dataPoint;
        }

        dataPoint[0] = (byte) (int) (input * 255 / 360);

        return dataPoint;
    }
}