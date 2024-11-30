namespace ODataApiGen.Abstracts
{
    public abstract class Package {
        public ApiOptions Options  {get; set;} 
        public string Name => Options.Name;
        public string ServiceRootUrl => Options.ServiceRootUrl;
        public string Version => Options.Version;
        public Package(ApiOptions options)
        {
            Options = options;
        }
        public abstract IEnumerable<Renderable> Renderables { get; }
        public abstract void Build();
        public abstract void ResolveDependencies();

        public abstract IEnumerable<string> GetAllDirectories();

    }
}