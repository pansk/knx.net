using System;
using System.Globalization;
using KNXLib.Log;

namespace KNXLib.DPT
{
    internal sealed class DataPoint2ByteFloatNoSign : DataPoint
    {
        public override string[] Ids
        {
            get
            {
                return new[]
                {
                    "9.004",
                    "9.005",
                    "9.006",
                    "9.007",
                    "9.008",
                    "9.028"
                };
            }
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
            // DPT bits high byte: MEEEEMMM, low byte: MMMMMMMM
            // first M is signed state from two's complement notation

            int val = 0;
            uint m = (uint) ((data[0] & 0x07) << 8) | (data[1]);
            bool signed = ((data[0] & 0x80) >> 7) == 0;


            val = (int) m;

            int power = (data[0] & 0x78) >> 3;

            double calc = 0.01d * val;

            return (decimal) Math.Round(calc * Math.Pow(2, power), 2);
        }

        public override byte[] ToDataPoint(string value)
        {
            return ToDataPoint(float.Parse(value, CultureInfo.InvariantCulture));
        }

        public override byte[] ToDataPoint(object val)
        {
            var dataPoint = new byte[] {0x00, 0x00, 0x00};

            decimal value;
            if (val is int)
                value = (int) val;
            else if (val is float)
                value = (decimal) ((float) val);
            else if (val is long)
                value = (long) val;
            else if (val is double)
                value = (decimal) ((double) val);
            else if (val is decimal)
                value = (decimal) val;
            else
            {
                Logger.Error("9.xxx", "input value received is not a valid type");
                return dataPoint;
            }

            if (value < 0 || value > +670760)
            {
                Logger.Error("9.xxx", "input value received is not in a valid range");
                return dataPoint;
            }

            // value will be multiplied by 0.01
            decimal v = Math.Round(value * 100m);
            // mantissa only holds 11 bits for value, so, check if exponet is required
            int e = 0;
            while (v < -2048m)
            {
                v = v / 2;
                e++;
            }

            while (v > 2047m)
            {
                v = v / 2;
                e++;
            }

            int mantissa;

            mantissa = (int) v;


            dataPoint[1] = ((byte) (dataPoint[1] | ((e & 0x0F) << 3)));
            dataPoint[1] = ((byte) (dataPoint[1] | ((mantissa >> 8) & 0x07)));
            dataPoint[2] = ((byte) mantissa);

            return dataPoint;
        }





        public override string Unit(string type)
        {
            switch (type)
            {
                case "9.004":
                    return "Lux";
                case "9.005":
                    return "m/s";
                case "9.006":
                    return "Pa";
                case "9.007":
                    return "%";
                case "9.008":
                    return "ppm";
                case "9.028":
                    return "km/h";
            }

            return "";
        }
    }
}