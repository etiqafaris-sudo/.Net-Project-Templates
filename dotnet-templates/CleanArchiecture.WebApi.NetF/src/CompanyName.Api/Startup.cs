using System.Web.Http;
using System.Web.Http.Owin;
using CompanyName.Api.Infrastructure;
using Owin;

namespace CompanyName.Api;

public sealed class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var config = new HttpConfiguration();
        ApplicationBootstrap.Configure(config);

#if DEBUG
        ApplicationBootstrap.TryApplyMigrations();
#endif

        app.UseWebApi(config);
    }
}