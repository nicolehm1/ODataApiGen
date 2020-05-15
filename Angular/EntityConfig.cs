using System.Collections.Generic;
using System.Linq;
using System;
using ODataApiGen.Models;
using Newtonsoft.Json;
using DotLiquid;

namespace ODataApiGen.Angular
{
    public class EntityFieldConfig : ILiquidizable
    {
        protected Models.Property Value { get; set; }
        protected IEnumerable<PropertyRef> Keys { get; set; }
        public EntityFieldConfig(Models.Property property, IEnumerable<PropertyRef> keys) {
            this.Keys = keys;
            this.Value = property;
        }
        public string Name => Value.Name;

        public string Type { 
            get {
                var values = new Dictionary<string, string>();
                values.Add("type", $"'{AngularRenderable.GetType(this.Value.Type)}'");
                var key = this.Keys.FirstOrDefault(k => k.Name == this.Value.Name);
                if (key != null) {
                    values.Add("key", "true");
                    values.Add("ref", $"'{key.Name}'");
                    if (!String.IsNullOrWhiteSpace(key.Alias)) {
                        values.Add("name", $"'{key.Alias}'");
                    }
                }
                if (!(this.Value is NavigationProperty) && !this.Value.Nullable)
                    values.Add("nullable", "false");
                if (!String.IsNullOrEmpty(this.Value.MaxLength) && this.Value.MaxLength.ToLower() != "max")
                    values.Add("maxLength", this.Value.MaxLength);
                if (!String.IsNullOrEmpty(this.Value.SRID))
                    values.Add("srid", this.Value.SRID);
                if (this.Value.IsCollection)
                    values.Add("collection", "true");
                if (this.Value is NavigationProperty) {
                    // Is Navigation
                    values.Add("navigation", "true");
                    var nav = this.Value as NavigationProperty;
                    if (!String.IsNullOrEmpty(nav.ReferentialConstraint))
                        values.Add("field", $"'{nav.ReferentialConstraint}'");
                    if (!String.IsNullOrEmpty(nav.ReferencedProperty))
                        values.Add("ref", $"'{nav.ReferencedProperty}'");
                }
                var annots = this.Value.Annotations;
                if (annots.Count > 0) {
                    var json = JsonConvert.SerializeObject(annots.Select(annot => annot.ToDictionary()));
                    values.Add("annotations", $"{json}");
                }
                return $"{{{String.Join(", ", values.Select(p => $"{p.Key}: {p.Value}"))}}}";
            }
        } 
        public object ToLiquid() {
            return new {
                Name = this.Name,
                Type = this.Type
            };
        }
    }
    public class EntityConfig : Structured 
    {
        public EntityConfig(StructuredType type) : base(type) { }
        public Angular.Entity Entity {get; private set;}

        public void SetEntity(Entity entity)
        {
            this.Entity = entity;
        }
        public Angular.Model Model {get; private set;}

        public void SetModel(Model model)
        {
            this.Model = model;
        }
        public Angular.Collection Collection {get; private set;}

        public void SetCollection(Collection collection)
        {
            this.Collection = collection;
        }
        public override string FileName => this.EdmStructuredType.Name.ToLower() + ".entity.config";
        public override string Name => this.EdmStructuredType.Name + "EntityConfig";
        public string EntityName => this.EdmStructuredType.Name;

        public string Annotations {
            get {
                return JsonConvert.SerializeObject(this.EdmStructuredType.Annotations.Select(annot => annot.ToDictionary()));
            }
        }

        // Imports
        public override IEnumerable<string> ImportTypes => new List<string> { this.EntityType };

        public IEnumerable<Angular.EntityFieldConfig> Properties {
            get {
                var props = this.EdmStructuredType.Properties.ToList();
                if (this.EdmStructuredType is EntityType) 
                    props.AddRange((this.EdmStructuredType as EntityType).NavigationProperties);
                var keys = (this.EdmStructuredType is EntityType) ? (this.EdmStructuredType as EntityType).Keys : new List<PropertyRef>();
                return props.Select(prop => new EntityFieldConfig(prop, keys));
            }
        } 

        public override object ToLiquid()
        {
            return new {
                Name = this.Name,
                Type = this.Type,
                EntityType = this.EntityType
            };
        }
    }
}