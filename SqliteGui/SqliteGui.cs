using ConGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;
public partial class SqliteGui : App
{
    Database database = new Database("database.db");
    string SelectedTable = string.Empty;
    List<Column> SelectedTableStructure = new();
    List<List<object?>> SelectedTableData = new();
    int SelectedTableBrowsePage = 0;
    int SelectedTableResultCount = 0;
    bool showSql = false;

    public async Task Run()
    {
        while (true)
        {
            Gui.BeginFrame();
            Gui.Begin("#main", WindowFlags.TopWindow | WindowFlags.HasMenu);
            {

                BuildMenu();

                Gui.Split("Split", true, 30);
                {
                    BuildTableList();
                }
                Gui.NextSplit();
                {
                    if (string.IsNullOrEmpty(SelectedTable))
                    {
                        BuildHome();
                    }
                    else
                    {
                        if (showSql)
                            Gui.Split("Split", false, Console.WindowHeight - 10);
                        Gui.BeginTabPanel("Tabs");

                        BuildTabstructure();
                        BuildTabBrowse();
                        BuildTabSearch();
                        BuildTabOperations();

                        Gui.EndTabPanel();

                        if (showSql)
                        {
                            Gui.NextSplit();
                            Gui.Text(string.Join("\n", database.SqlLog.Split("\n").TakeLast(6)));
                            Gui.EndSplit();
                        }
                    }
                }
                Gui.EndSplit();
            }
            Gui.End();


            Gui.Render();

            await Task.Delay(1);
        }
    }



    void SetTable(string table)
    {
        SelectedTable = table;
        if (!string.IsNullOrEmpty(SelectedTable))
        {
            SelectedTableStructure = database.GetTableStructure(SelectedTable);
            SelectedTableBrowsePage = 0;
            SelectedTableData = database.RefreshTableData(table, 0);
            SelectedTableResultCount = database.ResultCount(table);
        }
    }

    void SetPage(int newPage)
    {
        SelectedTableBrowsePage = Math.Clamp(newPage, 0, SelectedTableResultCount/20);
        SelectedTableData = database.RefreshTableData(SelectedTable, SelectedTableBrowsePage);

    }
}
