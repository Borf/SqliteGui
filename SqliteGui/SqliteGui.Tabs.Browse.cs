using ConGui;
using ConGui.Util;
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
            //TODO: if Tab was clicked... need to store an extra state for this
            //allow to change query

            Gui.SetNextBackgroundColor(Style.Info);
            Gui.SetNextTextColor(Style.Back);
            Gui.Text($"Showing {SelectedTableBrowsePage * 20} - {SelectedTableBrowsePage * 20 + 20} out of {SelectedTableResultCount}");

            Dictionary<DataType, int> colWidth = new() {
                { DataType.INTEGER, 10},
                { DataType.TEXT, 20},
                { DataType.NUMERIC, 10},
                { DataType.REAL, 10},
                { DataType.BLOB, 5},
                { DataType.UNKNOWN, 5 },
            };


            Gui.PushId("Header");
            Gui.Text("   │", true);
            foreach (Column field in SelectedTableStructure)
            {
                Gui.SameLine();
                Gui.SetNextWidth(colWidth[field.DataType]);
                Gui.Text(field.ColumnName.PadLength(colWidth[field.DataType]), true, field.ColumnName);
                Gui.SameLine();
                Gui.Text("│", true);
            }
            Gui.SameLine();
            Gui.Text("Edit│Del│", true);
            Gui.PopId();



            int rowId = 0;
            foreach (var row in SelectedTableData)
            {
                Gui.PushId("row" + rowId++);
                Gui.CheckBox("#select", false);
                Gui.SameLine();
                Gui.Text("│");

                int col = 0;
                for (int i = 0; i < row.Count; i++)
                {
                    object? cell = row[i];
                    Gui.SetNextWidth(colWidth[SelectedTableStructure[i].DataType]);
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
                    Gui.SameLine();
                    Gui.Text("│");

                }
                Gui.SameLine();
                if(Gui.Button("✏ ", false))
                {

                }
                Gui.SameLine();
                Gui.Text("│");
                Gui.SameLine();
                Gui.SetNextBackgroundColor(Style.Danger);
                if (Gui.Button("X", false))
                {

                }
                Gui.SameLine();
                Gui.Text("│");

                Gui.PopId();
            }

            Gui.PushId("Footer");
            Gui.Text("───┴");
            foreach (Column field in SelectedTableStructure)
            {
                Gui.SameLine();
                Gui.SetNextWidth(colWidth[field.DataType]);
                Gui.Text(new string('─', colWidth[field.DataType]));
                Gui.SameLine();
                Gui.Text("┴");
            }
            Gui.SameLine();
            Gui.Text("────┴───╯");
            Gui.PopId();

            Gui.Text("");


            if (SelectedTableResultCount > 0)
            {
                if (Gui.Button("Prev", false))
                {
                    SetPage(SelectedTableBrowsePage - 1);
                }
                Gui.SameLine();
                Gui.Text($"Page {SelectedTableBrowsePage + 1} of {(SelectedTableResultCount/20) + 1}");
                Gui.SameLine();
                if (Gui.Button("Next", false))
                {
                    SetPage(SelectedTableBrowsePage + 1);
                }
            }

            Gui.EndTab();
        }
    }


}
