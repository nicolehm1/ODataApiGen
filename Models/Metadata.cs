using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace ODataApiGen.Models
{
    public class Metadata 
    {
        private static ILogger Logger {get;} = Program.LoggerFactory.CreateLogger<Metadata>();
        public string? Version { get; private set; }
        public List<Schema> Schemas { get; private set; }
        public string Namespace => Schemas.Select(s => s.Namespace).OrderBy(n => n.Length).First();
        public IEnumerable<EnumType> EnumTypes => Schemas.SelectMany(s => s.EnumTypes);
        public EnumType? FindEnumType(string type) => EnumTypes.FirstOrDefault(e => e.IsTypeOf(type));
        public IEnumerable<ComplexType> ComplexTypes => Schemas.SelectMany(s => s.ComplexTypes);
        public ComplexType? FindComplexType(string type) => ComplexTypes.FirstOrDefault(c => c.IsTypeOf(type));
        public IEnumerable<EntityType> EntityTypes => Schemas.SelectMany(s => s.EntityTypes);
        public EntityType? FindEntityType(string type) => EntityTypes.FirstOrDefault(c => c.IsTypeOf(type));
        public Function[] Functions => Schemas.SelectMany(s => s.Functions).ToArray();
        public Action[] Actions => Schemas.SelectMany(s => s.Actions).ToArray();
        public Association[] Associations => Schemas.SelectMany(s => s.Associations).ToArray();
        public EntitySet[] EntitySets => Schemas.SelectMany(s => s.EntityContainers.SelectMany(c => c.EntitySets)).ToArray();
        public IEnumerable<KeyValuePair<string, List<Annotation>>> Annotations => Schemas.SelectMany(s => s.Annotations);

        #region Static Loaders
        public static List<Schema> ReadSchemas(XDocument xdoc)
        {
            Logger.LogDebug("Parsing entity types...");
            var schemas = new List<Schema>();
            var elements = xdoc.Descendants().Where(a => a.Name.LocalName == "Schema");
            
            foreach (var xElement in elements)
            {
                var entity = new Schema(xElement);
                schemas.Add(entity);
                Logger.LogInformation("Schema Type \'{EntityNamespace}\' parsed", entity.Namespace);
            }
            return schemas;
        }
        #endregion
        public Metadata(XDocument xDoc) 
        {
            Version = xDoc.Descendants().FirstOrDefault(a => a.Name.LocalName == "Edmx")?.Attribute("Version")?.Value;
            if (Version == "1.0") {
                Version = xDoc.Descendants().FirstOrDefault(a => a.Name.LocalName == "DataServices")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "DataServiceVersion")?.Value;
            }
            Schemas = ReadSchemas(xDoc);
            foreach (var schema in Schemas) {
                schema.ResolveFunctions(Functions);
                schema.ResolveActions(Actions);
                schema.ResolveAssociations(Associations);
                schema.ResolveAnnotations(Annotations);
            }
        }
    }
}
