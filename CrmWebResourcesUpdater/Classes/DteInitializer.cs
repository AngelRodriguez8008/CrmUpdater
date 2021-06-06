using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace CrmWebResourcesUpdater
{
    class DteInitializer : IVsShellPropertyEvents
    {
        private readonly IVsShell _shellService;
        private uint _cookie;
        private readonly Action _callback;

        internal DteInitializer(IVsShell shellService, Action callback)
        {
            _shellService = shellService;
            _callback = callback;

            // Set an event handler to detect when the IDE is fully initialized
            int hr = _shellService.AdviseShellPropertyChanges(this, out _cookie);

            ErrorHandler.ThrowOnFailure(hr);
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            if (propid != (int) __VSSPROPID.VSSPROPID_Zombie)
                return VSConstants.S_OK;

            var isZombie = (bool)var;

            if (isZombie)
                return VSConstants.S_OK;

            // Release the event handler to detect when the IDE is fully initialized
            var hr = _shellService.UnadviseShellPropertyChanges(_cookie);

            ErrorHandler.ThrowOnFailure(hr);

            _cookie = 0;

            _callback();
            return VSConstants.S_OK;
        }
    }
}
