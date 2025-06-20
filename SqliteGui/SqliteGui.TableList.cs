using ConGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteGui;

public partial class SqliteGui
{

	public void BuildTableList()
	{
		Gui.BeginList("Tables");
		{
			foreach (var table in database.Tables)
			{
				if (Gui.ListEntry(table, ListChangedEvent.OnSelect))
				{
					SetTable(table);
				}
			}
		}
		Gui.EndList();
	}

}
