namespace BsonToMySQL
{
    public class TupleGroup
    {
        public string Name { get; set; } = string.Empty;
        public IList<Tuple> Tuples { get; set; } = new List<Tuple>();
    }
}