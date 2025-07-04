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
        Gui.Text(" ╭─────────────╮ ╭──────────────╮ ╭────╮                               Borf");
        Gui.Text(" │             │ │              │ │    │                                   ");
        Gui.Text(" │    ╭────────╯ │    ╭────╮    │ │    │                                   ");
        Gui.Text(" │    │          │    │    │    │ │    │          ╭──╮  ╭──╮               ");
        Gui.Text(" │    ╰────────╮ │    │    │    │ │    │          ╰──╯  │  │               ");
        Gui.Text(" │             │ │    │    │    │ │    │          ╭──╮╭─╯  ╰─╮    ╭───────╮");
        Gui.Text(" ╰────────╮    │ │    │    │    │ │    │          │  │╰─╮  ╭─╯    │  ╭─╮  │");
        Gui.Text("          │    │ │    │    │    │ │    │          │  │  │  │      │  ╰─╯  │");
        Gui.Text(" ╭────────╯    │ │    ╰────╯    │ │    ╰────────╮ │  │  │  │ ╭──╮ │  ╭────╯");
        Gui.Text(" │             │ │         ╭╮   │ │             │ │  │  │  ╰─╯  │ │  ╰────╮");
        Gui.Text(" ╰─────────────╯ ╰─────────╯╰───╯ ╰─────────────╯ ╰──╯  ╰───────╯ ╰───────╯");
        if (Gui.InputButton("Create Table", ref CreateTableName, "Create"))
        { 
            database.RunQueries($"CREATE TABLE \"{CreateTableName}\"\n ( Id INTEGER NOT NULL\n);");
            database.RefreshTables();
            SelectedTable = CreateTableName;
        }

        //Gui.Text("  🭊🭂████🭍🬿   🭊🭂█████🭍🬿  🭃█🭌      🭃█🭌 🭃█🭌            ");
        //Gui.Text(" 🭋██🭚  🭥🭓█🯫 🭋██🭟🭗 🭢🭔██🭀 ███      🭒█🭝 ███            ");
        //Gui.Text(" 🭦██🬿       🭅██🭛   🭦██🭐 ███          ███            ");
        //Gui.Text("  🭥🭓███🭍🭑🬽  ███     ███ ███      🭃█🭌 █████🯫 🭊🭂██🭍🬿  ");
        //Gui.Text("      🭣🭕██🭎 ███     ███ ███      ███ ███   🭋█🭪  🭨█🭀 ");
        //Gui.Text("        ██🭡 🭖██🭀🭦█🭐🭋██🭡 ███      ███ ███   🭅██████🭛 ");
        //Gui.Text(" 🯩█🭎🬼 🭈🭄██🭛 🭦██🭎🬼🭦████🭛 ███🬼     ███ 🭔██🬼  🭖█🭪      ");
        //Gui.Text("  🭥🭓████🭝🭚   🭥🭓██████🭚  🭒██████🯫 🭒█🭝 🭢🭕███🯫🭤🭓████🯫  ");
        //Gui.Text("                   🭖█🭎                              ");
        /*
         *  	🬼	🬽	🬾	🬿   🭀	🭌	🭍	🭎	🭏   🭐	🭑	
         *  	🭇	🭈	🭉	🭊	🭋	🭁	🭂	🭃	🭄	🭅	🭆	
         *  	🭗	🭘	🭙	🭚	🭛	🭜	🭝	🭞	🭟   🭠	🭡	
         *  	🭢	🭣	🭤	🭥	🭦	🭧	🭒	🭓	🭔	🭕	🭖	
         *     🭨	🭩	🭪	🭫	🭬	🭭	🭮	🭯
         *     🯨	🯩	🯪	🯫	🯬	🯭	🯮	🯯
         *     🯰	🯱	🯲	🯳	🯴	🯵	🯶	🯷	🯸	🯹	
         */
    }

}
