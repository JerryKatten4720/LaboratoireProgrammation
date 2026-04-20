using System.Diagnostics;
using System.IO;

namespace LaboratoireProgrammation.Project.Helpers;

public static class VaultProcessHelper {
    public static bool CheckSingleInstance(string processName) {
        var processes = Process.GetProcessesByName(processName);
        return processes.Length <= 1;
    }

    public static string AuthenticateUser(string pathSecondaire, string grade, string nom) {
        var startInfo = new ProcessStartInfo {
            FileName = pathSecondaire,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo)) {
            process.StandardInput.WriteLine(grade);
            process.StandardInput.WriteLine(nom);
            process.StandardInput.Close();

            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result.Trim();
        }
    }

    public static Process OpenSurvivalGuide(string tempFilePath) {
        File.WriteAllText(tempFilePath,
            "MANUEL DE SURVIE VAULT-TEC\n\n1. Ne paniquez pas.\n2. Activez les pompes.\n3. Surveillez la pression.");
        return Process.Start("notepad.exe", tempFilePath);
    }
}