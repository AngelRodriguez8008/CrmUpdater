using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CrmWebResourcesUpdater.Helpers
{
    public static class MappingHelper
    {
        public static Dictionary<string, string> LoadMappings(Project project)
        {
            var mappingFilePath = GetMappingFilePath(project);
            if (mappingFilePath == null)
            {
                return null;
            }

            var projectRootPath = ProjectHelper.GetProjectRoot(project);
            var projectFiles = ProjectHelper.GetProjectFiles(project.ProjectItems);
            var mappingList = new Dictionary<string, string>();

            XDocument doc = XDocument.Load(mappingFilePath);
            var mappings = doc.Descendants("Mapping");
            foreach (var mapping in mappings)
            {
                var shortScriptPath = mapping.Attribute("localPath")?.Value;
                shortScriptPath = shortScriptPath ?? mapping.Attribute("scriptPath")?.Value;
                if(shortScriptPath == null)
                {
                    throw new ArgumentNullException("Mappings contains 'Mapping' tag without 'localPath' attribute");
                }
                var scriptPath = projectRootPath + "\\" + shortScriptPath;
                scriptPath = scriptPath.ToLower();
                var webResourceName = mapping.Attribute("webResourceName")?.Value;
                if (webResourceName == null)
                {
                    throw new ArgumentNullException("Mappings contains 'Mapping' tag without 'webResourceName' attribute");
                }
                if (mappingList.ContainsKey(scriptPath))
                {
                    throw new ArgumentException($"Mappings contains dublicate for \"{shortScriptPath}\"");
                }
                projectFiles.RemoveAll(x => x.ToLower() == scriptPath);
                mappingList.Add(scriptPath, webResourceName);
            }
            foreach(var mapping in mappingList)
            {
                var webResourceName = mapping.Value;
                var hasProjectMappingDublicates = projectFiles.Any(x => string.Equals(Path.GetFileName(x), webResourceName, StringComparison.OrdinalIgnoreCase));
                if (hasProjectMappingDublicates)
                {
                    throw new ArgumentException($"Project contains dublicate(s) for mapped web resource \"{webResourceName}\"");
                }

            }
            return mappingList;
        }

        public static void CreateMapping(Project project, string filePath, string webresourceName)
        {
            var mappingFilePath = GetMappingFilePath(project) ?? CreateMappingFile(project);

            var projectRootPath = ProjectHelper.GetProjectRoot(project) + "\\";
            var scriptShortPath = filePath.Replace(projectRootPath, "");

            XDocument doc = XDocument.Load(mappingFilePath);
            var mapping = new XElement("Mapping");
            mapping.SetAttributeValue("localPath", scriptShortPath);
            mapping.SetAttributeValue("webResourceName", webresourceName);
            doc.Element("Mappings")?.Add(mapping);
            doc.Save(mappingFilePath);
        }

        public static bool IsMappingFileReadOnly(Project project)
        {
            var path = GetMappingFilePath(project);
            if(path == null)
            {
                return false;
            }
            var fileInfo = new FileInfo(path);
            return fileInfo.IsReadOnly;
        }

        public static bool IsMappingRequired(Project project, string projectItemPath, string webresourceName)
        {
            var fileName = Path.GetFileName(projectItemPath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItemPath);
            if (fileName == webresourceName)
            {
                return false;
            }
            //var settings = ProjectHelper.GetSettings();
            //var uploadWithoutExtension = settings.CrmConnections.IgnoreExtensions;
            if(fileNameWithoutExtension == webresourceName)
            {
                return false;
            }
            return true;
        }

        public static string CreateMappingFile(Project project)
        {
            var projectPath = ProjectHelper.GetProjectRoot(project);
            var filePath = projectPath + "\\UploaderMapping.config";
            if (File.Exists(filePath))
            {
                var path = GetMappingFilePath(project);
                if (path == null)
                {
                    project.ProjectItems.AddFromFile(filePath);
                }
                return filePath;
            }
            var writer = File.CreateText(projectPath + "\\UploaderMapping.config");
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<Mappings  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"	xsi:noNamespaceSchemaLocation=\"http://exitoconsulting.ru/schema/CrmWebResourcesUpdater/MappingSchema.xsd\">");
            writer.WriteLine("<!--");
            writer.WriteLine("EXAMPLES OF MAPPINGS:");
            writer.WriteLine("<Mapping localPath=\"scripts\\contact.js\" webResourceName=\"new_contact\"/>");
            writer.WriteLine("<Mapping localPath=\"account.js\" webResourceName=\"new_account\"/>");
            writer.WriteLine("-->");
            writer.WriteLine("</Mappings>");
            writer.Flush();
            writer.Close();
            project.ProjectItems.AddFromFile(filePath);
            return filePath;
        }

        public static string GetMappingFilePath(Project project)
        {
            var projectFiles = ProjectHelper.GetProjectFiles(project.ProjectItems);
            if (projectFiles == null || projectFiles.Count == 0)
            {
                return null;
            }
            foreach (var file in projectFiles)
            {
                if (string.Equals(Path.GetFileName(file), Settings.MappingFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return file.ToLower();
                }
            }
            return null;
        }
    }
}
