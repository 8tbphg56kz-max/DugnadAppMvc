using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace DugnadAppMvc.Helpers;

public static class TimerAlternativerHelper
{
    public static List<SelectListItem> Hent()
    {
        var liste = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = "",
                Text = "Velg timer..."
            }
        };

        for (decimal t = 0.5m; t <= 24m; t += 0.5m)
        {
            liste.Add(new SelectListItem
            {
                Value = t.ToString(CultureInfo.InvariantCulture), // 1.5
                Text = t.ToString("0.0", CultureInfo.GetCultureInfo("nb-NO")) // 1,5
            });
        }

        return liste;
    }
}