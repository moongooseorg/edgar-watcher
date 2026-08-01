using EdgarWatcher.Models;

namespace EdgarWatcher.Features.SecApi;

public static class ApiHelper
{
    public static List<Filing> FixFilingObjectAndSort(RecentFiling filings)
    {
        List<Filing> result = new List<Filing>();

        for (int i = 0; i < filings.AcceptanceDateTime.Count; i++)
        {
            result.Add(new Filing
            {
                AccessionNumber = filings.AccessionNumber[i],
                FilingDate = filings.FilingDate[i],
                ReportDate = filings.ReportDate[i],
                AcceptanceDateTime = filings.AcceptanceDateTime[i],
                Act = filings.Act[i],
                Form = filings.Form[i],
                FileNumber = filings.FileNumber[i],
                FilmNumber = filings.FilmNumber[i],
                Items = filings.Items[i],
                Size = filings.Size[i],
                IsXBRL = filings.IsXBRL[i],
                IsInlineXBRL = filings.IsInlineXBRL[i],
                PrimaryDocument = filings.PrimaryDocument[i],
                PrimaryDocDescription = filings.PrimaryDocDescription[i]
            });
        }

        result.Sort((a, b) =>
        {
            DateTime dateA = DateTime.Parse(a.AcceptanceDateTime);
            DateTime dateB = DateTime.Parse(b.AcceptanceDateTime);
            return dateB.CompareTo(dateA);
        });

        return result;
    }
}
