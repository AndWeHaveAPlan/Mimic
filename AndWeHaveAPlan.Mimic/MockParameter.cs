using System;

namespace AndWeHaveAPlan.Mimic
{
    public class MockParameter
    {
        public MockParameter(string name, Type type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public readonly string Name;
        public readonly Type Type;
        public readonly object Value;
    }
}