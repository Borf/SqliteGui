using ConGui;
using ConGui.Util;
using SqliteGui;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;

Gui.CreateContext();


await new SqliteGui.SqliteGui().Run();