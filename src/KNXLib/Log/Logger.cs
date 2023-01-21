namespace KNXLib.Log;

/// <summary>
/// </summary>
public class Logger
{
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public delegate void DebugEvent(string id, string message);

    /// <summary>
    /// </summary>
    public static event DebugEvent? DebugEventEndpoint;

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public delegate void InfoEvent(string id, string message);

    /// <summary>
    /// </summary>
    public static event InfoEvent? InfoEventEndpoint;

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public delegate void WarnEvent(string id, string message);

    /// <summary>
    /// </summary>
    public static event WarnEvent? WarnEventEndpoint;

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public delegate void ErrorEvent(string id, string message);

    /// <summary>
    /// </summary>
    public static event ErrorEvent? ErrorEventEndpoint;

    internal static void Debug(string id, string message, params object[] arg)
    {
        DebugEventEndpoint?.Invoke(id, string.Format(message, arg));
    }

    internal static void Info(string id, string message, params object[] arg)
    {
        InfoEventEndpoint?.Invoke(id, string.Format(message, arg));
    }

    internal static void Warn(string id, string message, params object[] arg)
    {
        WarnEventEndpoint?.Invoke(id, string.Format(message, arg));
    }

    internal static void Error(string id, string message, params object[] arg)
    {
        ErrorEventEndpoint?.Invoke(id, string.Format(message, arg));
    }

    internal static void Error(string id, Exception e)
    {
        Error(id, e.Message);
        Error(id, e.ToString());
        if (e.StackTrace != null)
        {
            Error(id, e.StackTrace);
        }

        if (e.InnerException != null)
            Error(id, e.InnerException);
    }
}