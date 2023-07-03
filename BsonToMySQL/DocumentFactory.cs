using MongoDB.Bson;

namespace BsonToMySQL
{
    public class DocumentFactory
    {
        public static IList<Document> Create(BsonArray? array, string documentName)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (string.IsNullOrWhiteSpace(documentName))
                throw new ArgumentNullException(nameof(documentName));

            var records = new List<Document>();
            foreach (var bsonValue in array)
            {
                var bsonDocument = bsonValue.AsBsonDocument;
                var rec = new Document { DocumentName = documentName };
                BuildAttributes(bsonDocument, rec);
                records.Add(rec);
            }

            var dict = GetMaxSizeOfDocumentAttributes(records);
            foreach (var rec in records)
                SetAttributeTypeAndSize(dict, rec.Attributes);

            return records;
        }

        private static void BuildAttributes(BsonDocument document, Document targetDocument)
        {
            if (targetDocument == null) return;

            foreach (var element in document)
            {
                var docAttr = new DocumentAttribute
                {
                    Name = NormalizeColumnName(element.Name),
                    Document = targetDocument
                };

                var attributeValue = element.Value;
                if (attributeValue == null) continue;

                var targetDocumentName = $"{targetDocument.DocumentName}_{docAttr.Name}";
                switch (attributeValue.BsonType)
                {
                    case BsonType.ObjectId:
                        docAttr.Value = attributeValue.AsObjectId.ToString();
                        break;
                    case BsonType.Int64:
                        docAttr.Value = attributeValue.AsInt64.ToString();
                        docAttr.Type = "BIGINT";
                        break;
                    case BsonType.Int32:
                        docAttr.Value = attributeValue.AsInt32.ToString();
                        docAttr.Type = "BIGINT";
                        break;
                    case BsonType.Decimal128:
                        docAttr.Value = attributeValue.AsDecimal128.ToString();
                        docAttr.Type = "DECIMAL(10, 2)";
                        break;
                    case BsonType.Double:
                        docAttr.Value = attributeValue.AsDouble.ToString();
                        docAttr.Type = "DECIMAL(10, 2)";
                        break;
                    case BsonType.Document:
                        docAttr.Document = CreateDocumentAttribute(targetDocumentName, docAttr, attributeValue, targetDocument.Attributes.First(attr => attr.Name == "_id"));
                        docAttr.Type = "DOCUMENT_ATTRIBUTE";
                        break;
                    case BsonType.Array:
                        docAttr.Document = CreateDocumentArray(targetDocumentName, docAttr, attributeValue, targetDocument.Attributes.First(attr => attr.Name == "_id")); ;
                        docAttr.Type = "DOCUMENT_ARRAY";
                        break;
                    case BsonType.Boolean:
                        docAttr.Value = attributeValue.AsBoolean.ToString();
                        docAttr.Type = "TINYINT";
                        break;
                    case BsonType.DateTime:
                        docAttr.Value = attributeValue.ToUniversalTime().ToString("u").Replace(" ", "T"); ;
                        docAttr.Type = "VARCHAR";
                        break;
                    case BsonType.Null:
                        break;
                    default:
                        docAttr.Value = attributeValue.AsString;
                        break;
                }

                targetDocument.Attributes.Add(docAttr);
            }
        }

        private static Document CreateDocumentArray(string targetDocumentName, DocumentAttribute docAttribute, BsonValue attributeValue, DocumentAttribute id)
        {
            var arrayDocument = new Document
            {
                DocumentName = targetDocumentName,
            };
            var array = attributeValue.AsBsonArray;
            foreach (var bsonValue in array)
            {
                var bsonDocument = bsonValue.AsBsonDocument;
                var document = new Document
                {
                    DocumentName = arrayDocument.DocumentName,
                };
                document.Attributes.Add(id);
                BuildAttributes(bsonDocument, document);
                docAttribute.Attributes = document.Attributes;
                docAttribute.SubDocuments.Add(document);
            }
            return arrayDocument;
        }

        private static Document CreateDocumentAttribute(string targetDocumentName, DocumentAttribute docAttribute, BsonValue attributeValue, DocumentAttribute id)
        {
            var document = new Document
            {
                DocumentName = targetDocumentName,
            };
            document.Attributes.Add(id);
            BuildAttributes(attributeValue.AsBsonDocument, document);
            docAttribute.Attributes = document.Attributes;
            return document;
        }

        private static Dictionary<string, int> GetMaxSizeOfDocumentAttributes(List<Document> records)
        {
            var dict = new Dictionary<string, int>();
            foreach (var rec in records)
                FillMaxSizeOfDocumentAttributes(rec.Attributes, dict);
            return dict;
        }

        private static void FillMaxSizeOfDocumentAttributes(IList<DocumentAttribute> attributes, Dictionary<string, int> dict)
        {
            foreach (var attr in attributes)
            {
                if (attr == null || attr.Document == null) continue;

                var name = $"{attr.Document.DocumentName}____{attr.Name}";
                if (dict.TryGetValue(name, out int size))
                {
                    if (attr?.Value?.Length > size)
                        dict[name] = attr.Value.Length;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(attr.Value))
                    {
                        var currentSize = attr.Value.Length;
                        dict.Add(name, currentSize);
                    }
                }

                if (attr?.Attributes?.Count > 0)
                    FillMaxSizeOfDocumentAttributes(attr.Attributes, dict);

                if (attr != null && !attr.IsPrimitive && attr?.Attributes == null && attr?.Document.Attributes.Count > 0)
                    FillMaxSizeOfDocumentAttributes(attr.Document.Attributes, dict);

                if (attr != null && attr.SubDocuments.Any())
                {
                    foreach (var document in attr.SubDocuments)
                        FillMaxSizeOfDocumentAttributes(document.Attributes, dict);
                }
            }
        }

        private static void SetAttributeTypeAndSize(Dictionary<string, int> dict, IList<DocumentAttribute> attributes)
        {
            if (dict == null || dict.Count == 0 || attributes == null || attributes.Count == 0) return;

            foreach (var attr in attributes)
            {
                if (attr == null || string.IsNullOrWhiteSpace(attr.Type) || attr.Document == null) continue;

                if (attr.Type.Contains("VARCHAR"))
                {
                    if (dict.TryGetValue($"{attr.Document.DocumentName}____{attr.Name}", out int columnSize))
                        attr.Type = $"VARCHAR({columnSize})";
                }

                if (attr.Attributes != null && attr.Attributes.Count > 0)
                    SetAttributeTypeAndSize(dict, attr.Attributes);

                if (!attr.IsPrimitive && attr?.Attributes == null && attr?.Document.Attributes.Count > 0)
                    SetAttributeTypeAndSize(dict, attr.Document.Attributes);

                if (attr != null && attr.SubDocuments.Any())
                {
                    foreach (var document in attr.SubDocuments)
                        SetAttributeTypeAndSize(dict, document.Attributes);
                }
            }
        }

        private static string NormalizeColumnName(string columnName) => columnName.Replace("group", "_group"); // TODO - Improve reserved key words to avoid problems in SQL column names
    }
}