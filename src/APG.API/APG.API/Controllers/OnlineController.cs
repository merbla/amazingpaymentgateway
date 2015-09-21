using System;
using System.Web.Http;
using Serilog;

namespace APG.API.Controllers
{
    public class OnlineController : ApiController
    {
        public string Get()
        {
            Log.Information("gday conf 2015 : {0}", Guid.NewGuid().ToString());
            return "gday conf 2015 ";
        }
    }

}
