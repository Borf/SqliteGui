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
            Gui.InputText("Rename Table", true, ref NewTableName);
            Gui.SameLine();
            Gui.SetNextWidth(10);
            if (Gui.Button("Rename", true))
            {

            }

            Gui.NewLine();
            Gui.SetNextBackgroundColor(Style.Danger);
            if(Gui.Button("Truncate", true))
            {
                Gui.Confirm("Are you sure you want to delete all values in this table?", () =>
                {
                    // truncate table logic here
                });
            }

            Gui.SameLine(1);
            Gui.SetNextBackgroundColor(Style.Danger);
            if (Gui.Button("Drop", true))
            {
                Gui.Confirm("Are you sure you want to drop this table?", () =>
                {
                    // Drop table logic here
                });
            }





            Gui.EndTab();
        }
    }


}
