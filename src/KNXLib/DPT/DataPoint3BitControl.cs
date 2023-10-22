using System.Globalization;
using KNXLib.Log;

namespace KNXLib.DPT;

internal sealed class DataPoint3BitControl : DataPoint
{
    public override string[] Ids { get; } = {"3.008", "3.007"};

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

        var input = data[0] & 0x0F;

        var direction = (input >> 3) == 1;
        var step = input & 0x07;

        if (step == 0) return step;

        step = -step + 8;

        return direction ? step : -step;
    }

    public override byte[] ToDataPoint(string value)
    {
        return ToDataPoint(float.Parse(value, CultureInfo.InvariantCulture));
    }

    public override byte[] ToDataPoint(object val)
    {
        var dataPoint = new byte[1];
        dataPoint[0] = 0x00;

        int input;
        switch (val)
        {
            case int i:
                input = i;
                break;
            case float f:
                input = (int) f;
                break;
            case long l:
                input = (int) l;
                break;
            case double d:
                input = (int) d;
                break;
            case decimal val1:
                input = (int) val1;
                break;
            default:
                Logger.Error("6.xxx", "input value received is not a valid type");
                return dataPoint;
        }

        if (input > 7 || input < -7)
        {
            Logger.Error("3.xxx", "input value received is not in a valid range");
            return dataPoint;
        }

        var direction = 0; // binary 1000


        if (input > 0)
        {
            direction = 8;
            input = -input;
        }

        input += 8;

        var step = input & 7;

        dataPoint[0] = (byte) (step | direction);

        return dataPoint;
    }
}