using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class SchemaConfig : FlutterRenderable, ILiquidizable
    {
        public Schema EdmSchema { get; private set; }
        public override string FileName => EdmSchema.Namespace.Split('.').Last().Dasherize() + ".schema";
        //TODO: Create nice schema names
        public override string Name => Utils.ToDartName(EdmSchema.Namespace.Split('.').Last(), DartElement.Class) + "Schema";
        public ICollection<Enum> Enums { get; } = new List<Enum>();
        public ICollection<EnumTypeConfig> EnumConfigs { get; } = new List<EnumTypeConfig>();
        public ICollection<Entity> Entities { get; } = new List<Entity>();
        public ICollection<Model> Models { get; } = new List<Model>();
        public ICollection<Collection> Collections { get; } = new List<Collection>();
        public ICollection<StructuredTypeConfig> EntityConfigs { get; } = new List<StructuredTypeConfig>();
        public ICollection<CallableConfig> CallablesConfigs { get; } = new List<CallableConfig>();
        public ICollection<Container> Containers { get; } = new List<Container>();
        public SchemaConfig(Schema schema, ApiOptions options) : base(options)
        {
            EdmSchema = schema;
            AddEnums(schema.EnumTypes);
            AddComplexes(schema.ComplexTypes);
            AddEntities(schema.EntityTypes);
            AddCallables(schema.Functions);
            AddCallables(schema.Actions);
            foreach (var container in schema.EntityContainers)
            {
                Containers.Add(new Container(container, options));
            }
        }
        public void AddEnums(IEnumerable<EnumType> enums)
        {
            foreach (var e in enums)
            {
                var enu = new Enum(e, Options);
                Enums.Add(enu);
                var config = new EnumTypeConfig(enu, Options);
                EnumConfigs.Add(config);
            }
        }
        public void AddComplexes(IEnumerable<ComplexType> complexes)
        {
            foreach (var cmplx in complexes)
            {
                StructuredTypeConfig config;
                var entity = new Entity(cmplx, Options);
                Entities.Add(entity);
                if (Options.Models)
                {
                    var model = new Model(cmplx, entity, Options);
                    Models.Add(model);
                    var collection = new Collection(cmplx, model, Options);
                    Collections.Add(collection);
                    config = new StructuredTypeConfig(entity, model, collection, Options);
                } else {
                    config = new StructuredTypeConfig(entity, Options);
                }
                EntityConfigs.Add(config);
            }
        }
        public void AddEntities(IEnumerable<EntityType> entities)
        {
            foreach (var enty in entities)
            {
                StructuredTypeConfig config;
                var entity = new Entity(enty, Options);
                Entities.Add(entity);
                if (Options.Models)
                {
                    var model = new Model(enty, entity, Options);
                    Models.Add(model);
                    var collection = new Collection(enty, model, Options);
                    Collections.Add(collection);
                    config = new StructuredTypeConfig(entity, model, collection, Options);
                } else {
                    config = new StructuredTypeConfig(entity, Options);
                }
                EntityConfigs.Add(config);
            }
        }
        public void AddCallables(IEnumerable<Callable> callables)
        {
            foreach (var callable in callables)
            {
                CallablesConfigs.Add(new CallableConfig(callable));
            }
        }
        // Imports
        public override IEnumerable<string> ImportTypes => new List<string>();
        public override IEnumerable<Import> Imports => GetImportRecords();
        public bool HasAlias => !String.IsNullOrWhiteSpace(EdmSchema.Alias);
        public string Alias => EdmSchema.Alias;
        public string Namespace => EdmSchema.Namespace;
        public override string Directory => EdmSchema.Namespace.Replace('.', Path.DirectorySeparatorChar);
        public void ResolveDependencies()
        {
            AddDependencies(EnumConfigs);
            AddDependencies(EntityConfigs);
            AddDependencies(Containers);
        }
        public IEnumerable<string> GetAllDirectories()
        {
            return Enums.Select(e => e.Directory)
                .Union(Entities.Select(m => m.Directory))
                .Union(Models.Select(m => m.Directory))
                .Union(EntityConfigs.Select(m => m.Directory))
                .Union(EnumConfigs.Select(m => m.Directory))
                .Union(Collections.Select(m => m.Directory))
                .Union(Containers.SelectMany(c => c.GetAllDirectories()));
        }
        public IEnumerable<Renderable> Renderables
        {
            get
            {
                var renderables = new List<Renderable>();
                renderables.AddRange(Enums);
                renderables.AddRange(EnumConfigs);
                renderables.AddRange(Entities);
                renderables.AddRange(Models);
                renderables.AddRange(Collections);
                renderables.AddRange(EntityConfigs);
                renderables.AddRange(Containers);
                renderables.AddRange(Containers.SelectMany(s => s.Renderables));
                return renderables;
            }
        }
        public object ToLiquid()
        {
            return new
            {
                Name = ImportedName
            };
        }
    }
}