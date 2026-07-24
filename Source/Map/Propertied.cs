using System;
using System.Collections.Generic;
using Geranium.Reflection;

namespace ioi.Tiled.Map
{
    public class Propertied : IDisposable
    {
        public Dictionary<string, string> Properties { get; set; } = new();
        
        private Dictionary<string, object> propertyValueCache = new();

        public T GetPropertyValue<T>(string propName)
        {
            T value = default;

            if (propertyValueCache.ContainsKey(propName))
                return propertyValueCache[propName].As<T>();

            if (Properties.ContainsKey(propName))
            {
                var p = Properties[propName];
                if (typeof(T).IsEnum)
                {
                    value = Enum.Parse(typeof(T), p, true).As<T>();
                }
                else if (typeof(T).Is<Guid>())
                {
                    value = Guid.Parse(p).As<T>();
                }
                else
                {
                    value = Convert.ChangeType(p, typeof(T)).As<T>();
                }
            }

            propertyValueCache[propName] = value;

            return value;
        }
        
        public void MergeProperties(Propertied other)
        {
            foreach (var prop in other.Properties)
            {
                Properties[prop.Key] = prop.Value;
            }

            propertyValueCache.Clear();
        }

        public virtual void Dispose()
        {
            if (Properties != null)
                Properties.Clear();
            Properties = null;

            if (propertyValueCache != null)
                propertyValueCache.Clear();
            propertyValueCache = null;
        }
    }
}
