namespace BsonToMySQL
{
    public class Program
    {
        public const string ExpectedArgumentMessage = "Invalid argument, expected: -f filename -t tableNamePrefix";
        static void Main(string[] args)
        {           
            if (args.Length != 4 ||
                string.IsNullOrWhiteSpace(args[0]) || args[0].ToLower() != "-f" ||
                string.IsNullOrWhiteSpace(args[1]) ||
                string.IsNullOrWhiteSpace(args[2]) || args[2].ToLower() != "-t" ||
                string.IsNullOrWhiteSpace(args[3]))
            {
                Console.WriteLine(ExpectedArgumentMessage);
                return;
            }    
            FileManager.ExtractDataAndBuildSqlFile(args[1], args[3]);                  
        }
    }
}