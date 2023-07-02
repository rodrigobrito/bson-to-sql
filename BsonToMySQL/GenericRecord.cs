using System.Text;

namespace BsonToMySQL
{
    public class GenericRecord
    {
        private static Dictionary<string, string> CreatedTables = new Dictionary<string, string>();
        public string TableName { get; set; } = string.Empty;
        public IList<GenericAttribute> Attributes = new List<GenericAttribute>();
        public string BuidDDL()
        {
            return CreateTable(TableName, Attributes);
        }

        public static string GetDDLs()
        { 
            var sb = new StringBuilder();
            foreach (var table in CreatedTables)
                sb.AppendLine(table.Value);
               
            return sb.ToString();
        }

        public string BuildDML()
        {
            return CreateInsertDMLCommand(TableName, Attributes);
        }

        private static string CreateTable(string tableName, IList<GenericAttribute> attributes)
        {
            var sbTable = new StringBuilder();

            var tempTable = new StringBuilder();
            tempTable.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName.ToLower()}  (");
            var sbColumns = CreateColumns(tableName, attributes.Where(attr => attr.IsPrimitive).ToList());
            tempTable.Append(sbColumns);
            tempTable.AppendLine(");");
            tempTable.AppendLine();
            var tmpDDL = tempTable.ToString();

            if (!CreatedTables.ContainsKey(tableName))
                CreatedTables.Add(tableName, tmpDDL);

            if (CreatedTables[tableName].Length < tmpDDL.Length)
                CreatedTables[tableName] = tmpDDL;

            sbTable.Append(tmpDDL);

            foreach (var grouping in attributes.Where(attr => !attr.IsPrimitive).GroupBy(attr => attr.Table.TableName).ToList())
            {
                var attr = grouping?.FirstOrDefault();
                if (attr != null)
                {
                    var tableAttribute = attr.Table;
                    if (tableAttribute != null && tableAttribute.Attributes.Count > 0)
                        sbTable.Append(CreateTable(tableAttribute.TableName, tableAttribute.Attributes));

                    if (attr.Rows?.Count > 0 && !string.IsNullOrWhiteSpace(attr.Table?.TableName))
                    {
                        sbTable.Append(CreateTable(attr.Table.TableName, attr.Rows));
                    }
                }
            }

            return sbTable.ToString();
        }

        private static StringBuilder CreateColumns(string tableName, IList<GenericAttribute> attributes)
        {
            var sbColumns = new StringBuilder();

            if (attributes.Count == 0)
                return sbColumns;

            var comma = " ";
            foreach (var column in attributes)
            {
                if (sbColumns.Length > 0) comma = ",";
                var columnDefault = column.Name == "_id" ? string.Empty : "DEFAULT NULL";
                sbColumns.AppendLine($"  {comma}{column.Name.ToLower()} {column.Type} {columnDefault}");
            }
            return sbColumns;
        }

        private static string CreateInsertDMLCommand(string tableName, IList<GenericAttribute> attributes)
        {
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {tableName.ToLower()} (");
            bool columAlreadyAdded = false;
            var comma = string.Empty;
            foreach (var column in attributes.Where(attr => attr.IsPrimitive))
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
            foreach (var column in attributes.Where(attr => attr.IsPrimitive))
            {
                if (columAlreadyAdded) comma = ",";
                if (column.Type != null && column.Type.Contains("VARCHAR"))
                    sb.Append($"{comma} '{EscapeString(column.Value)}'");
                else
                    sb.Append($"{comma} {column.Value}");
                columAlreadyAdded = true;
            }
            sb.AppendLine(");");
            sb.AppendLine();

            foreach (var grouping in attributes.Where(attr => !attr.IsPrimitive).GroupBy(attr => attr.Table.TableName).ToList())
            {
                var attr = grouping?.FirstOrDefault();
                if (attr != null)
                {
                    var tableAttribute = attr.Table;
                    if (tableAttribute != null && tableAttribute.Attributes.Count > 0)
                        sb.Append(CreateInsertDMLCommand(tableAttribute.TableName, tableAttribute.Attributes));

                    if (attr.Rows?.Count > 0 && !string.IsNullOrWhiteSpace(attr.Table?.TableName))
                    {
                        sb.Append(CreateInsertDMLCommand(attr.Table.TableName, attr.Rows));
                    }
                }
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
