// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-05-31 10:19

using System;

namespace CrmWebResourcesUpdater.Helpers
{
    public static class ErrorHelper
    {
        public static string GetErrorMessage(this Exception error)
        {
            if (error == null)
                return null;

            string errorMessage = error.Message;
            var ex = error.InnerException;
            while (ex != null)
            {
                errorMessage += "\r\nInner Exception: " + ex.Message;
                ex = ex.InnerException;
            }

            return errorMessage;
        }
    }
}