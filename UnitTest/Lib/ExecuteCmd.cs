using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Lib
{
    public static class ExecuteCmd
    {
        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        public static void ExecuteCommandSync(object command)
        {
            try
            {
                Console.WriteLine("ExecuteCommandSync --> " + command);
                // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                // The following commands are needed to redirect the standard output. 
                //This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                //procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                Console.WriteLine("ExecuteCommandSync --> starting....");
                proc.StartInfo = procStartInfo;
                proc.Start();
                //Thread.Sleep(100);
                Console.WriteLine("ExecuteCommandSync --> show main window!");
                //ShowWindow(proc.MainWindowHandle, 5);
                Console.WriteLine("ExecuteCommandSync --> 3");
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                Console.WriteLine("ExecuteCommandSync --> 2");

                // Display the command output.
                Console.WriteLine(result);
                Console.WriteLine("ExecuteCommandSync --> 1");
            }
            catch (Exception objException)
            {
                // Log the exception
                Console.WriteLine("ExecuteCommandSync failed" + objException.Message);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        /// <summary>
        /// Execute the command Asynchronously.
        /// </summary>
        /// <param name="command">string command.</param>
        public static void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException)
            {
                // Log the exception
            }
            catch (ThreadAbortException)
            {
                // Log the exception
            }
            catch (Exception)
            {
                // Log the exception
            }
        }

    }
}
