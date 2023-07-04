namespace BsonToMySQL
{
    public class Document
    {
        public string DocumentName { get; set; } = string.Empty;

        public IList<DocumentAttribute> Attributes = new List<DocumentAttribute>();    
    }
}