﻿using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class Property : Annotable
    {
        public StructuredType StructuredType {get; private set;}
        public Property(XElement element, StructuredType structured) : base(element)
        {
            StructuredType = structured;
            Name = element.Attribute("Name")?.Value;
            IsCollection = element.Attribute("Type")?.Value.StartsWith("Collection(") ?? false;
            Type = element.Attribute("Type")?.Value;
            if (!string.IsNullOrWhiteSpace(Type) && Type.StartsWith("Collection("))
                Type = Type.Substring(11, Type.Length - 12);
            MaxLength = element.Attribute("MaxLength")?.Value;
            DefaultValue = element.Attribute("DefaultValue")?.Value;
            Nullable = !(element.Attribute("Nullable")?.Value == "false");
            SRID = element.Attribute("SRID")?.Value;
            Scale = element.Attribute("Scale")?.Value;
            Precision = element.Attribute("Precision")?.Value;
        }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public bool IsEdmType => !String.IsNullOrWhiteSpace(Type) && Type.StartsWith("Edm.");
        public EnumType? EnumType => Program.Metadata.FindEnumType(Type);
        public bool IsEnumType => EnumType != null;
        public ComplexType ComplexType => Program.Metadata.FindComplexType(Type);
        public bool IsComplexType => ComplexType != null;
        public EntityType EntityType => Program.Metadata.FindEntityType(Type);
        public bool IsEntityType => EntityType != null;
        public bool IsCollection { get; set; }
        public bool Nullable { get; set; }
        public string? MaxLength { get; set; }
        public string DefaultValue { get; set; }
        public string DisplayName { get; set; }
        public string SRID { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
    }
    public class PropertyRef
    {
        public string Name { get; set; }
        public string Alias { get; set; }
    }
}
