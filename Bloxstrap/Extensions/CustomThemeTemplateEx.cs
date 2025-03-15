using System.Text;

namespace Bloxstrap.Extensions
{
    static class CustomThemeTemplateEx
    {
        const string EXAMPLES_URL = "https://github.com/bloxstraplabs/custom-bootstrapper-examples";

        public static string GetFileName(this CustomThemeTemplate template)
        {
            return $"CustomBootstrapperTemplate_{template}.xml";
        }

        public static string GetFileContents(this CustomThemeTemplate template)
        {
            string contents = Encoding.UTF8.GetString(Resource.Get(template.GetFileName()).Result);

            switch (template)
            {
                case CustomThemeTemplate.Blank:
                    {
                        string moreText = string.Format(Strings.CustomTheme_Templates_Blank_MoreExamples, EXAMPLES_URL);
                        return string.Format(contents, Strings.CustomTheme_Templates_Blank_UIElements, moreText);
                    }
                case CustomThemeTemplate.Simple:
                    {
                        string moreText = string.Format(Strings.CustomTheme_Templates_Simple_MoreExamples, EXAMPLES_URL);
                        return string.Format(contents, moreText);
                    }
                default:
                    Debug.Assert(false);
                    return contents;
            }
        }
    }
}
