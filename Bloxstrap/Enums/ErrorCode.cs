namespace Bloxstrap.Enums
{
    // https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
    // https://i-logic.com/serial/errorcodes.htm
    // just the ones that we're interested in

    public enum ErrorCode
    {
        ERROR_SUCCESS = 0,
        ERROR_INSTALL_USEREXIT = 1602,
        ERROR_INSTALL_FAILURE = 1603,
        ERROR_CANCELLED = 1223
    }
}
