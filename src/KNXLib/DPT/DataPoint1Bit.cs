using System;
using System.Globalization;





namespace KNXLib.DPT
{
    internal class DataPoint1Bit : DataPoint
    {
        public override string[] Ids
        {
            get
            {
                return new[]
                {
                    "1.001",
                    "1.002",
                    "1.003",
                    "1.004",
                    "1.005",
                    "1.006",
                    "1.007",
                    "1.008",
                    "1.009",
                    "1.010",
                    "1.011",
                    "1.012",
                    "1.013",
                    "1.014",
                    "1.015",
                    "1.016",
                    "1.017",
                    "1.018",
                    "1.019",
                    "1.021",
                    "1.022",
                    "1.023",
                    "1.100"
                };
            }
        }





        public override object FromDataPoint(string data)
        {
            var dataConverted = new byte[data.Length];
            for (var i = 0; i < data.Length; i++) dataConverted[i] = (byte) data[i];

            return this.FromDataPoint(dataConverted);
        }





        public override object FromDataPoint(byte[] data)
        {
            // only byte[0] to care about
            return Convert.ToBoolean(data[0]);
        }





        public override byte[] ToDataPoint(string value)
        {
            return ToDataPoint(float.Parse(value, CultureInfo.InvariantCulture));
        }





        public override byte[] ToDataPoint(object val)
        {
            if (Convert.ToBoolean(val))
            {
                return new byte[]
                {
                    0x01
                };
            }

            return new byte[]
            {
                0x00
            };
        }





        public override string Unit(string type)
        {
            switch (type)
            {

                case "1.001":
                    return "";
                case "1.002":
                    return "";
                case "1.003":
                    return "";
                case "1.004":
                    return "";
                case "1.005":
                    return "";
                case "1.006":
                    return "";
                case "1.007":
                    return "";
                case "1.008":
                    return "";
                case "1.009":
                    return "";
                case "1.010":
                    return "";
                case "1.011":
                    return "";
                case "1.012":
                    return "";
                case "1.013":
                    return "";
                case "1.014":
                    return "";
                case "1.015":
                    return "";
                case "1.016":
                    return "";
                case "1.017":
                    return "";
                case "1.018":
                    return "";
                case "1.019":
                    return "";
                case "1.021":
                    return "";
                case "1.022":
                    return "";
                case "1.023":
                    return "";
                case "1.100":
                    return "";
            }

            return "";
        }
    }
}