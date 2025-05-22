using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pluginVivado
{
    public class Plugin : CodeEditor2Plugin.IPlugin
    {
        public static string StaticID = "Vivado";
        public string Id { get { return StaticID; } }

        public static Avalonia.Media.Color ThemeColor = Avalonia.Media.Colors.YellowGreen;

        public bool Register()
        {
            if (!CodeEditor2.Global.Plugins.ContainsKey("Verilog")) return false;

            // register project property creator
            CodeEditor2.Data.Project.Created += projectCreated;

            return true;
        }

        private void projectCreated(CodeEditor2.Data.Project project)
        {
            //            project.ProjectProperties.Add(Id, new ProjectProperty(project));
        }

        public bool Initialize()
        {
            pluginVerilog.NavigatePanel.VerilogFileNode.CustomizeNavigateNodeContextMenu += CustomizeNavigateNodeContextMenuHandler;
            return true;
        }

        public static void CustomizeNavigateNodeContextMenuHandler(Avalonia.Controls.ContextMenu contextMenu)
        {
            MenuItem menuItem_Vivado = CodeEditor2.Global.CreateMenuItem(
                "Vivado", "menuItem_Vivado",
                "CodeEditor2/Assets/Icons/play.svg",
                 Plugin.ThemeColor);
            contextMenu.Items.Add(menuItem_Vivado);
            MenuItem menuItem_RunSimulation = CodeEditor2.Global.CreateMenuItem(
                "Run Simulation",
                "menuItem_RunSimulation",
                "CodeEditor2/Assets/Icons/play.svg",
                Plugin.ThemeColor);
            menuItem_Vivado.Items.Add(menuItem_RunSimulation);
            menuItem_RunSimulation.Click += MenuItem_RunSimulation_Click;
        }

        private static void MenuItem_RunSimulation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Views.SimulationTab? tab = Views.SimulationTab.Create();
            if (tab == null) return;
            CodeEditor2.Controller.Tabs.AddItem(tab);
            tab.Run();
        }
    }
}
