namespace Bloxstrap.Extensions
{
    static class ServerTypeEx
    {
        public static string ToTranslatedString(this ServerType value) => value switch
        {
            ServerType.Public => Strings.Enums_ServerType_Public,
            ServerType.Private => Strings.Enums_ServerType_Private,
            ServerType.Reserved => Strings.Enums_ServerType_Reserved,
            _ => "?"
        };
    }
}
