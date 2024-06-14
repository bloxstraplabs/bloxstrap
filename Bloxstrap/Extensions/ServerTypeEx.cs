namespace Bloxstrap.Extensions
{
    static class ServerTypeEx
    {
        public static string ToTranslatedString(this ServerType value) => value switch
        {
            ServerType.Public => Resources.Strings.Enums_ServerType_Public,
            ServerType.Private => Resources.Strings.Enums_ServerType_Private,
            ServerType.Reserved => Resources.Strings.Enums_ServerType_Reserved,
            _ => "?"
        };
    }
}
