using ConGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{
    public void BuildTabstructure()
    {
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

            if (Gui.Button("Add column", true))
            {
                SelectedTableStructure.Add(new Column { ColumnName = "", DataType = "TEXT" });
            }

            Gui.EndTab();
        }
    }


}
