﻿namespace BsonToMySQL
{
    public class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = new[] { @"C:\Users\rodri\OneDrive\Desenhos\teste\legacy_skus.json", @"zz__legacy_skus" };
#endif
            if (args.Length != 2) {
                Console.WriteLine("Invalid arguments!");
                return;
            }
            BsonExtractor.ExtractDataAndBuildSqlFile(args[0], args[1]);                  
        }
    }
}