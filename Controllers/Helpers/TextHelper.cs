using System.Net;

namespace DugnadAppMvc.Helpers
{
    public static class TextHelper
    {
        public static string FormaterLinjeskift(string? tekst)
        {
            if (string.IsNullOrWhiteSpace(tekst))
            {
                return string.Empty;
            }

            // Normaliserer alle typer linjeskift
            var linjer = tekst
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');

            // Fjern tomme linjer og mellomrom rundt hver linje
            var rensedeLinjer = linjer
                .Select(linje => linje.Trim())
                .Where(linje => !string.IsNullOrWhiteSpace(linje))
                .Select(linje => WebUtility.HtmlEncode(linje));

            // Ett <br /> per faktisk linjeskift
            return string.Join("<br />", rensedeLinjer);
        }
    }
}