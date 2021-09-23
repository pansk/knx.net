using System.Globalization;
using KNXLib.Log;

namespace KNXLib.DPT
{
    internal sealed class DataPoint8BitSignRelativeValue : IDataPoint
    {
        public string[] Ids
        {
            get { return new[] { "6.001", "6.010" }; }
        }

        public object FromDataPoint(string data)
        {
            var dataConverted = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
                dataConverted[i] = (byte) data[i];

            return FromDataPoint(dataConverted);
        }

        public object FromDataPoint(byte[] data)
        {
            if (data == null || data.Length != 1)
                return 0;

            return (int) ((sbyte) data[0]);
        }

        public byte[] ToDataPoint(string value)
        {
            return ToDataPoint(float.Parse(value, CultureInfo.InvariantCulture));
        }

        public byte[] ToDataPoint(object val)
        {
            var dataPoint = new byte[1];
            dataPoint[0] = 0x00;
            
            int input;
            if (val is int)
                input = ((int) val);
            else if (val is float)
                input = (int) ((float) val);
            else if (val is long)
                input = (int) ((long) val);
            else if (val is double)
                input = (int) ((double) val);
            else if (val is decimal)
                input = (int) ((decimal) val);
            else
            {
                Logger.Error("6.xxx", "input value received is not a valid type");
                return dataPoint;
            }
            
            if (input < -128 || input > 127)
            {
                Logger.Error("6.xxx", "input value received is not in a valid range");
                return dataPoint;
            }

            dataPoint[0] = (byte) ((sbyte) input);

            return dataPoint;
        }

        public string Unit(string type)
        {
            switch (type)
            {
                case "6.001":
                    return "%";
                case "6.010":
                    return "counter pulses";
            }
            return "";
        }
    }
}