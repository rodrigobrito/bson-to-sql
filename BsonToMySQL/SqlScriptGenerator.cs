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
            var sb = new StringBuilder();
            foreach (var group in tupleGroups)
            {
                var insertCommand = CreateInsertScript(group.Name, group.Tuples);
                sb.AppendLine(insertCommand);
            }
            return sb.ToString();
        }

        private static Dictionary<string, Dictionary<string, string>> ExtractTablesAndColumns(IList<TupleGroup> groups)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var group in groups)
            {
                foreach (var tuple in group.Tuples)
                {
                    if (dict.TryGetValue(group.Name, out Dictionary<string, string>? columns))
                    {
                        foreach (var column in tuple.ColumnValues)
                        {
                            if (!columns.TryGetValue(column.Name, out string? colType))
                            {
                                columns.Add(column.Name, column.Type);
                            }
                        }
                    }
                    else
                    {
                        var cols = new Dictionary<string, string>();
                        foreach (var column in tuple.ColumnValues)
                        {
                            cols.Add(column.Name, column.Type);
                        }
                        dict.Add(group.Name, cols);
                    }
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

        private static string CreateInsertScript(string name, IList<Tuple> tupleGroups)
        {
            var sb = new StringBuilder();
            foreach (var tuple in tupleGroups)
            {
                sb.Append($"INSERT INTO {name} (");
                bool columAlreadyAdded = false;
                var comma = string.Empty;
                foreach (var column in tuple.ColumnValues)
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
                foreach (var column in tuple.ColumnValues)
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
