using DotLiquid;
using ODataApiGen.Abstracts;

namespace ODataApiGen.Flutter
{
    public class Package : Abstracts.Package, ILiquidizable
    {
        public Module Module { get; private set; }
        public ApiConfig Config { get; private set; }
        public Index Index { get; private set; }
        public ICollection<SchemaConfig> Schemas { get; private set; } = new List<SchemaConfig>();
        public IEnumerable<Enum> Enums => Schemas.SelectMany(s => s.Enums);
        public Enum FindEnum(string type) {
            return Enums.FirstOrDefault(m => m.EdmEnumType.IsTypeOf(type));
        }
        public IEnumerable<Entity> Entities => Schemas.SelectMany(s => s.Entities);
        public Entity FindEntity(string type) {
            return Entities.FirstOrDefault(m => m.EdmStructuredType.IsTypeOf(type));
        }
        public IEnumerable<Model> Models => Schemas.SelectMany(s => s.Models);
        public Model FindModel(string type) {
            return Models.FirstOrDefault(m => m.EdmStructuredType.IsTypeOf(type));
        }
        public IEnumerable<Collection> Collections => Schemas.SelectMany(s => s.Collections);
        public Collection FindCollection(string type) {
            return Collections.FirstOrDefault(m => m.EdmStructuredType.IsTypeOf(type));
        }
        public Package(ApiOptions options) : base(options)
        {
            Module = new Module(this, options);
            Config = new ApiConfig(this, options);
            Index = new Index(this, options);
        }

        public override void Build()
        {
            foreach (var schema in Program.Metadata.Schemas)
            {
                Schemas.Add(new SchemaConfig(schema, Options));
            }
        }
        public override void ResolveDependencies()
        {
            // Enums
            foreach (var enumm in Enums)
            {
            }
            // Entities
            foreach (var entity in Entities)
            {
                if (!String.IsNullOrEmpty(entity.EdmStructuredType.BaseType))
                {
                    var baseEntity = Entities.FirstOrDefault(e => e.EdmStructuredType.IsTypeOf(entity.EdmStructuredType.BaseType));
                    entity.SetBase(baseEntity);
                    entity.AddDependency(baseEntity);
                }
            }
            // Models
            foreach (var model in Models)
            {
                if (!String.IsNullOrEmpty(model.EdmStructuredType.BaseType))
                {
                    var baseModel = Models.FirstOrDefault(e => e.EdmStructuredType.IsTypeOf(model.EdmStructuredType.BaseType));
                    model.SetBase(baseModel);
                    model.AddDependency(baseModel);
                }
                var collection = Collections.FirstOrDefault(m => m.EdmStructuredType.Name == model.EdmStructuredType.Name);
                if (collection != null)
                {
                    model.SetCollection(collection);
                    //model.AddDependency(collection);
                }
            }
            // Collections
            foreach (var collection in Collections)
            {
                if (!String.IsNullOrEmpty(collection.EdmStructuredType.BaseType))
                {
                    var baseCollection = Collections.FirstOrDefault(e => e.EdmStructuredType.IsTypeOf(collection.EdmStructuredType.BaseType));
                    collection.SetBase(baseCollection);
                    collection.AddDependency(baseCollection);
                }
            }

            foreach (var schema in Schemas)
            {
                schema.ResolveDependencies();
                foreach (var container in schema.Containers)
                {
                    container.ResolveDependencies(Enums, Entities, Models, Collections);
                }
            }

            // Resolve Renderable Dependencies
            foreach (var renderable in Renderables)
            {
                var types = renderable.ImportTypes;
                if (renderable is Enum || renderable is EnumTypeConfig || renderable is Entity || renderable is Model || renderable is Collection || renderable is Service)
                {
                    renderable.AddDependencies(
    Enums.Where(e => e != renderable && types.Any(type => e.EdmEnumType.IsTypeOf(type))));
                    if (renderable is Entity || renderable is Model || renderable is Collection || renderable is Service)
                    {
                        renderable.AddDependencies(
        Entities.Where(e => e != renderable && types.Any(type => e.EdmStructuredType.IsTypeOf(type))));
                        if (!(renderable is EnumTypeConfig))
                        {
                            {
                                renderable.AddDependencies(
                Models.Where(e => e != renderable && types.Any(type => e.EdmStructuredType.IsTypeOf(type))));
                                renderable.AddDependencies(
                Collections.Where(e => e != renderable && types.Any(type => e.EdmStructuredType.IsTypeOf(type))));
                            }
                        }
                    }
                }
            }

            // Module
            Module.AddDependencies(Schemas.SelectMany(s => s.Containers.Select(c => c.Service)));
            Module.AddDependencies(Schemas.SelectMany(s => s.Containers.SelectMany(c => c.Services)));
            // Config
            Config.AddDependencies(Schemas);
            // Index
            Index.AddDependencies(Schemas.SelectMany(s => s.Enums));
            Index.AddDependencies(Schemas.SelectMany(s => s.EnumConfigs));
            Index.AddDependencies(Schemas.SelectMany(s => s.Entities));
            Index.AddDependencies(Schemas.SelectMany(s => s.Models));
            Index.AddDependencies(Schemas.SelectMany(s => s.Collections));
            Index.AddDependencies(Schemas.SelectMany(s => s.EntityConfigs));
            Index.AddDependencies(Schemas.SelectMany(s => s.Containers.Select(c => c.Service)));
            Index.AddDependencies(Schemas.SelectMany(s => s.Containers.SelectMany(c => c.Services)));
            Index.AddDependencies(Schemas.SelectMany(s => s.Containers.SelectMany(c => c.EntitySetConfigs)));
            Index.AddDependency(Config);
            Index.AddDependency(Module);
        }

        public override IEnumerable<string> GetAllDirectories()
        {
            return Schemas.SelectMany(s => s.GetAllDirectories())
                .Distinct();
        }

        public object ToLiquid()
        {
            return new
            {
                Name,
                ServiceRootUrl,
                Version,
                Creation = DateTime.Now,
                Schemas
            };
        }

        public override IEnumerable<Renderable> Renderables
        {
            get
            {
                var renderables = new List<Renderable>();
                renderables.Add(Module);
                renderables.Add(Config);
                renderables.Add(Index);
                renderables.AddRange(Schemas);
                renderables.AddRange(Schemas.SelectMany(s => s.Renderables));
                return renderables;
            }
        }
    }
}