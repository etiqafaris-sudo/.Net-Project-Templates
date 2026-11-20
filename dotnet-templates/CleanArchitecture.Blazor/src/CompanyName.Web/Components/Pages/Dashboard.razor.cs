using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.IO;

namespace CompanyName.Web.Components.Pages
{
    public partial class Dashboard : AppComponentBase<object>
    {
        #region --- Declaration ---
        [Inject] IJSRuntime JS { get; set; } = default!;

        public int TotalOverallApplications { get; set; } = 1304200;

        public List<StateData> TopStates { get; set; } = new();
        public List<NotificationItem> Notifications { get; set; } = new();
        public List<AppTypeData> ApplicationTypes { get; set; } = new();
        public List<TopAppData> TopApplications { get; set; } = new();

        public class StateData
        {
            public string State { get; set; } = "";
            public int Value { get; set; }
            public string DisplayValue { get; set; } = "";
            public string Color { get; set; } = "";
            public int Percent { get; set; }
        }

        public class NotificationItem
        {
            public string Name { get; set; } = "";
            public string Action { get; set; } = "";
            public string Subtitle { get; set; } = "";
            public string Date { get; set; } = "";
        }

        public class AppTypeData
        {
            public string Month { get; set; } = "";
            public double PasarRaya { get; set; }
            public double Ejen { get; set; }
            public double Peniaga { get; set; }
            public double Penjaja { get; set; }
        }

        public class TopAppData
        {
            public string Season { get; set; } = "";
            public int Count { get; set; }
            public int Percent { get; set; }
        }
        #endregion

        #region --- Page Load ---
        protected override void OnInitialized()
        {
            TopStates = new List<StateData>
            {
                new StateData { State = "Selangor", Value = 12000000, DisplayValue = "12M", Color = "#3b82f6", Percent = 100 },
                new StateData { State = "Kuala Lumpur", Value = 1302400, DisplayValue = "1,302,400", Color = "#eab308", Percent = 80 },
                new StateData { State = "Perak", Value = 304102, DisplayValue = "304,102", Color = "#60a5fa", Percent = 40 },
                new StateData { State = "Johor", Value = 134102, DisplayValue = "134,102", Color = "#818cf8", Percent = 20 }
            };

            Notifications = new List<NotificationItem>
            {
                new NotificationItem { Name = "Firework Sdn Bhd", Action = "has Applied the application", Subtitle = "Tahun Baru Cina", Date = "March 6, 2020" },
                new NotificationItem { Name = "Firework Sdn Bhd", Action = "has Applied the application", Subtitle = "Tahun Baru Cina", Date = "March 6, 2020" },
                new NotificationItem { Name = "Firework Sdn Bhd", Action = "has Applied the application", Subtitle = "Tahun Baru Cina", Date = "March 6, 2020" },
                new NotificationItem { Name = "Firework Sdn Bhd", Action = "has Applied the application", Subtitle = "Tahun Baru Cina", Date = "March 6, 2020" },
                new NotificationItem { Name = "Firework Sdn Bhd", Action = "has Applied the application", Subtitle = "Tahun Baru Cina", Date = "March 6, 2020" },
                new NotificationItem { Name = "Firework Sdn Bhd", Action = "has Applied the application", Subtitle = "Tahun Baru Cina", Date = "March 6, 2020" }
            };

            ApplicationTypes = new List<AppTypeData>
            {
                new AppTypeData { Month = "Jan", PasarRaya = 180, Ejen = 100, Peniaga = 280, Penjaja = 50 },
                new AppTypeData { Month = "Feb", PasarRaya = 210, Ejen = 130, Peniaga = 160, Penjaja = 60 },
                new AppTypeData { Month = "Mar", PasarRaya = 150, Ejen = 100, Peniaga = 180, Penjaja = 80 },
                new AppTypeData { Month = "Apr", PasarRaya = 100, Ejen = 150, Peniaga = 120, Penjaja = 90 },
                new AppTypeData { Month = "May", PasarRaya = 250, Ejen = 150, Peniaga = 380, Penjaja = 40 },
                new AppTypeData { Month = "Jun", PasarRaya = 120, Ejen = 60,  Peniaga = 150, Penjaja = 40 },
                new AppTypeData { Month = "Jul", PasarRaya = 160, Ejen = 100, Peniaga = 190, Penjaja = 20 }
            };

            TopApplications = new List<TopAppData>
            {
                new TopAppData { Season = "Tahun Baru Cina", Count = 14521, Percent = 40 }
            };
        }
        #endregion

        #region --- Other Function ---
        // --- Export Functions ---

        public async Task OnExportPdf()
        {
            // Triggers the browser's native print dialog
            await JS.InvokeVoidAsync("printDashboard");
        }

        public async Task OnExportCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("State,Total Patients/Applications");
            foreach (var s in TopStates)
            {
                sb.AppendLine($"{s.State},{s.Value}");
            }

            sb.AppendLine();
            sb.AppendLine("Month,Pasar Raya,Ejen,Peniaga,Penjaja");
            foreach (var a in ApplicationTypes)
            {
                sb.AppendLine($"{a.Month},{a.PasarRaya},{a.Ejen},{a.Peniaga},{a.Penjaja}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var base64 = Convert.ToBase64String(bytes);
            await JS.InvokeVoidAsync("downloadFileFromBase64", "Dashboard_Report.csv", base64, "text/csv");
        }

        public async Task OnExportExcel()
        {
            // A simple trick to generate "Excel" is an HTML table saved as .xls
            var sb = new StringBuilder();
            sb.AppendLine("<html><body><table border='1'>");
            sb.AppendLine("<tr><th>State</th><th>Total Applications</th></tr>");
            foreach (var s in TopStates) sb.AppendLine($"<tr><td>{s.State}</td><td>{s.Value}</td></tr>");
            sb.AppendLine("</table><br/><table border='1'>");
            sb.AppendLine("<tr><th>Month</th><th>Pasar Raya</th><th>Ejen</th><th>Peniaga</th><th>Penjaja</th></tr>");
            foreach (var a in ApplicationTypes) sb.AppendLine($"<tr><td>{a.Month}</td><td>{a.PasarRaya}</td><td>{a.Ejen}</td><td>{a.Peniaga}</td><td>{a.Penjaja}</td></tr>");
            sb.AppendLine("</table></body></html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var base64 = Convert.ToBase64String(bytes);
            // using application/vnd.ms-excel ContentType
            await JS.InvokeVoidAsync("downloadFileFromBase64", "Dashboard_Report.xls", base64, "application/vnd.ms-excel");
        }

        public async Task OnExportWord()
        {
            // Simulating Word export using HTML + .doc extension
            var sb = new StringBuilder();
            sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.AppendLine("<head><title>Dashboard Report</title></head><body>");
            sb.AppendLine("<h1>Overall Application Dashboard</h1>");
            sb.AppendLine($"<p>Total Overall Applications: {TotalOverallApplications.ToString("N0")}</p>");
            sb.AppendLine("<h3>Top Higher Patients by State</h3><ul>");
            foreach (var s in TopStates) sb.AppendLine($"<li>{s.State}: {s.DisplayValue}</li>");
            sb.AppendLine("</ul></body></html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var base64 = Convert.ToBase64String(bytes);
            await JS.InvokeVoidAsync("downloadFileFromBase64", "Dashboard_Report.doc", base64, "application/msword");
        }
        #endregion
    }
}
