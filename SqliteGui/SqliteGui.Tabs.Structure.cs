using ConGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{
    public void BuildTabstructure()
    {
        if (Gui.BeginTab("Structure"))
        {
            Gui.SetNextWidth(2);
            Gui.Text("Id");

            Gui.SameLine();
            Gui.SetNextWidth(15);
            Gui.Text("Name: ");

            Gui.SameLine();
            Gui.SetNextWidth(15);
            Gui.Text("Type: ");

            Gui.SameLine();
            Gui.SetNextWidth(15);
            Gui.Text("Default: ");

            Gui.SameLine();
            Gui.SetNextWidth(5);
            Gui.Text("Null");

            Gui.SameLine();
            Gui.SetNextWidth(5);
            Gui.Text("Key");
            
            Gui.SameLine();
            Gui.SetNextWidth(5);
            Gui.Text("Uni");

            Gui.SameLine();
            Gui.SetNextWidth(5);
            Gui.Text("AA");

            int column = 0;
            foreach (Column field in SelectedTableStructure.ToList())
            {
                Gui.PushId((column++) + "");
                Gui.SetNextWidth(2);
                Gui.Text(column + "");

                Gui.SameLine();
                Gui.SetNextWidth(15);
                Gui.InputText("#name", false, ref field.ColumnName);

                Gui.SameLine();
                Gui.SetNextWidth(15);
                if(Gui.BeginComboBox("#type", field.DataType.ToString()))
                {
                    foreach (var t in Enum.GetValues<DataType>())
                        if (Gui.ComboBoxEntry(t.ToString(), field.DataType == t))
                            field.DataType = t;
                    Gui.EndComboBox();
                }


                Gui.SameLine();
                Gui.SetNextWidth(5);
                if(Gui.CheckBox("#def", field.DefaultValue == null))
                    field.DefaultValue = field.DefaultValue == null ? string.Empty : null;

                Gui.SameLine();
                Gui.SetNextWidth(10);
                if (field.DefaultValue != null)
                    Gui.InputText("#default", false, ref field.DefaultValue);
                else
                    Gui.Text("NULL");

                Gui.SameLine();
                Gui.SetNextWidth(5);
                Gui.CheckBox("#Null", ref field.AllowDBNull);

                Gui.SameLine();
                Gui.SetNextWidth(5);
                Gui.CheckBox("#Key", ref field.IsKey);

                Gui.SameLine();
                Gui.SetNextWidth(5);
                Gui.CheckBox("#Unique", ref field.IsUnique);

                Gui.SameLine();
                Gui.SetNextWidth(5);
                Gui.CheckBox("#AA", ref field.IsAutoIncrement);

                Gui.SameLine();
                Gui.SetNextWidth(5);
                if (column == 1)
                    Gui.SetNextBackgroundColor(Style.Dark);
                if(Gui.Button("↑", false))
                {

                }

                Gui.SameLine();
                Gui.SetNextWidth(5);
                if (column == SelectedTableStructure.Count)
                    Gui.SetNextBackgroundColor(Style.Dark);
                if (Gui.Button("↓", false))
                {
                }

                Gui.SameLine();
                Gui.SetNextWidth(5);
                Gui.SetNextBackgroundColor(Style.Danger);
                if (Gui.Button("X", false))
                {
                    if(SelectedTableStructure.Count > 2)
                        SelectedTableStructure.Remove(field);
                }

                Gui.PopId();
            }
            Gui.NewLine();
            if (Gui.Button("Add column", true))
            {
                SelectedTableStructure.Add(new Column { ColumnName = "", OriginalColumnName = string.Empty, DataType = DataType.TEXT, DefaultValue = "", AllowDBNull = false, IsKey = false });
            }
            Gui.SameLine();
            Gui.SetNextBackgroundColor(Style.Danger);
            if (Gui.Button("Apply Changes", true))
                ApplyStructureChanges();


            Gui.EndTab();
        }
    }





    public void ApplyStructureChanges()
    {
        Gui.Progress("Updating table structure");
        Task.Run(() =>
        {
            var originalFields = database.GetTableStructure(SelectedTable);
            var selectedFields = SelectedTableStructure;

            var tempName = "TEMP_" + DateTimeOffset.Now.ToUnixTimeSeconds();

            string queries = string.Empty;

            queries += "BEGIN TRANSACTION;\n";

            queries += $"CREATE TABLE \"{tempName}\" (\n";
            foreach (var col in selectedFields)
            {
                queries += $"    {col.ColumnName} {col.DataType.ToString().ToUpper()}";

                queries += col.AllowDBNull ? " NULL" : " NOT NULL";
                if (!string.IsNullOrEmpty(col.DefaultValue))
                    if (col.DataType == DataType.INTEGER || col.DataType == DataType.NUMERIC || col.DataType == DataType.REAL)
                        queries += $" DEFAULT {col.DefaultValue}";
                    else
                        queries += $" DEFAULT \"{col.DefaultValue}\""; //TODO: should these always have quotes?
                if (col.IsUnique)
                    queries += " UNIQUE"; //TODO: test this
                queries += ",\n";
            }
            var keys = selectedFields.Where(c => c.IsKey).ToList();
            if (keys.Count == 1)
            {
                queries += $"    PRIMARY KEY(\"{keys.First().ColumnName}\" {(keys.First().IsAutoIncrement ? "AUTOINCREMENT" : "")})\n";
            }
            else if (keys.Count > 1)
            {
                queries += $"    PRIMARY KEY(\"";
                foreach (var key in keys)
                    queries += $"{key.ColumnName}\" {(key.IsAutoIncrement ? "AUTOINCREMENT" : "")}, ";
                queries = queries[0..^2] + "\n";
                queries += $";)\n";
            }
            else
                queries = queries[0..^2] + "\n";


            queries += $");\n";

            List<(string, string)> mapping = new();
            foreach (var col in selectedFields)
            {
                var original = originalFields.FirstOrDefault(c => c.OriginalColumnName == col.OriginalColumnName);
                if (original != null)
                    mapping.Add((col.ColumnName, original.ColumnName));
            }

            queries += $"INSERT INTO \"{tempName}\"\n" +
                $"(";
            queries += string.Join(", ", mapping.Select(m => $"\"{m.Item1}\""));
            queries += ")\n" +
                "SELECT ";
            queries += string.Join(", ", mapping.Select(m => $"\"{m.Item2}\""));
            queries += $" FROM \"{SelectedTable}\";\n";

            queries += $"DROP TABLE \"{SelectedTable}\";\n";
            queries += $"ALTER TABLE \"{tempName}\" RENAME TO \"{SelectedTable}\";\n";

            queries += "END TRANSACTION;\n";

            database.RunQueries(queries);

            Debug.WriteLine(queries);

            database.RefreshTables();
            selectedFields = database.GetTableStructure(SelectedTable);

            Gui.ProgressDone();
        });
    }

}
