namespace ODataApiGen.Abstracts
{
    public class ApiOptions {
        public required string Name {get; set;}
        public required string Version {get; set;}
        public required string ServiceRootUrl {get; set;}
        public bool Models {get; set;}
        public bool GeoJson {get; set;}
    }
    public abstract class Renderable {
        public ApiOptions Options  {get; set;} 
        public Renderable(ApiOptions options) {
            Options = options;
        }
        // About Identity
        public abstract string Name { get; }
        // About File
        public abstract string FileName { get; }
        public abstract string FileExtension { get; }
        public abstract string Directory { get; }
        public Uri Uri => !String.IsNullOrEmpty(Directory) ? new Uri($"r://{Directory}{Path.DirectorySeparatorChar}{FileName}", UriKind.Absolute) : new Uri($"r://{FileName}");

        // About Template
        public string TemplateFile => GetType().Name + FileExtension;
        // About References
        public string ImportedName {get;set;} = String.Empty;
        public void CleanImportedNames() {
            foreach (var dependency in Dependencies) {
               dependency.Item2.ImportedName = dependency.Item2.Name; 
            }
        }
        public abstract IEnumerable<string> ImportTypes {get; }
        protected List<Tuple<string, Renderable>> Dependencies {get; set;} = new List<Tuple<string, Renderable>>();
        public void AddDependency(Renderable renderable) {
            if (Dependencies.All(d => d.Item2 != renderable)) {
                var alias = renderable.Name;
                while (Dependencies.Any(d => d.Item1 == alias)) {
                    alias = NameGenerator.GetRandomName();
                }
                Dependencies.Add(Tuple.Create(alias, renderable));
            }
        }

        public void AddDependencies(IEnumerable<Renderable> renderables) {
            foreach (var renderable in renderables)
                AddDependency(renderable);
        }
    }
}