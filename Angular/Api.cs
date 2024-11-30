using System.Text.Json;
using System.Text.RegularExpressions;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;
using Action = ODataApiGen.Models.Action;

namespace ODataApiGen.Angular
{
    public class Api : AngularRenderable
    {
        public Package Package { get; private set; }
        public Api(Package package, ApiOptions options) : base(options)
        {
            Package = package;
        }
        // Imports
        public override IEnumerable<string> ImportTypes => Package.Schemas.SelectMany(m => m.ImportTypes);
        // Exports
        public override IEnumerable<Import> Imports => GetImportRecords();
        public override string Name => Package.Name + "Api";
        // About File
        public override string FileName => Package.Name.Dasherize() + ".api";
        public override string Directory => "";
        public IEnumerable<EnumTypeConfig> EnumTypeConfigs { get; set; }
        public IEnumerable<StructuredTypeConfig> StructuredTypeConfigs { get; set; }
        public IEnumerable<EntitySetConfig> EntitySetConfigs { get; set; }
        public IEnumerable<SingletonConfig> SingletonConfigs { get; set; }
        public IEnumerable<CallableConfig> CallableConfigs { get; set; }
        public IEnumerable<EnumType> EnumTypes => EnumTypeConfigs.Select(e => e.Enum.EdmEnumType);
        public IEnumerable<ComplexType> ComplexTypes => StructuredTypeConfigs.Select(e => e.Entity.EdmStructuredType).OfType<ComplexType>();
        public IEnumerable<EntityType> EntityTypes => StructuredTypeConfigs.Select(e => e.Entity.EdmStructuredType).OfType<EntityType>();
        public IEnumerable<EntitySet> EntitySets => EntitySetConfigs.Select(s => s.Service.EdmEntitySet);
        public IEnumerable<Singleton> Singletons => SingletonConfigs.Select(s => s.Service.EdmSingleton);
        public IEnumerable<Function> Functions => CallableConfigs.Select(e => e.Callable).OfType<Function>();
        public IEnumerable<Action> Actions => CallableConfigs.Select(e => e.Callable).OfType<Action>();
        public string Typescript
        {
            get
            {
                var useStrings = true;
                var root = new Dictionary<string, object>();
                var elements = EnumTypes.Select(
                    e => new { typ = "EnumType", e.Name, e.Alias, e.Namespace, e.NamespaceQualifiedName, e.AliasQualifiedName }
                ).Union(
                    EntityTypes.Select(
                    e => new { typ = "EntityType", e.Name, e.Alias, e.Namespace, e.NamespaceQualifiedName, e.AliasQualifiedName })
                ).Union(
                    ComplexTypes.Select(
                    e => new { typ = "ComplexType", e.Name, e.Alias, e.Namespace, e.NamespaceQualifiedName, e.AliasQualifiedName })
                ).Union(
                    EntitySets.Select(
                    e => new { typ = "EntitySet", e.Name, e.Alias, e.Namespace, e.NamespaceQualifiedName, e.AliasQualifiedName })
                ).Union(
                    Functions.Select(
                    e => new { typ = "Function", e.Name, e.Alias, e.Namespace, e.NamespaceQualifiedName, e.AliasQualifiedName })
                ).Union(
                    Actions.Select(
                    e => new { typ = "Action", e.Name, e.Alias, e.Namespace, e.NamespaceQualifiedName, e.AliasQualifiedName })
                );
                foreach (var element in elements)
                {
                    var current = root;
                    foreach (var chunk in element.Namespace.Split("."))
                    {
                        if (!current.ContainsKey(chunk))
                        {
                            current.Add(chunk, new Dictionary<string, object>());
                        }
                        current = (Dictionary<string, object>) current[chunk];
                    }
                    if (!current.ContainsKey(element.Name))
                    {
                        if (useStrings)
                        {
                            current.Add(element.Name, element.NamespaceQualifiedName);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(element.Alias))
                            {
                                current.Add(element.Name, new { element.typ, nqn = element.NamespaceQualifiedName, aqn = element.AliasQualifiedName });
                            }
                            else
                            {
                                current.Add(element.Name, new { element.typ, nqn = element.NamespaceQualifiedName });
                            }
                        }
                    }
                }
                var jsonText = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                string regexPattern = "\"([^\"]+)\":"; // the "propertyName": pattern
                return Regex.Replace(jsonText, regexPattern, "$1:");
            }
        }
    }
}