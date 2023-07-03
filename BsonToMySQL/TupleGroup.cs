namespace BsonToMySQL
{
    public class TupleGroup
    {
        public string Name { get; set; } = string.Empty;
        public IList<TupleColumnValue> TupleColumnValues { get; set; } = new List<TupleColumnValue>();
    }
}