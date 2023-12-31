﻿using MongoDB.Bson;
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
                return;
            }

            Console.WriteLine($"Loading bson data from {fileName}");
            var array = ToBsonArray(fileName);
            if (array == null)
            {
                Console.WriteLine($"No data found in file!");
                return;
            }

            Console.WriteLine("Creating document-oriented models...");
            var documents = DocumentFactory.Create(array, targetTable);
            Console.WriteLine("Translating document-oriented models to relational models...");
            var tupleGroups = TupleFactory.Create(documents);
            Console.WriteLine("Creating DDL scripts...");
            var ddl = SqlScriptGenerator.CreateDDL(tupleGroups);
            Console.WriteLine("Creating DML scripts...");
            var dml = SqlScriptGenerator.CreateDML(tupleGroups);

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine(ddl);
            sqlBuilder.AppendLine(dml);

            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory)) return;

            var path = Path.Combine(directory, $"{targetTable}.sql");
            var sql = sqlBuilder.ToString();

            Console.WriteLine($"Saving SQL scripts to {path}.");
            File.WriteAllText(path, sql);      
            Console.WriteLine($"SQL file created on {path}.");
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
