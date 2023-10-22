namespace KNXLib.DPT;

internal sealed class DataPointTranslator
{
    public static readonly DataPointTranslator Instance = new();
    private readonly IDictionary<string, DataPoint> _dataPoints;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static DataPointTranslator()
    {
    }

    private DataPointTranslator()
    {
        var type = typeof(DataPoint);
        _dataPoints = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsSubclassOf(type)).Select(t => Activator.CreateInstance(t) as DataPoint)
            .SelectMany(dp =>
                dp?.Ids.Select(id => new KeyValuePair<string, DataPoint>(id, dp)) ??
                Array.Empty<KeyValuePair<string, DataPoint>>()).ToDictionary(t => t.Key, t => t.Value);

    }

    public object? FromDataPoint(string type, string data)
    {
        try
        {
            if (_dataPoints.TryGetValue(type, out var dpt))
                return dpt.FromDataPoint(data);
        }
        catch
        {
        }

        return null;
    }

    public object? FromDataPoint(string type, byte[] data)
    {
        try
        {
            if (_dataPoints.TryGetValue(type, out var dpt))
                return dpt.FromDataPoint(data);
        }
        catch
        {
        }

        return null;
    }

    public byte[]? ToDataPoint(string type, string value)
    {
        try
        {
            if (_dataPoints.TryGetValue(type, out var dpt))
                return dpt.ToDataPoint(value);
        }
        catch
        {
        }

        return null;
    }

    public byte[]? ToDataPoint(string type, object value)
    {
        try
        {
            if (_dataPoints.TryGetValue(type, out var dpt))
                return dpt.ToDataPoint(value);
        }
        catch
        {
        }

        return null;
    }
}