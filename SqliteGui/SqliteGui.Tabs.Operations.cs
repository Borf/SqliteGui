using ConGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{
    public string NewTableName = string.Empty;
    public void BuildTabOperations()
    {
        if (Gui.BeginTab("Operations"))
        {
            Gui.SetNextWidth(Gui.CurrentSize.X - 10);
            if(Gui.InputButton("Rename Table", ref NewTableName, "Rename"))
            { 
                database.RunQueries($"ALTER TABLE \"{SelectedTable}\" RENAME TO \"{NewTableName}\";");
                database.RefreshTables();
                SelectedTable = NewTableName;
            }

            Gui.NewLine();
            Gui.SetNextBackgroundColor(Style.Danger);
            if(Gui.Button("Truncate", true))
            {
                Gui.Confirm($"Are you sure you want to delete all values in the table '{SelectedTable}'", () =>
                {
                    database.RunQueries($"TRUNCATE TABLE \"{SelectedTable}\"");
                    SelectedTableBrowsePage = 0;
                    SelectedTableResultCount = 0;
                    SelectedTableData = database.RefreshTableData(SelectedTable, 0);

                });
            }

            Gui.SameLine(1);
            Gui.SetNextBackgroundColor(Style.Danger);
            if (Gui.Button("Drop", true))
            {
                Gui.Confirm($"Are you sure you want to drop the table '{SelectedTable}'?", () =>
                {
                    database.RunQueries($"DROP TABLE \"{SelectedTable}\"");
                    database.RefreshTables();
                    SelectedTable = string.Empty;
                });
            }
            Gui.EndTab();
        }
    }


}
