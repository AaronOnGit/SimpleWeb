using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SimpleWeb.Startup))]
namespace SimpleWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
