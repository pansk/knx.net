using System.Globalization;
using KNXLib.Log;

namespace KNXLib.DPT;

internal sealed class DataPoint8BitNoSignScaledScaling : DataPoint
{
    public override string[] Ids
    {
        get { return new[] { "5.001" }; }
    }

    public override object FromDataPoint(string data)
    {
        var dataConverted = new byte[data.Length];
        for (var i = 0; i < data.Length; i++)
            dataConverted[i] = (byte) data[i];

        return FromDataPoint(dataConverted);
    }

    public override object FromDataPoint(byte[] data)
    {
        if (data == null)
            return 0;
        if (data.Length == 2)
            data = data.Skip(1).ToArray();
        else if (data.Length != 1)
            return 0;

        var value = (int)data[0];

        return (decimal)value * 100 / 255;
    }

    public override byte[] ToDataPoint(string value)
    {
        return ToDataPoint(float.Parse(value, CultureInfo.InvariantCulture));
    }

    public override byte[] ToDataPoint(object val)
    {
        var dataPoint = new byte[2];
        dataPoint[0] = 0x00;
        dataPoint[1] = 0x00;

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
                Logger.Error("5.001", "input value received is not a valid type");
                return dataPoint;
        }

        if (input < 0 || input > 100)
        {
            Logger.Error("5.001", "input value received is not in a valid range");
            return dataPoint;
        }

        dataPoint[1] = (byte) (input * 255 / 100);

        return dataPoint;
    }
}