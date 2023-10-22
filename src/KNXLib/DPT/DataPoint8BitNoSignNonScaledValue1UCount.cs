﻿using System.Globalization;
using KNXLib.Log;

namespace KNXLib.DPT;

internal sealed class DataPoint8BitNoSignNonScaledValue1UCount : DataPoint
{
    public override string[] Ids { get; } = { "5.010" };

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

        return (int) data[0];
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
                Logger.Error("5.010", "input value received is not a valid type");
                return dataPoint;
        }

        if (input < 0 || input > 255)
        {
            Logger.Error("5.010", "input value received is not in a valid range");
            return dataPoint;
        }
            
        dataPoint[0] = (byte) input;

        return dataPoint;
    }
}