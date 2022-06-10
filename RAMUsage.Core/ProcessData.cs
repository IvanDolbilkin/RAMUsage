using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.Dynamic;

namespace RAMUsage.Core
{
    public class ProcessData
    {
        public Process Process { get; set; }

        public bool IsResponding { get; set; }

        public string Type { get; private set; }

        public string Details { get; private set; }


        public ProcessData(Process pr)
        {
            Process = pr;
            IsResponding = pr.Responding;
            Type = GetType(pr);
            Details = GetDetails(pr);
        }

        private string GetType(Process p)
        {
            if (p.ProcessName.ToLower() == "ntoskrnl" || p.ProcessName.ToLower() == "WerFault" || p.ProcessName.ToLower() == "backgroundTaskHost" || p.ProcessName.ToLower() == "backgroundTransferHost" || p.ProcessName.ToLower() == "winlogon" || p.ProcessName.ToLower() == "wininit" || p.ProcessName.ToLower() == "csrss" || p.ProcessName.ToLower() == "lsass" || p.ProcessName.ToLower() == "smss" || p.ProcessName.ToLower() == "services" || p.ProcessName.ToLower() == "taskeng" || p.ProcessName.ToLower() == "taskhost" || p.ProcessName.ToLower() == "dwm" || p.ProcessName.ToLower() == "conhost" || p.ProcessName.ToLower() == "svchost" || p.ProcessName.ToLower() == "sihost")
            {
                return "Windows Process";
            }
            else if (p.MainWindowHandle != IntPtr.Zero && p.StartInfo.CreateNoWindow != true)
            {
                return "App";
            }
            else
            {
                return "Background Process";
            }
        }

        private string GetDetails(Process p)
        {
            int processId = p.Id;
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();
            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username = "Unknown";

            foreach (ManagementObject obj in processList)
            {
                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch { }
                }
            }
            return response.Description;
        }
    }
}
