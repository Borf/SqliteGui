using ConGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{
    private string CreateTableName = "";
    public void BuildHome()
    {
        Gui.NewLine();
        Gui.Text(" SQLite Gui by Borf");

        if(Gui.InputButton("Create Table", ref CreateTableName, "Create"))
        { 
            database.RunQueries($"CREATE TABLE \"{CreateTableName}\"\n ( Id INTEGER NOT NULL\n);");
            database.RefreshTables();
            SelectedTable = CreateTableName;
        }


    }

}
