using MongoDB.Bson;
using System.Text;

namespace BsonToMySQL
{
    public class BsonExtractor
    {
        public static void ExtractDataAndBuildSqlFile(string fileName, string targetTable)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File {fileName} not found!");
                Console.ReadLine();
                return;
            }

            var array = ToBsonArray(fileName);
            if (array == null)
            {
                Console.WriteLine($"No data found in file!");
                Console.ReadLine();
                return;
            }

            var records = new List<GenericRecord>();
            foreach (var bsonValue in array)
            {
                var bsonDocument = bsonValue.AsBsonDocument;
                var rec = new GenericRecord { TableName = targetTable };
                BuildAttributes(bsonDocument, rec);
                records.Add(rec);
            }

            var dict = GetColumnDict(records);
            foreach (var rec in records)
                UpdateColumnSizes(rec.Attributes, dict);

            var ddl = string.Empty;
            for (var i = 0; i < records.Count; i++)
                records[i].BuidDDL();

            ddl = GenericRecord.GetDDLs();

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(ddl);
            for (var i = 0; i < records.Count; i++)
                sqlBuilder.AppendLine(records[i].BuildDML());
            
            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory)) return;

            var path = Path.Combine(directory, $"{targetTable}.sql");
            var sql = sqlBuilder.ToString();

            File.WriteAllText(path, sql);
            Console.WriteLine($"SQL extraction from bson file {fileName} finished!");
            Console.ReadLine();
        }

        private static void UpdateColumnSizes(IList<GenericAttribute> rows, Dictionary<string, int> dict)
        {
            if (rows == null || rows.Count == 0 || dict == null || dict.Count == 0) return;

            foreach (var attr in rows)
            {
                if (attr == null || string.IsNullOrWhiteSpace(attr.Type) || attr.Table == null) continue;

                if (attr.Type.Contains("VARCHAR"))
                {
                    if (dict.TryGetValue($"{attr.Table.TableName}____{attr.Name}", out int columnSize))
                        attr.Type = $"VARCHAR({columnSize})";
                }

                if (attr.Rows != null && attr.Rows.Count > 0)
                    UpdateColumnSizes(attr.Rows, dict);

                if (!attr.IsPrimitive && attr?.Rows == null && attr?.Table.Attributes.Count > 0)
                    UpdateColumnSizes(attr.Table.Attributes, dict);
            }
        }

        private static Dictionary<string, int> GetColumnDict(List<GenericRecord> records)
        {
            var dict = new Dictionary<string, int>();
            foreach (var rec in records)
                PopulateDictWithColumnSizes(rec.Attributes, dict);
            return dict;

        }

        private static void PopulateDictWithColumnSizes(IList<GenericAttribute> rows, Dictionary<string, int> dictColumnSize)
        {
            foreach (var attr in rows)
            {
                if (attr == null || attr.Table == null) continue;

                var name = $"{attr.Table.TableName}____{attr.Name}";
                if (dictColumnSize.TryGetValue(name, out int size))
                {
                    if (attr?.Value?.Length > size)
                        dictColumnSize[name] = attr.Value.Length;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(attr.Value))
                    {
                        var currentSize = attr.Value.Length;
                        dictColumnSize.Add(name, currentSize);
                    }
                }

                if (attr?.Rows?.Count > 0)
                    PopulateDictWithColumnSizes(attr.Rows, dictColumnSize);

                if (!attr.IsPrimitive && attr?.Rows == null && attr?.Table.Attributes.Count > 0)
                    PopulateDictWithColumnSizes(attr.Table.Attributes, dictColumnSize);
            }
        }

        private static BsonArray? ToBsonArray(string fileName)
        {
            var bsonString = File.ReadAllText(fileName);
            if (bsonString.StartsWith("{"))
            {
                bsonString = "{ \"items\": [" + bsonString + "] }";
                var document = BsonDocument.Parse(bsonString);
                var array = document["items"].AsBsonArray;
                return array;
            }
            return null;
        }

        private static void BuildAttributes(BsonDocument document, GenericRecord targetTable, GenericRecord? parentTable = null)
        {
            if (parentTable != null)
            {
                var idAttribute = parentTable.Attributes.FirstOrDefault(attr => attr.Name == "_id");
                if (idAttribute != null)
                    targetTable.Attributes.Insert(0, idAttribute);
            }

            foreach (var element in document)
            {
                var attr = new GenericAttribute
                {
                    Name = element.Name.Replace("group", "_group"), // TODO - Adjust reserved key words
                    Table = targetTable
                };

                var presumableTableName = $"{targetTable.TableName}_{attr.Name}";
                var attributeValue = element.Value;
                switch (attributeValue.BsonType)
                {
                    case BsonType.ObjectId:
                        attr.Value = attributeValue.AsObjectId.ToString();
                        break;
                    case BsonType.Int64:
                        attr.Value = attributeValue.AsInt64.ToString();
                        attr.Type = "INT";
                        break;
                    case BsonType.Int32:
                        attr.Value = attributeValue.AsInt32.ToString();
                        attr.Type = "INT";
                        break;
                    case BsonType.Decimal128:
                        attr.Value = attributeValue.AsDecimal128.ToString();
                        attr.Type = "DECIMAL(10, 2)";
                        break;
                    case BsonType.Double:
                        attr.Value = attributeValue.AsDouble.ToString();
                        attr.Type = "DECIMAL(10, 2)";
                        break;
                    case BsonType.Document:
                        var dTable = new GenericRecord
                        {
                            TableName = presumableTableName,
                        };
                        BuildAttributes(attributeValue.AsBsonDocument, dTable, targetTable);
                        attr.Table = dTable;
                        attr.Type = "OBJECT";
                        break;
                    case BsonType.Array:
                        var arrayTable = new GenericRecord
                        {
                            TableName = presumableTableName,
                        };
                        var array = attributeValue.AsBsonArray;
                        foreach (var bsonValue in array)
                        {
                            var bsonDocument = bsonValue.AsBsonDocument;
                            var table = new GenericRecord
                            {
                                TableName = arrayTable.TableName,
                            };
                            BuildAttributes(bsonDocument, table, targetTable);
                            attr.Rows = new List<GenericAttribute>(table.Attributes);
                        }
                        attr.Table = arrayTable;
                        attr.Type = "OBJECT";
                        break;
                    case BsonType.Boolean:
                        attr.Value = attributeValue.AsBoolean.ToString();
                        attr.Type = "TINYINT";
                        break;
                    case BsonType.DateTime:
                        if (attributeValue != null)
                        {
                            attr.Value = attributeValue.ToUniversalTime().ToString("u").Replace(" ", "T"); ;
                            attr.Type = "VARCHAR";
                        }
                        break;
                    case BsonType.Null:
                        break;
                    default:
                        attr.Value = attributeValue.AsString;
                        break;
                }
                targetTable.Attributes.Add(attr);
            }
        }
    }
}
