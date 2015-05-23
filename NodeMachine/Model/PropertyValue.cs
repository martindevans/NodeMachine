
namespace NodeMachine.Model
{
    public struct PropertyValue
    {
        public string Key { get; set; }

        public object Value { get; set; }

        public PropertyValue(string key, object value)
            : this()
        {
            Key = key;
            Value = value;
        }
    }
}
