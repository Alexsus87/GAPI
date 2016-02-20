using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BritishCab.Startup))]
namespace BritishCab
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
