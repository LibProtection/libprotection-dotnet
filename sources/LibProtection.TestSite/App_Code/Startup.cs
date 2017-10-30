using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LibProtectionTestSite.Startup))]
namespace LibProtectionTestSite
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            
        }
    }
}
