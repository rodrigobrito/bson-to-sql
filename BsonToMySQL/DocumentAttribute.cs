namespace BsonToMySQL
{
    public class DocumentAttribute
    {
        public const string DefaultType = "VARCHAR(3)";
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = DefaultType;
        public Document? Document { get; set; } = null;
        public IList<DocumentAttribute>? Attributes { get; set; } = null;
        public bool IsPrimitive
        {
            get
            {
                if (Type == "DOCUMENT_ATTRIBUTE" || Type == "DOCUMENT_ARRAY") return false;
                return Attributes == null || Attributes.Count == 0;
            }
        }
    }
}