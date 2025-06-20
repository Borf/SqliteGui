using ConGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{
    public void BuildTabBrowse()
    {
        if (Gui.BeginTab("Browse"))
        {
            int colWidth = 20;
            Gui.PushId("Header");
            foreach (Column field in SelectedTableStructure)
            {
                if (field != SelectedTableStructure.First())
                    Gui.SameLine();
                Gui.SetNextWidth(colWidth);
                Gui.Text(field.ColumnName);
            }
            Gui.PopId();

            int rowId = 0;
            foreach (var row in SelectedTableData)
            {
                Gui.PushId("row" + rowId++);
                int col = 0;
                for (int i = 0; i < row.Count; i++)
                {
                    object? cell = row[i];
                    Gui.SetNextWidth(colWidth);
                    if (i > 0)
                        Gui.SameLine();
                    if(cell is null)
                    {
                        Gui.Text("NULL");
                    }
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
                        Gui.Text(cell!.GetType().ToString());
                    }
                }

                Gui.PopId();
            }
            Gui.Text("");

            if (SelectedTablePageCount > 0)
            {
                if (Gui.Button("Prev", true))
                {
                    SetPage(SelectedTableBrowsePage - 1);
                }
                Gui.SameLine();
                Gui.Text($"Page {SelectedTableBrowsePage + 1} of {SelectedTablePageCount + 1}");
                Gui.SameLine();
                if (Gui.Button("Next", true))
                {
                    SetPage(SelectedTableBrowsePage + 1);
                }
            }

            Gui.EndTab();
        }
    }


}
