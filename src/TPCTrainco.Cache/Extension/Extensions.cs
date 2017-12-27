using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TPCTrainco.Cache.Extension
{
    public static class Extensions
    {
        public static TObject ToObject<TObject>(this IDictionary<string, object> someSource, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
           where TObject : class, new()
        {
            Contract.Requires(someSource != null);
            TObject targetObject = new TObject();
            Type targetObjectType = typeof(TObject);

            // Go through all bound target object type properties...
            foreach (PropertyInfo property in
                        targetObjectType.GetProperties(bindingFlags))
            {
                Type propertyType = property.PropertyType;
                if (someSource.ContainsKey(property.Name)
                    && propertyType == someSource[property.Name].GetType())
                {
                    if (propertyType != DateTime.UtcNow.GetType())
                        property.SetValue(targetObject, someSource[property.Name]);
                    else
                        property.SetValue(targetObject, ((DateTime)someSource[property.Name]).ToUniversalTime());
                }
            }

            return targetObject;
        }

        public static bool IsPropertyACollection(this PropertyInfo property)
        {
            return property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null;
        } 
    }
}