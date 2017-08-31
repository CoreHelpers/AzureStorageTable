using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public static class AssemblyExtensions
    {
        static internal IEnumerable<Type> GetTypesWithAttribute(this Assembly assembly, Type attributeType)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetTypeInfo().GetCustomAttribute(attributeType);
                if (attribute != null)
                    yield return type;
            }
        }
    }
}
