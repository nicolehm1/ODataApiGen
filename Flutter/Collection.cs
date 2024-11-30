using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class Collection : StructuredType
    {
        public Model Model { get; private set; }
        public Collection(Models.StructuredType type, Model model, ApiOptions options) : base(type, options)
        {
            Model = model;
        }

        // Imports
        public override IEnumerable<string> ImportTypes
        {
            get
            {
                var parameters = new List<Parameter>();
                var list = new List<string> {
                    Model.Entity.EdmStructuredType.NamespaceQualifiedName
                };
                list.AddRange(EdmStructuredType.Properties.Select(a => a.Type));
                if (EdmEntityType != null) {
                    list.AddRange(EdmEntityType.Properties.Select(a => a.Type));
                    list.AddRange(EdmEntityType.NavigationProperties.Select(a => a.Type));
                    list.AddRange(EdmEntityType.NavigationProperties.Select(a => a.ToEntityType));
                    list.AddRange(EdmEntityType.Actions.SelectMany(a => CallableNamespaces(a)));
                    list.AddRange(EdmEntityType.Functions.SelectMany(a => CallableNamespaces(a)));
                    foreach (var cal in EdmEntityType.Actions)
                        parameters.AddRange(cal.Parameters);
                    foreach (var cal in EdmEntityType.Functions)
                        parameters.AddRange(cal.Parameters);
                    list.AddRange(parameters.Select(p => p.Type));
                }
                return list.Where(t => !String.IsNullOrWhiteSpace(t) && !t.StartsWith("Edm.")).Distinct();
            }
        }
        // Exports
        public override string FileName => EdmStructuredType.Name.Dasherize() + ".collection";
        public override string Name => Utils.ToDartName(EdmStructuredType.Name, DartElement.Class) + "Collection";
        public string ModelName => Model.ImportedName;
        public override IEnumerable<Import> Imports => GetImportRecords();

        public override object ToLiquid()
        {
            return new {
                Name = ImportedName,
            };
        }

        public IEnumerable<string> Actions {
            get {
                if (EdmEntityType != null) {
                    var collectionActions = EdmEntityType.Actions.Where(a => a.IsCollection);
                    return collectionActions.Count() > 0 ? RenderCallables(collectionActions) : [];
                }
                return [];
            }
        }
        public IEnumerable<string> Functions {
            get {
                if (EdmEntityType != null) {
                    var collectionFunctions = EdmEntityType.Functions.Where(a => a.IsCollection);
                    return collectionFunctions.Count() > 0 ? RenderCallables(collectionFunctions) : [];
                }
                return [];
            }
        }
    }
}