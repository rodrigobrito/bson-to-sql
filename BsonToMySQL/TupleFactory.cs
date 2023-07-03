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

            var tupleGroups = new List<TupleGroup>();

            // Add tuple from root document
            var group = new TupleGroup
            {
                Name = document.DocumentName.ToLower(),
            };
            group.Tuples.Add(new Tuple(CreateTupleColumnValues(document.Attributes)));
            tupleGroups.Add(group);

            // Add group tuples from sub documents
            var groups = CreateGroupsFromSubDocuments(group.Name, document.Attributes);
            if (groups.Any())
                tupleGroups.AddRange(groups);

            return tupleGroups;
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

        private static IList<TupleGroup> CreateGroupsFromSubDocuments(string groupName, IList<DocumentAttribute> attributes)
        {
            var groups = new List<TupleGroup>();
            foreach (var attr in attributes.Where(att => !att.IsPrimitive).ToList())
            {
                if (attr == null || attr.Attributes == null || attr.Attributes.Count == 0) continue;

                var tuple = new Tuple(CreateTupleColumnValues(attr.Attributes));             
               
                var group = new TupleGroup
                {
                    Name = $"{groupName}_{attr.Name.ToLower()}",
                    Tuples = new List<Tuple> { tuple }
                };      
                groups.Add(group);

                if (attr.Attributes.Any(x => !x.IsPrimitive))
                {
                    var groupFromSubDocuments = CreateGroupsFromSubDocuments(group.Name, attr.Attributes);
                    groups.AddRange(groupFromSubDocuments);
                }

                if (attr.SubDocuments.Any())
                {
                    group.Tuples.Remove(tuple);
                    foreach (var subDocument in attr.SubDocuments)
                    {
                        if (subDocument.Attributes.Any(x => x.IsPrimitive))
                        {
                            group.Tuples.Add(new Tuple(CreateTupleColumnValues(subDocument.Attributes)));
                        }
                    }
                }
            }
            return groups;
        }
    }
}
