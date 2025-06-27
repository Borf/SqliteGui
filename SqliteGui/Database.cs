using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqliteGui;

public class Database : IDisposable
{
    private SqliteConnection connection;

    public string SqlLog { private set; get; }

    public List<string> Tables { get; set; } = new();

    public Database(string fileName)
    {
        connection = new SqliteConnection($"Data Source='{fileName}'");
        connection.Open();
        RefreshTables();

        if(!Tables.Contains("_gui_tablename"))
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"CREATE TABLE _gui_tablename (TableName	TEXT NOT NULL,	NameColumn	TEXT NOT NULL,	PRIMARY KEY(TableName))";
            command.ExecuteNonQuery();
        }
    }

    private void ReadQuery(string query, Action<SqliteDataReader> action)
    {
        SqlLog += query + "\n";
        using var command = connection.CreateCommand();
        command.CommandText = query;
        using var reader = command.ExecuteReader();
        action.Invoke(reader);
    }
    private void RunQuery(string query)
    {
        SqlLog += query + "\n";
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();
    }


    public void RefreshTables()
    {
        ReadQuery("SELECT name FROM sqlite_master WHERE type='table'", reader =>
        {
            Tables = new();
            while (reader.Read())
            {
                Tables.Add(reader.GetString(0));
            }
            Tables = Tables.Order().ToList();
        });
    }


    public void Dispose()
    {
        connection.Dispose();
    }

    public List<List<object?>> RefreshTableData(string tableName, int offset)
    {
        List<List<object?>> data = new();
        ReadQuery($"SELECT * FROM {tableName} LIMIT {offset * 20},20", reader =>
        {
            while (reader.Read())
            {
                List<object?> row = new();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetValue(i));
                }
                data.Add(row);
            }
        });
        return data;
    }

    public string GetCreateTable(string selectedTable)
    {
        string query = string.Empty;
        ReadQuery("SELECT sql FROM sqlite_master WHERE type='table' and name='" + selectedTable + "'", reader =>
        {
            if (reader.Read())
                query = reader.GetString(0);
        });
        return query;
    }

    public List<TableRelation> GetTableRelations(string tableName)
    {
        var relations = new List<TableRelation>();
        string createTableSql = GetCreateTable(tableName);

        if (!string.IsNullOrEmpty(createTableSql))
        {
            var foreignKeyMatches = Regex.Matches(createTableSql, @"FOREIGN KEY\s*\(([^)]+)\)\s*REFERENCES\s*([^\(]+)\s*\(([^)]+)\)(.*)", RegexOptions.IgnoreCase);
            foreach (Match match in foreignKeyMatches)
            {
                if (match.Groups.Count > 4)
                {
                    var relation = new TableRelation
                    {
                        Field = match.Groups[1].Value.Trim(),
                        ReferencedTable = match.Groups[2].Value.Trim(),
                        ReferencedField = match.Groups[3].Value.Trim(),
                        CascadingEffect = match.Groups[4].Value.Trim()
                    };
                    relations.Add(relation);
                }
            }
        }

        return relations;
    }

    public List<Column> GetTableStructure(string tableName)
    {
        if (tableName == "")
            return new();

        List<Column> ret = new();
        ReadQuery($"PRAGMA table_info({tableName});", reader =>
        {
            while (reader.Read())
            {
                if (!Enum.TryParse<DataType>(reader.GetString(2), out DataType dataType))
                    dataType = DataType.UNKNOWN;

                ret.Add(new Column()
                {
                    ColumnName = reader.GetString(1),
                    OriginalColumnName = reader.GetString(1),
                    DataType = dataType,
                    AllowDBNull = reader.GetInt32(3) == 0,
                    DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsKey = reader.GetInt32(5) != 0,
                });
            }
        });

        //seq, name, unique, origin, partial
        ReadQuery($"PRAGMA index_list({tableName});", reader =>
        {
            while (reader.Read())
            {
                string indexName = reader.GetString(1);
                using (var command2 = connection.CreateCommand())
                {
                    //seqno, cid, name
                    command2.CommandText = $"PRAGMA index_info({indexName});";
                    using var reader2 = command2.ExecuteReader();
                    reader2.Read(); //should be 1 result, but can verify that the reader2.cid == column.cid (but column.cid is not stored)
                    var col = ret.First(c => c.ColumnName == reader2.GetString(2));
                    col.IsUnique = reader.GetInt32(2) != 0;
                }
            }
        });
        //seq, name, unique, origin, partial
        ReadQuery(@$" select ""is-autoincrement"" from sqlite_master where tbl_name=""{tableName}"" and SQL like '%AUTOINCREMENT%';", reader =>
        {
            if (reader.Read())
                if (reader.HasRows)
                    ret.First(r => r.IsKey).IsAutoIncrement = true;
        });
        return ret;
    }

    public List<DbIndex> GetIndices(string tableName)
    {
        var indices = new List<DbIndex>();
        if (tableName == "")
            return indices;
        ReadQuery("SELECT * FROM sqlite_master WHERE type='index' AND tbl_name = '" + tableName + "' LIMIT 1", reader =>
        {
            while (reader.Read())
            {
                indices.Add(new DbIndex(reader.IsDBNull(4) ? string.Empty : reader.GetString(4))
                {
                    Name = reader.GetString(1),
                    Table = reader.GetString(2),
                });
            }
        });
        return indices;
    }

    public int PageCount(string table)
    {
        int count = 0;
        ReadQuery($"SELECT count(*) FROM {table}", reader =>
        {
            if (reader.Read())
            {
                count = reader.GetInt32(0) / 20;
            }
        });
        return count;
    }

    public void RunQueries(string queries)
    {
        RunQuery(queries);
    }
}

public class DbIndex
{
    public required string Name { get; set; }
    public required string Table { get; set; }
    public string Sql { get; set; }

    public string Field { get; private set; } = string.Empty;
    public DbIndex(string Sql)
    {
        this.Sql = Sql;
        var match = Regex.Match(Sql.Replace("\r", "").Replace("\n", ""), @"CREATE INDEX ""(.*)"" ON ""(.*)"" \((.*)\)");
        if (match.Success)
        {
            var fields = match.Groups[3].Value;
            fields = fields[1..(fields.Length - 1)];
            Field = string.Join(", ", fields.Split("\", \""));
        }
        else
            Field = "ERROR";
    }
}


public class TableRelation
{
    public string Field { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedField { get; set; } = string.Empty;
    public string CascadingEffect { get; set; } = string.Empty;
}


public class Column
{ //Not properties, because they need to be passed as ref
    public required string ColumnName = string.Empty;
    public required string OriginalColumnName = string.Empty;
    public required string? DefaultValue = string.Empty;
    public required DataType DataType = DataType.UNKNOWN;
    public required bool AllowDBNull;
    public required bool IsKey;
    public bool IsAutoIncrement = false;
    public bool IsUnique = false;
}

public enum DataType
{
    TEXT,
    INTEGER,
    BLOB,
    REAL,
    NUMERIC,
    UNKNOWN,
}