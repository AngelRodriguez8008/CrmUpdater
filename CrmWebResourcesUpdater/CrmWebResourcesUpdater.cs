using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using CrmWebResourcesUpdater.Helpers;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.OptionsForms;
using Microsoft.VisualStudio.Shell.Interop;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// CrmWebResourceUpdater extension package class
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true,SatellitePath ="")]
    [ProvideBindingPath]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ProjectGuids.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]

    public sealed class CrmWebResourcesUpdater : Package
    {
        EnvDTE80.DTE2 dte;
        private DteInitializer dteInitializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebResources"/> class.
        /// </summary>
        public CrmWebResourcesUpdater()
        {
            ProjectHelper.SetServiceProvider(this);
            Logger.Initialize();
        }

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            InitializeDTE();
        }

        private void InitializeDTE()
        {
            IVsShell shellService;

            dte = GetService(typeof(SDTE)) as EnvDTE80.DTE2;

            var extendedLog = false;
            var settings = ProjectHelper.GetSettings<Settings>();
            if(settings != null)
            {
                extendedLog = settings.ExtendedLog;
            }

            if (dte == null) // The IDE is not yet fully initialized
            {
                Logger.WriteLine("Warning: DTE service is null. Seems that VisualStudio is not fully initialized.", extendedLog);
                Logger.WriteLine("Waiting for DTE.", extendedLog);
                shellService = GetService(typeof(SVsShell)) as IVsShell;
                dteInitializer = new DteInitializer(shellService, InitializeDTE);
            }
            else
            {
                Logger.WriteLine("DTE service found.", extendedLog);
                dteInitializer = null;
                UpdateWebResources.Initialize(this);
                UpdaterOptions.Initialize(this);
                UpdateSelectedWebResources.Initialize(this);
                CreateWebResource.Initialize(this);
            }
        }


        #endregion
    }
}
