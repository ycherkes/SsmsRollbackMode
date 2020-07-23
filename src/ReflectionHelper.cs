using System;
using System.Linq;
using System.Reflection;

namespace SsmsRollbackMode
{
    internal static class ReflectionHelper
    {
        public static object GetPropertyValue(object obj, string name)
        {
            object propertyValue = null;
            try
            {
                var propertyInfo = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (propertyInfo != null)
                    propertyValue = propertyInfo.GetValue(obj);
            }
            catch (Exception)
            {
                // todo: logging
            }
            return propertyValue;
        }
    }
}
