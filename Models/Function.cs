using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class Function : Callable
    {
        public Function(XElement xElement, Schema schema) : base(xElement, schema)
        {
            Type = "Function";
        }
    }
}
