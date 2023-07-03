using System.Text;

namespace BsonToMySQL
{
    public class SqlScriptGenerator
    {
        public static string CreateDDL(IList<TupleGroup> tuplesGroups)
        {
            if (tuplesGroups == null || tuplesGroups.Count == 0)
                throw new ArgumentNullException(nameof(tuplesGroups));

            return CreateTable(tuplesGroups);
        }

        public static string CreateDML(IList<TupleGroup> tupleGroups)
        {
            return CreateInsertDMLCommand(tupleGroups);
        }

        private static Dictionary<string, Dictionary<string, string>> ExtractTablesAndColumns(IList<TupleGroup> tuplesGroups)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var tupleGroup in tuplesGroups)
            {
                if (dict.TryGetValue(tupleGroup.Name, out Dictionary<string, string>? columns))
                {
                    foreach (var tuple in tupleGroup.TupleColumnValues)
                    {
                        if (!columns.TryGetValue(tuple.Name, out string? colType))
                        {
                            columns.Add(tuple.Name, tuple.Type);
                        }
                    }
                }
                else
                {
                    var cols = new Dictionary<string, string>();
                    foreach (var tuple in tupleGroup.TupleColumnValues)
                    {
                        cols.Add(tuple.Name, tuple.Type);
                    }
                    dict.Add(tupleGroup.Name, cols);
                }
            }
            return dict;
        }

        private static string CreateTable(IList<TupleGroup> tuplesGroups)
        {
            var sbTable = new StringBuilder();
            var tablesAndColumnsDict = ExtractTablesAndColumns(tuplesGroups);
            foreach (var table in tablesAndColumnsDict)
            {
                sbTable.AppendLine($"CREATE TABLE IF NOT EXISTS {table.Key} (");
                var sbColumns = CreateColumns(table.Value);
                sbTable.Append(sbColumns);
                sbTable.AppendLine(");");
                sbTable.AppendLine();
            }
            return sbTable.ToString();
        }

        private static StringBuilder CreateColumns(Dictionary<string, string> columns)
        {
            if (columns == null || columns.Count == 0)
                throw new ArgumentNullException(nameof(columns));

            var sbColumns = new StringBuilder();

            var comma = " ";
            foreach (var column in columns)
            {
                if (sbColumns.Length > 0) comma = ",";
                var columnDefault = column.Key == "_id" ? string.Empty : "DEFAULT NULL";
                sbColumns.AppendLine($"  {comma}{column.Key} {column.Value} {columnDefault}");
            }
            return sbColumns;
        }

        private static string CreateInsertDMLCommand(IList<TupleGroup> tupleGroups)
        {
            var sb = new StringBuilder();
            foreach (var tuple in tupleGroups)
            {
                sb.Append($"INSERT INTO {tuple.Name} (");
                bool columAlreadyAdded = false;
                var comma = string.Empty;
                foreach (var column in tuple.TupleColumnValues)
                {
                    if (columAlreadyAdded) comma = ", ";
                    sb.Append($"{comma}{column.Name.ToLower()}");
                    columAlreadyAdded = true;
                }
                sb.Append(')');
                sb.AppendLine($" VALUES ");
                sb.Append('(');
                comma = " ";
                columAlreadyAdded = false;
                foreach (var column in tuple.TupleColumnValues)
                {
                    if (columAlreadyAdded) comma = ",";

                    if (string.IsNullOrWhiteSpace(column.Value) || column.Value == "NaN")
                    {
                        sb.Append($"{comma} NULL");
                    }
                    else
                    {
                        if (column.Type != null && column.Type.Contains("VARCHAR"))
                            sb.Append($"{comma} '{EscapeString(column.Value)}'");
                        else
                            sb.Append($"{comma} {column.Value}");
                    }

                    columAlreadyAdded = true;
                }
                sb.AppendLine(");");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string EscapeString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return EscapeLiteral(EscapeIdentifier(EscapeSqlLiteral(str)));
        }
        private static string EscapeSqlLiteral(string literal) => literal.Replace("'", "''");
        private static string EscapeIdentifier(string identifier) => identifier.Replace("`", "``");
        private static string EscapeLiteral(string s) => s.Replace("\"", "\"\"");
    }
}
