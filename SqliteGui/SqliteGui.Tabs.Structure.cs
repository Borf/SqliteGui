using ConGui;
using System;
using System.Collections.Generic;
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
            foreach (Column field in SelectedTableStructure)
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
                    Gui.SetNextBackgroundColor(Style.Danger);
                if(Gui.Button("↑", false))
                {

                }

                Gui.SameLine();
                Gui.SetNextWidth(5);
                if (column == SelectedTableStructure.Count)
                    Gui.SetNextBackgroundColor(Style.Danger);
                if (Gui.Button("↓", false))
                {

                }
                

                Gui.PopId();
            }

            if (Gui.Button("Add column", true))
            {
                SelectedTableStructure.Add(new Column { ColumnName = "", DataType = DataType.TEXT });
            }

            Gui.EndTab();
        }
    }


}
