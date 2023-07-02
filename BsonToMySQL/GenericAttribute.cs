namespace BsonToMySQL
{
    public class GenericAttribute
    {
        public string? Type { get; set; } = "VARCHAR(2)";
        public GenericRecord? Table { get; set; } = null;
        public IList<GenericAttribute>? Rows { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsPrimitive
        {
            get
            {
                if (Type == "OBJECT") return false;
                return Rows == null || Rows.Count == 0;
            }
        }
    }
}