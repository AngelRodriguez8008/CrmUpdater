using System;
using System.ServiceModel;

namespace McTools.Xrm.Connection
{
    public class CrmExceptionHelper
    {
        public static string GetErrorMessage(Exception error, bool returnWithStackTrace)
        {
            if (error.InnerException is FaultException exception)
            {
                return returnWithStackTrace ? exception.ToString() : exception.Message;
            }

            if (error.InnerException != null)
            {
                return returnWithStackTrace ? error.InnerException.ToString() : error.InnerException.Message;
            }

            return returnWithStackTrace ? error.ToString() : error.Message;
        }
    }
}
