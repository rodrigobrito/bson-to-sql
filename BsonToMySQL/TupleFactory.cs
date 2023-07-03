namespace BsonToMySQL
{
    public class TupleFactory
    {
        public static IList<TupleGroup> Create(IList<Document> documents)
        {
            if (documents == null) throw new ArgumentNullException(nameof(documents));
            if (documents.Count == 0) throw new ArgumentException("Expected at least one document.", nameof(documents));

            return CreateTuplesGroup(documents);
        }

        private static IList<TupleGroup> CreateTuplesGroup(IList<Document> documents)
        {
            var ret = new List<TupleGroup>();
            foreach (var document in documents)
            {
                var tuplesGroup = CreateTuplesGroup(document);
                ret.AddRange(tuplesGroup);
            }
            return ret;
        }

        private static IList<TupleGroup> CreateTuplesGroup(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var tuples = new List<TupleGroup>();

            // Add tuple from root document
            var tuple = new TupleGroup
            {
                Name = document.DocumentName.ToLower(),
                TupleColumnValues = CreateTupleColumnValues(document.Attributes)
            };
            tuples.Add(tuple);

            // Add tuples from sub documents
            var tupleFromSubDocuments = CreateTupleGroupsFromSubDocuments(tuple.Name, document.Attributes);
            if (tupleFromSubDocuments.Any())
                tuples.AddRange(tupleFromSubDocuments);

            return tuples;
        }

        private static IList<TupleColumnValue> CreateTupleColumnValues(IList<DocumentAttribute> attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));

            var tuples = new Dictionary<string, TupleColumnValue>();
            foreach (var attr in attributes.Where(att => att.IsPrimitive).ToList())
            {
                if (!tuples.TryGetValue(attr.Name.ToLower(), out var columnValue))
                {
                    var tuple = new TupleColumnValue
                    {
                        Name = attr.Name,
                        Type = attr.Type,
                        Value = attr.Value,
                    };
                    tuples.Add(attr.Name.ToLower(), tuple);
                }
            }
            return tuples.Select(t => t.Value).ToList();
        }

        private static IList<TupleGroup> CreateTupleGroupsFromSubDocuments(string tupleGroupName, IList<DocumentAttribute> attributes)
        {
            var tables = new List<TupleGroup>();
            foreach (var attr in attributes.Where(att => !att.IsPrimitive).ToList())
            {
                if (attr == null || attr.Attributes == null || attr.Attributes.Count == 0) continue;

                var namedTuple = new TupleGroup
                {
                    Name = $"{tupleGroupName}_{attr.Name.ToLower()}",
                };
                namedTuple.TupleColumnValues = CreateTupleColumnValues(attr.Attributes);
                tables.Add(namedTuple);

                if (attr.Attributes.Any(x => !x.IsPrimitive))
                {
                    var namedTuplesFromSubDocuments = CreateTupleGroupsFromSubDocuments(namedTuple.Name, attr.Attributes);
                    tables.AddRange(namedTuplesFromSubDocuments);
                }
            }
            return tables;
        }
    }
}
