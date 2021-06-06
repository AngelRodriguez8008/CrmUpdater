#region Version
// Last Change: Rodriguez Mustelier Angel (rodang) - 2019-02-26 03:58
// 
#endregion

using System;
using System.IO;
using System.Linq;
using Microsoft.Xrm.Sdk;
using CrmWebResourcesUpdater.Helpers;

namespace CrmWebResourcesUpdater
{
    public class VersionsHelper
    {
        public static string JsVersionTemplate = "// Version: ";
        public static string[] JsExtensions = { "js" };

        public static string UpdateJS(string filePath)
        {
            // Read the appropriate line from the file.
            var newVersion = GenerateJsVersion(DateTime.UtcNow);

            var isJsFile = IsFileExtensionSomeOf(filePath, JsExtensions);
            if (isJsFile == false)
                return null;
            
            // Read the old file.
            string[] lines = File.ReadAllLines(filePath);
            int index = Array.FindIndex(lines, l => l.StartsWith(JsVersionTemplate));
            if (index >= 0)
                lines[index] = newVersion;
            else
            {
                // Append 2 lines at begin
                var previosLines = lines;
                lines = new string[lines.Length + 2];
                lines[0] = newVersion;
                lines[1] = string.Empty;
                Array.Copy(previosLines, 0, lines, 2, previosLines.Length);
            }
            File.WriteAllLines(filePath, lines);

            return newVersion;
        }

        public static string GenerateJsVersion(DateTime now)
        {
            string dateTimeStr = $"{now:yyyy-MM-dd HH:mm}";
            string lineToWrite = JsVersionTemplate + dateTimeStr;
            return lineToWrite;
        }

        public static bool IsFileExtensionSomeOf(string filePath, string[] extensions)
        {
            var extension = Path.GetExtension(filePath)?.TrimStart('.');
            var result = extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase);
            return result;
        }

        public static string UpdateSolution(IOrganizationService service, Guid? solutionId)
        {
            if (solutionId == null)
                return null;

            var version = service.GetSolutionVersion(solutionId.Value);
            version = IncrementSolutionVersion(version, DateTime.UtcNow);
            service.UpdateSolutionVersion(solutionId.Value, version);
            return version;
        }

        public static string IncrementSolutionVersion(string previos, DateTime now)
        {
            string result;
            if (string.IsNullOrWhiteSpace(previos))
                result = "1.0";
            else
            {
                var parts = previos.Split(new[] { '.' }, 3);
                parts = Clean(parts);
                if (parts.Length == 0)
                    result = "1.0";
                else if (parts.Length == 1)
                    result = parts[0] + ".0";
                else
                    result = parts[0] + "." + parts[1];
            }
            string dateTimeStr = $"{now:.yyMMdd.HHmm}";
            result += dateTimeStr;
            return result;
        }

        private static string[] Clean(string[] parts)
        {
            var result = parts
               .Select(v => v.Trim().TrimEnd('.').Trim())
               .Where(v => string.IsNullOrWhiteSpace(v) == false)
               .ToArray();

            return result;
        }
    }
}