using System.Xml.Linq;
using DotLiquid;

namespace ODataApiGen.Models
{
    public class Singleton : Annotable, ILiquidizable
    {
        public EntityContainer EntityContainer {get; private set;}
        public Singleton(XElement xElement, EntityContainer container) : base(xElement)
        {
            EntityContainer = container;
            Name = xElement.Attribute("Name")?.Value;
            Type = xElement.Attribute("Type")?.Value;

            NavigationPropertyBindings = xElement.Descendants().Where(a => a.Name.LocalName == "NavigationPropertyBinding")
                .Select(navPropBind => new NavigationPropertyBinding(navPropBind, this)).ToList();
        }

        public void ImportActions(IEnumerable<ActionImport> actionImports, IEnumerable<Action> actions) {
            Actions = actionImports
                .Where(a => a.EntitySet == Name)
                .Select(ai => actions.FirstOrDefault(a => a.NamespaceQualifiedName == ai.Action))
                .Union(actions.Where(a => a.IsBound && a.BindingParameter?.Type == Type));
        }
        public void ImportFunctions(IEnumerable<FunctionImport> functionImports, IEnumerable<Function> functions) {
            Functions = functionImports
                .Where(f => f.EntitySet == Name)
                .Select(fi => functions.FirstOrDefault(f => f.NamespaceQualifiedName == fi.Function))
                .Union(functions.Where(f => f.IsBound && f.BindingParameter?.Type == Type));
        }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Namespace => EntityContainer.Schema.Namespace; 
        public string NamespaceQualifiedName => $"{Namespace}.{Name}";
        public IEnumerable<Action> Actions { get; set; }
        public IEnumerable<Function> Functions { get; set; }
        public IEnumerable<NavigationPropertyBinding> NavigationPropertyBindings { get; set; }
        public object ToLiquid()
        {
            return new
            {
                Name,
                NamespaceQualifiedName
            };
        }
    }
}
