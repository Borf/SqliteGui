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
    public void BuildMenu()
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
    }

}
