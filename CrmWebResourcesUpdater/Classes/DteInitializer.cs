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
            int hr;
            bool isZombie;

            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie)
            {
                isZombie = (bool)var;

                if (!isZombie)
                {
                    // Release the event handler to detect when the IDE is fully initialized
                    hr = _shellService.UnadviseShellPropertyChanges(_cookie);

                    ErrorHandler.ThrowOnFailure(hr);

                    _cookie = 0;

                    _callback();
                }
            }
            return VSConstants.S_OK;
        }
    }
}
