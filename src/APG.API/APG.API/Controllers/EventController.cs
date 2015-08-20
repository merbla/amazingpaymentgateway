using System.Collections.Generic;
using System.Web.Http;

namespace APG.API.Controllers
{
    public class EventController : ApiController
    {
        // GET: api/Event
        public IEnumerable<string> Get()
        {
            return new string[] { "Foo", "Bar" };
        }

        // GET: api/Event/5
        public string Get(int id)
        {
            return $"Foo-{id}";
        }

    }
}
