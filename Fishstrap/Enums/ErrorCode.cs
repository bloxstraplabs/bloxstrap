namespace Bloxstrap.Enums
{
    // https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
    // https://i-logic.com/serial/errorcodes.htm
    // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/705fb797-2175-4a90-b5a3-3918024b10b8
    // just the ones that we're interested in

    public enum ErrorCode
    {
        ERROR_SUCCESS = 0,
        ERROR_INVALID_FUNCTION = 1,
        ERROR_FILE_NOT_FOUND = 2,
        
        ERROR_CANCELLED = 1223,
        ERROR_INSTALL_USEREXIT = 1602,
        ERROR_INSTALL_FAILURE = 1603,

        CO_E_APPNOTFOUND = -2147221003
    }
}
