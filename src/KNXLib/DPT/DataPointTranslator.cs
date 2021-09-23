using System;
using System.Collections.Generic;
using System.Linq;

namespace KNXLib.DPT
{
    internal sealed class DataPointTranslator
    {
        public static readonly DataPointTranslator Instance = new DataPointTranslator();
        private readonly IDictionary<string, IDataPoint> _dataPoints = new Dictionary<string, IDataPoint>();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DataPointTranslator()
        {
        }

        private DataPointTranslator()
        {
            Type type = typeof(IDataPoint);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => type.IsAssignableFrom(p) && p != type);

            foreach (Type t in types)
            {
                IDataPoint dp = (IDataPoint)Activator.CreateInstance(t);

                foreach (string id in dp.Ids)
                {
                    _dataPoints.Add(id, dp);
                }
            }
        }

        public object FromDataPoint(string type, string data)
        {
            try
            {
                IDataPoint dpt;
                if (_dataPoints.TryGetValue(type, out dpt))
                    return dpt.FromDataPoint(data);
            }
            catch
            {
            }

            return null;
        }

        public object FromDataPoint(string type, byte[] data)
        {
            try
            {
                IDataPoint dpt;
                if (_dataPoints.TryGetValue(type, out dpt))
                    return dpt.FromDataPoint(data);
            }
            catch
            {
            }

            return null;
        }

        public byte[] ToDataPoint(string type, string value)
        {
            try
            {
                IDataPoint dpt;
                if (_dataPoints.TryGetValue(type, out dpt))
                    return dpt.ToDataPoint(value);
            }
            catch
            {
            }

            return null;
        }

        public byte[] ToDataPoint(string type, object value)
        {
            try
            {
                IDataPoint dpt;
                if (_dataPoints.TryGetValue(type, out dpt))
                    return dpt.ToDataPoint(value);
            }
            catch
            {
            }

            return null;
        }

        public string UnitFromDataPoint(string type)
        {
            try
            {
                IDataPoint dpt;
                if (this._dataPoints.TryGetValue(type, out dpt))
                    return dpt.Unit(type);
            }
            catch {}

            return null;
        }
    }
}
