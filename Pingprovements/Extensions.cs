﻿using System.Reflection;

namespace Pingprovements
{
    public static class Extensions
    {
        public static void SetObjectValue(this object obj, string fieldName, object value)
        {
            obj.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(obj, value);
        }

        public static T GetObjectValue<T>(this object obj, string fieldName) =>
            (T) obj.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(obj);
    }
}