using MongoDB.Bson;
using System.Text;

namespace BsonToMySQL
{
    public class FileManager
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

            var documents = DocumentFactory.Create(array, targetTable);
            var tupleGroups = TupleFactory.Create(documents);
            
            var ddl = SqlScriptGenerator.CreateDDL(tupleGroups);
            var dml = SqlScriptGenerator.CreateDML(tupleGroups);

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine(ddl);
            sqlBuilder.AppendLine(dml);

            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory)) return;

            var path = Path.Combine(directory, $"{targetTable}.sql");
            var sql = sqlBuilder.ToString();

            File.WriteAllText(path, sql);
            Console.WriteLine($"SQL extraction from bson file {fileName} finished!"); 
            Console.WriteLine($"SQL file created on {path}.");
            Console.WriteLine($"Press any key to finish!");
            Console.ReadLine();
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
    }
}
