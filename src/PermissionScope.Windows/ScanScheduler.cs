using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace PermissionScope.Windows;

public static class ScanScheduler
{
    public static void CreateDaily(string root, string time)
    {
        if (!TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start)) throw new ArgumentException("Use HH:mm for the scheduled time.");
        var executable = Path.Combine(AppContext.BaseDirectory, "permissionscope-cli.exe");
        if (!File.Exists(executable)) throw new FileNotFoundException("Keep permissionscope-cli.exe beside PermissionScope.exe to enable scheduling.", executable);
        if (root.Contains('"')) throw new ArgumentException("A path cannot contain quotation marks.");
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        var quotedRoot = root.EndsWith('\\') ? root + "." : root;
        using var currentIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var document = new XDocument(new XElement(ns + "Task", new XAttribute("version", "1.2"),
            new XElement(ns + "Triggers", new XElement(ns + "CalendarTrigger", new XElement(ns + "StartBoundary", DateTime.Today.Add(start.ToTimeSpan()).ToString("s")), new XElement(ns + "Enabled", "true"), new XElement(ns + "ScheduleByDay", new XElement(ns + "DaysInterval", 1)))),
            new XElement(ns + "Principals", new XElement(ns + "Principal", new XAttribute("id", "Author"), new XElement(ns + "UserId", currentIdentity.User!.Value), new XElement(ns + "LogonType", "InteractiveToken"), new XElement(ns + "RunLevel", "LeastPrivilege"))),
            new XElement(ns + "Settings", new XElement(ns + "MultipleInstancesPolicy", "IgnoreNew"), new XElement(ns + "DisallowStartIfOnBatteries", "false"), new XElement(ns + "StopIfGoingOnBatteries", "false"), new XElement(ns + "ExecutionTimeLimit", "PT4H")),
            new XElement(ns + "Actions", new XAttribute("Context", "Author"), new XElement(ns + "Exec", new XElement(ns + "Command", executable), new XElement(ns + "Arguments", $"scan \"{quotedRoot}\" --save")))));
        var temp = Path.Combine(Path.GetTempPath(), "PermissionScope-task-" + Guid.NewGuid().ToString("N") + ".xml");
        try
        {
            Core.AtomicFile.WriteText(temp, document.ToString());
            var info = new ProcessStartInfo("schtasks.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var arg in new[] { "/Create", "/TN", "PermissionScope-" + Guid.NewGuid().ToString("N")[..8], "/XML", temp }) info.ArgumentList.Add(arg);
            using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start Windows Task Scheduler.");
            var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
            process.WaitForExit(); Task.WaitAll(stdout, stderr);
            if (process.ExitCode != 0) throw new InvalidOperationException(stderr.Result + stdout.Result);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
