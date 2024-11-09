using Avalonia.Media;
using pluginVerilog.Verilog.BuildingBlocks;
using pluginVerilog.Verilog.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CodeEditor2.Controller;

namespace pluginVivado.Views
{
    internal class SimulationTab : CodeEditor2.Views.CodeTabItem
    {
        protected SimulationTab(string title, string? iconName, Avalonia.Media.Color? iconColor, bool closeButtonEnable) : base(title, iconName, iconColor, closeButtonEnable)
        {
            SimPanel = new SimPanel();
            Content = SimPanel;
        }

        private const string prompt = "xSimShell";
        public static SimulationTab? Create()
        {
            CodeEditor2.Data.File? file;
            file = CodeEditor2.Controller.NavigatePanel.GetSelectedFile();

            pluginVerilog.Data.VerilogFile? vFile = file as pluginVerilog.Data.VerilogFile;
            if (vFile == null) return null;

            pluginVerilog.Data.SimulationSetup? simulationSetup = pluginVerilog.Data.SimulationSetup.Create(vFile);
            if (simulationSetup == null) return null;

            SimulationTab tab = new SimulationTab(simulationSetup.TopName, "play", Plugin.ThemeColor, true);
            tab.SimulationSetup = simulationSetup;

            tab.CloseButton_Clicked += new Action(() => { tab.Close(); });

            return tab;
        }

        public SimPanel SimPanel;
        protected pluginVerilog.Data.SimulationSetup? SimulationSetup;
        protected CodeEditor2.Shells.WinCmdShell? shell;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private void Close()
        {
            tokenSource.Cancel();
            CodeEditor2.Controller.Tabs.RemoveItem(this);
            tokenSource.Dispose();
            if (shell != null) shell.Dispose();
        }

        public void Run()
        {
            var _ = work(tokenSource.Token);
        }

        private async Task work(CancellationToken token)
        {
            if (SimulationSetup == null) throw new Exception();

            string simName = SimulationSetup.TopName;


            string simulationPath = Setup.SimulationPath + "\\" + simName;
            simulationPath = @"c:\temp\" + simName;

            if (!System.IO.Directory.Exists(simulationPath))
            {
                System.IO.Directory.CreateDirectory(simulationPath);
            }


            // create project file
            // verilog<work_library> < file_names > ... [-d<macro>]...[-i<include_path>]...
            // vhdl<work_library> <file_name>
            // sv<work_library> <file_name>
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(simulationPath + "\\" + simName + ".prj"))
            {
                foreach (CodeEditor2.Data.File file in SimulationSetup.Files)
                {
                    pluginVerilog.Data.VerilogFile? vFile = file as pluginVerilog.Data.VerilogFile;
                    if (vFile == null) continue;

                    if (vFile.SystemVerilog)
                    {
                        sw.Write("sv " + "work" + " \"" + SimulationSetup.Project.GetAbsolutePath(file.RelativePath) + "\"");
                    }
                    else
                    {
                        sw.Write("verilog " + "work" + " \"" + SimulationSetup.Project.GetAbsolutePath(file.RelativePath) + "\"");
                    }

                    if (SimulationSetup.IncludePaths.Count != 0)
                    {
                        foreach (string includePath in SimulationSetup.IncludePaths)
                        {
                            sw.Write(" -i \"" + includePath + "\""); // path with space is not accepted
                        }
                    }
                    sw.Write("\r\n");
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(simulationPath + "\\command.bat"))
            {
                sw.Write("echo #compile\r\n");

                //foreach (string absolutePath in filePathList) 
                //{
                //    sw.Write("call "+Setup.BinPath + "xvlog ^\r\n");
                //    if (includeFileList.Count != 0)
                //    {
                //        foreach (string includePath in includeFileList)
                //        {
                //            sw.Write("-i \"" + includePath + "\" ^\r\n"); // path with space is not accepted
                //        }
                //    }
                //    sw.Write("\""+absolutePath + "\"");
                //    sw.Write("\r\n");
                //}

                sw.Write("call " + Setup.BinPath + "xvlog -prj " + simName + ".prj" + " ^\r\n");
                //if (includeFileList.Count != 0)
                //{
                //    foreach (string includePath in includeFileList)
                //    {
                //        sw.Write("-i \"" + includePath + "\" ^"); // path with space is not accepted
                //    }
                //}
                //foreach (string absolutePath in filePathList)
                //{
                //    sw.Write(" ^\r\n \"" + absolutePath + "\"");
                //}
                sw.Write("\r\n");
                sw.Write("\r\n");
                sw.Write("echo #elaboration\r\n");
                sw.Write("call " + Setup.BinPath + "xelab ^\r\n");
                sw.Write("--debug all ^\r\n");
                sw.Write("--notimingchecks ^\r\n");
                sw.Write(SimulationSetup.TopName + "\r\n");
                sw.Write("\r\n");
                sw.Write("\r\n");

                sw.Write("echo #simulation\r\n");
                sw.Write("call " + Setup.BinPath + "xsim " + SimulationSetup.TopName + " -t xsim_run.tcl\r\n");

            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(simulationPath + "\\xsim_run.tcl"))
            {
                sw.Write("run all\r\n");
            }


            shell = new CodeEditor2.Shells.WinCmdShell(new List<string> {
                "prompt "+prompt+"$G$_",
                "cd "+simulationPath
            });

            shell.LineReceived += Shell_LineReceived;
            shell.Start();


            while (shell.GetLastLine() != prompt+">")
            {
                await Task.Delay(10, token);
                if (token.IsCancellationRequested) return;
            }
            shell.ClearLogs();
            shell.StartLogging();
            shell.Execute("command.bat");
            while (shell.GetLastLine() != "%xsim")
            {
                await Task.Delay(10, token);
                if (token.IsCancellationRequested) return;
            }
            //RequestTabIconChange(codeEditor.Global.IconImages.Wave0, IconImage.ColorStyle.Green);
            List<string> logs = shell.GetLogs();
            if (logs.Count != 3 || logs[1] != "")
            {
                return;
            }
            shell.EndLogging();
            ///
        }
        private void Shell_LineReceived(string lineString)
        {
            if (lineString == prompt + ">")
            {
                SimPanel.LineReceived(lineString, Colors.Green);
            }else if (lineString.StartsWith("ERROR:")){
                SimPanel.LineReceived(lineString, Colors.Red);
            }
            else
            {
                SimPanel.LineReceived(lineString, null);
            }
        }
    }
}
