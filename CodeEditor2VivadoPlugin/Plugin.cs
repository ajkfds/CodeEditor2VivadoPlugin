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
            ContextMenu contextMenu = CodeEditor2.Controller.NavigatePanel.GetContextMenu();
            {
                MenuItem menuItem_Vivado = CodeEditor2.Global.CreateMenuItem(
                    "Vivado", "menuItem_Vivado",
                    "CodeEditor2/Assets/Icons/play.svg",
                    Avalonia.Media.Colors.Yellow);
                contextMenu.Items.Add(menuItem_Vivado);

                MenuItem menuItem_RunSimulation = CodeEditor2.Global.CreateMenuItem(
                    "Run Simulation",
                    "menuItem_RunSimulation",
                    "CodeEditor2/Assets/Icons/play.svg",
                    Avalonia.Media.Colors.Yellow);
                menuItem_Vivado.Items.Add(menuItem_RunSimulation);
                menuItem_RunSimulation.Click += MenuItem_RunSimulation_Click;
            }
            // register project property form tab
            //            CodeEditor.Tools.ProjectPropertyForm.FormCreated += Tools.ProjectPropertyTab.ProjectPropertyFromCreated;

            return true;
        }

        private void MenuItem_RunSimulation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Views.SimulationTab? tab = Views.SimulationTab.Create();
            if (tab == null) return;
            CodeEditor2.Controller.Tabs.AddItem(tab);
            tab.Run();
        }
    }
}
