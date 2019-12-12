﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public abstract class StructuredType 
    {
        public Schema Schema {get; private set;}
        public StructuredType(XElement sourceElement, Schema schema)
        {
            this.Schema = schema;
            Name = sourceElement.Attribute("Name")?.Value;
            BaseType = sourceElement.Attribute("BaseType")?.Value;
            OpenType = sourceElement.Attribute("OpenType")?.Value == "true";
            HasStream = sourceElement.Attribute("HasStream")?.Value == "true";

            Keys = sourceElement.Descendants()
                    .Where(a => a.Name.LocalName == "Key")
                    .Descendants()
                    .Select(element => new PropertyRef() {
                        Name = element.Attribute("Name")?.Value,
                        Alias = element.Attribute("Alias")?.Value
                    })
                    .ToList();

            Properties = sourceElement.Descendants().Where(a => a.Name.LocalName == "Property")
                .Select(element => new Property()
                    {
                        Collection = element.Attribute("Type")?.Value.StartsWith("Collection(") ?? false,
                        Name = element.Attribute("Name")?.Value,
                        MaxLength = element.Attribute("MaxLength")?.Value,
                        Nullable = !(element.Attribute("Nullable")?.Value == "false"),
                        Type = element.Attribute("Type")?.Value.TrimStart("Collection(".ToCharArray()).TrimEnd(')'),
                        SRID = element.Attribute("SRID")?.Value
                    }).ToList();

            NavigationProperties = sourceElement.Descendants().Where(a => a.Name.LocalName == "NavigationProperty")
                .Select(element => new NavigationProperty()
                    {
                        Name = element.Attribute("Name")?.Value.Split(".").Last(),
                        FullName = element.Attribute("Name")?.Value,
                        MaxLength = null,
                        Collection = element.Attribute("Type")?.Value.StartsWith("Collection(") ?? false,
                        Partner = element.Attribute("Partner")?.Value,
                        Type = element.Attribute("Type")?.Value.TrimStart("Collection(".ToCharArray()).TrimEnd(')'),
                        ReferentialConstraint = element.Descendants().SingleOrDefault(a => a.Name.LocalName == "ReferentialConstraint")?.Attribute("Property")?.Value,
                        ReferencedProperty = element.Descendants().SingleOrDefault(a => a.Name.LocalName == "ReferentialConstraint")?.Attribute("ReferencedProperty")?.Value
                }).ToList();
        }
        public string Namespace => this.Schema.Namespace;
        public string Name { get; private set; }
        public string BaseType { get; private set; }
        public bool OpenType { get; private set; }
        public bool HasStream { get; private set; }
        public string FullName { get { return $"{this.Namespace}.{this.Name}"; } }
        public bool IsCompositeKey { get { return this.Keys.Count() > 1; } }
        public List<PropertyRef> Keys { get; private set; }
        public List<Property> Properties { get; private set; }
        public List<NavigationProperty> NavigationProperties { get; set; }
        public void AddActions(IEnumerable<Action> actions) {
            Actions = actions.Where(a => a.IsBound && a.BindingParameter == FullName);
        }
        public void AddFunctions(IEnumerable<Function> functions) {
            Functions = functions.Where(f => f.IsBound && f.BindingParameter == FullName);
        }
        public IEnumerable<Action> Actions { get; set; } = Enumerable.Empty<Action>();
        public IEnumerable<Function> Functions { get; set; } = Enumerable.Empty<Function>();
    }
}
