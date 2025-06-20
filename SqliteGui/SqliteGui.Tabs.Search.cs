using ConGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{
    public void BuildTabSearch()
    {
        if (Gui.BeginTab("Search"))
        {

            Gui.EndTab();
        }
    }


}
