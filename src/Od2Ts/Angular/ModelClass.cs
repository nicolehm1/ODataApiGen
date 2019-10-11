using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Od2Ts.Models;

namespace Od2Ts.Angular
{
    public class ModelProperty : DotLiquid.ILiquidizable
    {
        private Models.Property Value { get; set; }
        public ModelProperty(Od2Ts.Models.Property prop)
        {
            this.Value = prop;
        }
        public IEnumerable<string> Name { get; set; }

        public string Type => EntityProperty.GetTypescriptType(Value.Type);
        public object ToLiquid()
        {
            return new
            {
                Name = Value.Name + (Value.IsNullable ? "?" : ""),
                Type = this.Type + (Value.IsCollection ? Value.IsEdmType ? "[]" : "Collection" : "")
            };
        }
    }
    public class ModelClass : Model
    {
        public ModelClass(StructuredType type) : base(type)
        {
        }
        public override string FileName => this.EdmStructuredType.Name.ToLower() + ".model";
        public override IEnumerable<string> ExportTypes => new string[] { this.Name };
        public override IEnumerable<Import> Imports => GetImportRecords();
        public string GetModelType(string type)
        {
            if (String.IsNullOrWhiteSpace(type))
                return "any";
            switch (type)
            {
                case "Edm.String":
                case "Edm.Duration":
                case "Edm.Guid":
                case "Edm.Binary":
                    return "String";
                case "Edm.Int16":
                case "Edm.Int32":
                case "Edm.Int64":
                case "Edm.Double":
                case "Edm.Decimal":
                case "Edm.Single":
                case "Edm.Byte":
                    return "Number";
                case "Edm.Boolean":
                    return "Boolean";
                case "Edm.DateTimeOffset":
                    return "Date";
                default:
                    {
                        return type.Contains(".") && !type.StartsWith("Edm") ? type : "Object";
                    }
            }
        }
        protected string RenderProperty(Models.Property prop)
        {
            var field = $"{prop.Name}" +
                (prop.IsNullable ? "?:" : ":") +
                $" {this.GetTypescriptType(prop.Type)}";
            if (prop.IsEdmType) {
                field = $"{field}" + (prop.IsCollection ? "[];" : ";");
            } else {
                field = $"{field}" + (prop.IsCollection ? "Collection;" : ";");
            }
            return field;
        }

        public IEnumerable<string> RenderModelMethods(NavigationProperty nav)
        {
            var type = this.GetTypescriptType(nav.Type);
            var name = nav.Name[0].ToString().ToUpper() + nav.Name.Substring(1);
            var methodRelationName = $"get{name}";
            var baseMethodRelationName = nav.IsCollection ? $"relatedCollection" : $"relatedModel";
            var returnType = (nav.IsCollection) ?
                $"Collection<{type}>" :
                $"{type}";
            // Navigation
            var methods = new List<string>() {$@"public {methodRelationName}(): {returnType} {{
    return this.{baseMethodRelationName}('{nav.Name}') as {returnType};
  }}"};
            return methods;
        }
        public IEnumerable<string> RenderODataModelMethods(NavigationProperty nav)
        {
            var type = this.GetTypescriptType(nav.Type);
            var name = nav.Name[0].ToString().ToUpper() + nav.Name.Substring(1);
            var methodRelationName = $"get{name}";
            var methodCreateName = nav.IsCollection ? $"add{type}To{name}" : $"set{type}As{name}";
            var methodDeleteName = nav.IsCollection ? $"remove{type}From{name}" : $"unset{type}As{name}";
            var baseMethodRelationName = nav.IsCollection ? $"relatedODataCollection" : $"relatedODataModel";
            var baseMethodCreateName = nav.IsCollection ? $"createODataCollectionRef" : $"createODataModelRef";
            var baseMethodDeleteName = nav.IsCollection ? $"deleteODataCollectionRef" : $"deleteODataModelRef";
            var returnType = (nav.IsCollection) ?
                $"ODataCollection<{type}>" :
                $"{type}";
            // Navigation
            var methods = new List<string>() {$@"public {methodRelationName}(): {returnType} {{
    return this.{baseMethodRelationName}('{nav.Name}') as {returnType};
  }}"};
            // Link
            methods.Add($@"public {methodCreateName}(target: ODataQueryBase, options?) {{
    return this.{baseMethodCreateName}('{nav.Name}', target, options);
  }}");
            // Unlink
            methods.Add($@"public {methodDeleteName}(target: ODataQueryBase, options?) {{
    return this.{baseMethodDeleteName}('{nav.Name}', target, options);
  }}");
            return methods;
        }
        public string RenderKey(PropertyRef propertyRef)
        {
            var d = new Dictionary<string, string>() {
                {"name", $"'{propertyRef.Name}'"}
            };
            if (!String.IsNullOrWhiteSpace(propertyRef.Alias)) {
                d.Add("name", $"'{propertyRef.Alias}'");
                d.Add("resolve", $"(model) => model.{propertyRef.Name.Replace('/', '.')}");
            }
            return $"{{{String.Join(", ", d.Select(p => $"{p.Key}: {p.Value}"))}}}";
        }
        public IEnumerable<string> SchemaKeys => this.EdmStructuredType.Keys.Select(prop => this.RenderKey(prop));
        public string RenderField(Models.Property property)
        {
            var d = new Dictionary<string, string>() {
                {"name", $"'{property.Name}'"}
            };
            var propType = property.Type;
            if (property.IsCollection)
                propType = $"{propType}Collection";
            var type = this.Dependencies.FirstOrDefault(dep => dep.Type == propType);
            d.Add("type", type == null ? $"'{this.GetModelType(property.Type)}'" : $"'{type.Type}'");
            if (property.IsNullable)
                d.Add("isNullable", "true");
            if (!String.IsNullOrEmpty(property.MaxLength) && property.MaxLength.ToLower() != "max")
                d.Add("maxLength", property.MaxLength);
            if (property.IsCollection)
                d.Add("isCollection", "true");
            if (type is Enum) {
                d.Add("isFlags", (type as Enum).IsFlags);
            } else if (property is NavigationProperty) {
                // Is Navigation
                d.Add("isNavigation", "true");
                var nav = property as NavigationProperty;
                if (!String.IsNullOrEmpty(nav.ReferentialConstraint))
                    d.Add("field", $"'{nav.ReferentialConstraint}'");
                if (!String.IsNullOrEmpty(nav.ReferencedProperty))
                    d.Add("ref", $"'{nav.ReferencedProperty}'");
            }
            return $"{{{String.Join(", ", d.Select(p => $"{p.Key}: {p.Value}"))}}}";
        }
        public IEnumerable<string> SchemaFields => this.EdmStructuredType.Properties
                .Union(this.EdmStructuredType.NavigationProperties)
                .Select(prop => this.RenderField(prop));
        public IEnumerable<Angular.ModelProperty> Properties => this.EdmStructuredType.Properties
                .Union(this.EdmStructuredType.NavigationProperties)
                .Select(prop => new Angular.ModelProperty(prop));
        public override string Render()
        {
            var properties = this.EdmStructuredType.Properties
                .Select(prop => this.RenderProperty(prop)).ToList();
            properties.AddRange(this.EdmStructuredType.NavigationProperties
                .Select(prop => this.RenderProperty(prop)));
            var methods = this.EdmStructuredType.NavigationProperties
                .SelectMany(nav => 
                    (EdmStructuredType is ComplexType) ? 
                    this.RenderModelMethods(nav) : 
                    this.RenderODataModelMethods(nav));

            var imports = this.RenderImports();
            return $@"{String.Join("\n", imports)}
import {{ Schema, Model, ODataModel, ODataCollection, PlainObject }} from 'angular-odata';

  static set = '{(this.Service != null ? this.Service.EdmEntitySet.EntitySetName : "")}';
  static type = '{this.Type}';
  static schema = {(this.Base == null ? $"Schema.create({{" : $"{this.Base.Name}.schema.extend({{")}
    keys: [
    ],
    fields: [
    ]
  }});
  {String.Join("\n  ", properties)}

  {String.Join("\n  ", methods)}
}}";
        }
    }
}