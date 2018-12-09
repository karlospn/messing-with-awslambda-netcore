using Microsoft.AspNetCore.Mvc;

namespace DummyWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("method1")]
        public ActionResult<string> GetMethod1()
        {
            return "method_1";
        }

        [HttpGet("method2")]
        public ActionResult<string> GetMethod2()
        {
            return "method_2";
        }

      
    }
}
