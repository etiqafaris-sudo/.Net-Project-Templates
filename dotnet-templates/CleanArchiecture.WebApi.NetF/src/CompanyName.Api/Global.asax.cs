using System.Web;
using System.Web.Http;
using CompanyName.Api.Infrastructure;

namespace CompanyName.Api;

public class WebApiApplication : HttpApplication
{
    protected void Application_Start()
    {
        if (Program.IsSelfHosted)
        {
            return;
        }

        ApplicationBootstrap.Configure(GlobalConfiguration.Configuration);
        ApplicationBootstrap.StartBackgroundWorkers();

#if DEBUG
        ApplicationBootstrap.TryApplyMigrations();
#endif
    }

    protected void Application_End()
    {
        if (!Program.IsSelfHosted)
        {
            ApplicationBootstrap.Shutdown();
        }
    }
}