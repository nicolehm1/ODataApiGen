using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class AssociationSetEnd
    {
        public AssociationSet AssociationSet {get; private set;}
        public AssociationSetEnd(XElement xElement, AssociationSet set)
        {
            AssociationSet = set;
            Role = xElement.Attribute("Role").Value;
            EntitySet = xElement.Attribute("EntitySet").Value;
        }
        public string Role { get; set; }
        public string EntitySet { get; set; }
    }
}