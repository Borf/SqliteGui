using ConGui;
using ConGui.Util;
using SqliteGui;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;

Gui.CreateContext();


Database database = new Database("database.db");
string SelectedTable = string.Empty;
List<Column> SelectedTableStructure = new();
List<List<object?>> SelectedTableData = new();
int SelectedTableBrowsePage = 0;
int SelectedTablePageCount = 0;

string text = "Hello";

while (true)
{
    Gui.BeginFrame();

    Gui.Begin("#main", WindowFlags.TopWindow | WindowFlags.HasMenu);
    {
        Gui.BeginMenuBar();
        if (Gui.BeginMenu("File"))
        {
            if (Gui.MenuItem("Open"))
                Debug.WriteLine("Open clicked");
            if (Gui.MenuItem("Save"))
                Debug.WriteLine("Save clicked");
            if (Gui.MenuItem("Quit"))
                Environment.Exit(0);
            Gui.EndMenu();
        }

        if (Gui.BeginMenu("View"))
        {
            Gui.EndMenu();
        }

        if (Gui.BeginMenu("Options"))
        {
            Gui.EndMenu();
        }
        Gui.EndMenuBar();


        Gui.Split("Split", true, 30);
        {
            Gui.BeginList("Tables");
            {
                foreach (var table in database.Tables)
                {
                    if (Gui.ListEntry(table, ListChangedEvent.OnSelect))
                    {
                        SetTable(table);
                    }
                }
            }
            Gui.EndList();
        }
        Gui.NextSplit();
        {
            if (string.IsNullOrEmpty(SelectedTable))
            {
                Gui.SetNextTextColor(Style.DangerText);
                Gui.Text("No table selected");
            }
            else
            {
                Gui.BeginTabPanel("Tabs");

                if (Gui.BeginTab("Structure"))
                {
                    Gui.Text("Name: ");
                    int column = 0;
                    foreach (Column field in SelectedTableStructure) 
                    {
                        Gui.PushId((column++) + "");
                        Gui.SetNextWidth(2);
                        Gui.Text(column + "");

                        Gui.SameLine();
                        Gui.SetNextWidth(15);
                        Gui.InputText("#name", false, ref field.ColumnName);

                        Gui.SameLine();
                        Gui.SetNextWidth(10);
                        Gui.InputText("#type", false, ref field.DataType);

                        Gui.PopId();
                    }

                    if(Gui.Button("Add column", true))
                    {
                        SelectedTableStructure.Add(new Column { ColumnName = "", DataType = "TEXT" });
                    }

                    Gui.EndTab();
                }
                if (Gui.BeginTab("Browse"))
                {
                    int colWidth = 20;
                    Gui.PushId("Header");
                    foreach (Column field in SelectedTableStructure)
                    {
                        if(field != SelectedTableStructure.First())
                            Gui.SameLine();
                        Gui.SetNextWidth(colWidth);
                        Gui.Text(field.ColumnName);
                    }
                    Gui.PopId();

                    int rowId = 0;
                    foreach(var row in SelectedTableData)
                    {
                        Gui.PushId("row" + rowId++);
                        int col = 0;
                        for (int i = 0; i < row.Count; i++)
                        {
                            object? cell = row[i];
                            Gui.SetNextWidth(colWidth);
                            if (i > 0)
                                Gui.SameLine();
                            if (cell is string)
                            {
                                string value = (string)cell;
                                if (Gui.InputText("#" + col++, false, ref value))
                                    cell = (object?)value;
                            }
                            else if (cell is int || cell is long)
                            {
                                Gui.Text(((long)cell) + "");
                            }
                            else
                            {
                                Gui.Text(cell.GetType().ToString());
                            }
                        }

                        Gui.PopId();
                    }
                    Gui.Text("");

                    if(SelectedTablePageCount > 0)
                    {
                        if(Gui.Button("Prev", true))
                        {
                            SetPage(SelectedTableBrowsePage - 1);
                        }
                        Gui.SameLine();
                        Gui.Text($"Page {SelectedTableBrowsePage+1} of {SelectedTablePageCount+1}");
                        Gui.SameLine();
                        if(Gui.Button("Next", true))
                        {
                            SetPage(SelectedTableBrowsePage + 1);
                        }
                    }

                    Gui.EndTab();
                }
                if (Gui.BeginTab("Search"))
                {

                    Gui.EndTab();
                }



                Gui.EndTabPanel();
            }
        }
        Gui.EndSplit();
    }
    Gui.End();


    Gui.Render();

    await Task.Delay(1);
}

void SetTable(string table)
{
    SelectedTable = table;
    SelectedTableStructure = database.GetTableStructure(SelectedTable);
    SelectedTableBrowsePage = 0;
    SelectedTableData = database.RefreshTableData(table, 0);
    SelectedTablePageCount = database.PageCount(table);
}

void SetPage(int newPage)
{
    SelectedTableBrowsePage = Math.Clamp(newPage, 0, SelectedTablePageCount);
    SelectedTableData = database.RefreshTableData(SelectedTable, SelectedTableBrowsePage);

}