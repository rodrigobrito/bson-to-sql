namespace BsonToMySQL
{
    public class Tuple
    {
        public Tuple() { }
        public Tuple(IList<TupleColumnValue> columnsValues) { ColumnValues = columnsValues; }
        public IList<TupleColumnValue> ColumnValues { get; set; } = new List<TupleColumnValue>();
    }
}