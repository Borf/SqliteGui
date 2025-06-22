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
    private string fileName;
    private SqliteConnection connection;

    public List<string> Tables { get; set; } = new();

    public Database(string fileName)
    {
        this.fileName = fileName;
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


    public void RefreshTables()
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
        using var reader = command.ExecuteReader();
        Tables = new();
        while (reader.Read())
        {
            Tables.Add(reader.GetString(0));
        }
    }


    public void Dispose()
    {
        connection.Dispose();
    }

    public List<List<object?>> RefreshTableData(string tableName, int offset)
    {
        List<List<object?>> data = new();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} LIMIT {offset*20},20";
        using var reader = command.ExecuteReader();
        //var columnScheme = reader.GetColumnSchema();
        //foreach (var column in columnScheme)
        //{
        //    browseTable.Columns.Add(column.ColumnName);
        //}
        while(reader.Read())
        {
            List<object?> row = new();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.GetValue(i));
            }
            data.Add(row);
        }
        return data;
    }

    public string GetCreateTable(string selectedTable)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' and name='"+selectedTable+"'";
        using var reader = command.ExecuteReader();
        if (reader.Read())
            return reader.GetString(0);
        return "";
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

        //using var command = connection.CreateCommand();
        //command.CommandText = "SELECT * FROM " + tableName + " LIMIT 1";
        //using var reader = command.ExecuteReader();
        //return reader.GetColumnSchema().Select(c => new Column
        //{
        //    ColumnName = c.ColumnName,
        //    DataType = Enum.Parse<DataType>(c.DataTypeName ?? DataType.UNKNOWN.ToString()),
        //    AllowDBNull = c.AllowDBNull ?? false,
        //    IsKey = c.IsKey ?? false,
        //    IsAutoIncrement = c.IsAutoIncrement ?? false,
        //    IsReadOnly = c.IsReadOnly ?? false,
        //    IsUnique = c.IsUnique ?? false,
        //    Size = c.ColumnSize ?? 0,
        //    DefaultValue = "", //TODO: where do I get this?
        //}).ToList();

        List<Column> ret = new();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"PRAGMA table_info({tableName});";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ret.Add(new Column()
                {
                    ColumnName = reader.GetString(1),
                    DataType = Enum.Parse<DataType>(reader.GetString(2)),
                    AllowDBNull = reader.GetInt32(3) != 0,
                    DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsKey = reader.GetInt32(5) != 0
                });
            }
        }

        using (var command = connection.CreateCommand())
        {
            //seq, name, unique, origin, partial
            command.CommandText = $"PRAGMA index_list({tableName});";
            using var reader = command.ExecuteReader();
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
        }
        using (var command = connection.CreateCommand())
        {
            //seq, name, unique, origin, partial
            command.CommandText = @$" select ""is-autoincrement"" from sqlite_master where tbl_name=""{tableName}"" and SQL like '%AUTOINCREMENT%';";
            using var reader = command.ExecuteReader();
            if (reader.Read())
                if(reader.HasRows)
                    ret.First(r => r.IsKey).IsAutoIncrement = true;
        }        
        return ret;
    }

    public List<DbIndex> GetIndices(string tableName)
    {
        var indices = new List<DbIndex>();
        if (tableName == "")
            return indices;
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM sqlite_master WHERE type='index' AND tbl_name = '" + tableName + "' LIMIT 1";
        using var reader = command.ExecuteReader();
        while(reader.Read())
        {
            indices.Add(new DbIndex(reader.IsDBNull(4) ? string.Empty : reader.GetString(4))
            {
                Name = reader.GetString(1),
                Table = reader.GetString(2),
            });
        }
        return indices;
    }

    public int PageCount(string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT count(*) FROM {table}";
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0)/20;
        }
        return 0;
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
{ //TODO: remove fields that are not used in sqlite
    public string ColumnName = string.Empty;
    public string? DefaultValue = string.Empty;
    public DataType DataType = DataType.UNKNOWN;
    public bool AllowDBNull;
    public bool IsKey;
    public bool IsAutoIncrement;
    public bool IsUnique;
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