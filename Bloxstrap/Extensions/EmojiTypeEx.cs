namespace Bloxstrap.Extensions
{
    static class EmojiTypeEx
    {
        public static IReadOnlyDictionary<EmojiType, string> Filenames => new Dictionary<EmojiType, string>
        {
            { EmojiType.Catmoji, "Catmoji.ttf" },
            { EmojiType.Windows11, "Win1122H2SegoeUIEmoji.ttf" },
            { EmojiType.Windows10, "Win10April2018SegoeUIEmoji.ttf" },
            { EmojiType.Windows8, "Win8.1SegoeUIEmoji.ttf" },
        };

        public static IReadOnlyDictionary<EmojiType, string> Hashes => new Dictionary<EmojiType, string>
        {
            { EmojiType.Catmoji, "98138f398a8cde897074dd2b8d53eca0" },
            { EmojiType.Windows11, "d50758427673578ddf6c9edcdbf367f5" },
            { EmojiType.Windows10, "d8a7eecbebf9dfdf622db8ccda63aff5" },
            { EmojiType.Windows8, "2b01c6caabbe95afc92aa63b9bf100f3" },
        };

        public static string GetHash(this EmojiType emojiType) => Hashes[emojiType];

        public static string GetUrl(this EmojiType emojiType)
        {
            if (emojiType == EmojiType.Default)
                return "";

            return $"https://github.com/bloxstraplabs/rbxcustom-fontemojis/releases/download/my-phone-is-78-percent/{Filenames[emojiType]}";
        }
    }
}
