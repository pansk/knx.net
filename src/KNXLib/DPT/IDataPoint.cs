namespace KNXLib.DPT
{
    internal interface IDataPoint
    {
        string[] Ids { get; }

        object FromDataPoint(string data);

        object FromDataPoint(byte[] data);

        byte[] ToDataPoint(string value);

        byte[] ToDataPoint(object value);

        string Unit(string type);
    }
}
