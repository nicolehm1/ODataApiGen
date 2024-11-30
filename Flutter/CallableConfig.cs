using DotLiquid;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class CallableParameterConfig : ILiquidizable
    {
        protected Parameter Value { get; set; }
        public CallableParameterConfig(Parameter property) {
            Value = property;
        }
        public string Name => Value.Name;

        public string Type { 
            get {
                var values = new Dictionary<string, string>();
                values.Add("type", $"'{Value.Type}'");
                if (Value.IsCollection)
                    values.Add("collection", "true");
                if (!Value.Nullable)
                    values.Add("nullable", "false");
                return $"{{{String.Join(", ", values.Select(p => $"{p.Key}: {p.Value}"))}}}";
            }
        } 
        public object ToLiquid() {
            return new {
                Name, Type
            };
        }
    }
    public class CallableConfig : ILiquidizable 
    {
        public Callable Callable {get; private set;}
        public string Name => Callable.Name;
        public CallableConfig(Callable callable) {
            Callable = callable;
        }

        public IEnumerable<CallableParameterConfig> Parameters {
            get {
                return Callable.Parameters.ToList().Select(param => new CallableParameterConfig(param));
            }
        }

    public object ToLiquid()
    {
        return new {
            Name,
            HasPath = !String.IsNullOrWhiteSpace(Callable.EntitySetPath),
            Callable.EntitySetPath,
            HasParameters = Parameters.Count() > 0,
            Parameters,
            Bound = Callable.IsBound,
            Composable = Callable.IsComposable,
            Callable.ReturnType,
            Callable.ReturnsCollection
        };
    }
  }
}