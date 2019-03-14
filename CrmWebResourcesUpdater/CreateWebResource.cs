﻿using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Helpers;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateWebResource
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0400;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebResources"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CreateWebResource(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(ProjectGuids.ItemCommandSet, CommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateWebResource Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new CreateWebResource(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var settings = ProjectHelper.GetSettings();
            var result = DialogResult.Cancel;

            if (settings.SelectedConnection == null)
            {

                if (ProjectHelper.ShowErrorDialog() == DialogResult.Yes)
                {
                    var project = ProjectHelper.GetSelectedProject();
                    result = Publisher.ShowConfigurationDialog(ConfigurationMode.Normal, project);
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            if (settings.SelectedConnection == null)
            {
                Logger.WriteLine("Error: Connection is not selected");
                return;
            }

            using (var publisher = new Publisher(settings.SelectedConnection, settings.CrmConnections.PublishAfterUpload, settings.CrmConnections.IgnoreExtensions, settings.CrmConnections.ExtendedLog))
            {
                publisher.CreateWebResource();
            }
        }
    }
}
