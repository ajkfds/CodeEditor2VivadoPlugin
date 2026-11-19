using Avalonia.Controls;

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

        private void projectCreated(CodeEditor2.Data.Project project, CodeEditor2.Data.Project.Setup? setup)
        {
            //            project.ProjectProperties.Add(Id, new ProjectProperty(project));
        }

        /// <summary>
        /// Id of the "Vivado" grouping row in the shared node menu.
        /// </summary>
        public const string MenuGroupId = "vivado";

        /// <summary>Id of the "Run Simulation" row inside that group.</summary>
        public const string MenuRunSimulationId = "vivadoRunSimulation";

        public bool Initialize()
        {
            // A catalog entry instead of a ContextMenu subscription, so the
            // editor and VS Code share one declaration and the row can state the
            // condition under which it is meaningful (see Available).
            pluginVerilog.CoreBridge.HierarchyMenuCatalog.Register(new pluginVerilog.CoreBridge.HierarchyMenuCatalog.Entry
            {
                Id = MenuGroupId,
                Label = "Vivado",
                Detail = "Xilinx Vivado simulation",
                Targets = pluginVerilog.CoreBridge.HierarchyMenuCatalog.AppliesTo.File,
                // The simulation tab exists only in the editor.
                Hosts = pluginVerilog.CoreBridge.HierarchyMenuCatalog.Hosts.Desktop,
                Group = 30,
                IconPath = "CodeEditor2/Assets/Icons/play.svg",
                IconColor = PackColor(ThemeColor),
                Children =
                {
                    new pluginVerilog.CoreBridge.HierarchyMenuCatalog.Entry
                    {
                        Id = MenuRunSimulationId,
                        Label = "Run Simulation",
                        Detail = "open the Vivado simulation tab",
                        Targets = pluginVerilog.CoreBridge.HierarchyMenuCatalog.AppliesTo.File,
                        Hosts = pluginVerilog.CoreBridge.HierarchyMenuCatalog.Hosts.Desktop,
                        Group = 10,
                        IconPath = "CodeEditor2/Assets/Icons/play.svg",
                        IconColor = PackColor(ThemeColor),
                        Action = pluginVerilog.CoreBridge.HierarchyMenuCatalog.ActionKind.DesktopHandler,
                        Available = () => CodeEditor2.Global.Plugins.ContainsKey(StaticID),
                        Handler = RunSimulation,
                    },
                },
            });
            return true;
        }

        /// <summary>Packs an Avalonia colour into the catalog's 0xAARRGGBB form.</summary>
        private static uint PackColor(Avalonia.Media.Color color)
        {
            return ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
        }

        private static void RunSimulation(pluginVerilog.CoreBridge.NavigateNodeMenuContext context)
        {
            Views.SimulationTab? tab = Views.SimulationTab.Create();
            if (tab == null) return;
            CodeEditor2.Controller.Tabs.AddItem(tab);
            tab.Run();
        }
    }
}
