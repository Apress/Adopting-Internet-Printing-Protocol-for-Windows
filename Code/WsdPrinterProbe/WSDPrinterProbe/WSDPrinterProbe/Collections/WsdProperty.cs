

namespace WSDPrinterProbe
{
    public class WsdProperty
    {
        public WsdProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
