using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MIB.Controllers
{
    public class FuckController : ApiController
    {
        [HttpGet]
        public string Index()
        {
            return "123";
        }
    }
}
