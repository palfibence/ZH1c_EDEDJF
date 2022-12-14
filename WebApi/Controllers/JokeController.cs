using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi.Controllers
{
    [Route("api/jokes")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        // GET: api/jokes
        [HttpGet]
        public IActionResult Get()
        {
            Models.FunnyDatabaseContext context = new Models.FunnyDatabaseContext();
            return Ok(context.Jokes.ToList());
        }

        // GET api/jokes/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Models.FunnyDatabaseContext context = new Models.FunnyDatabaseContext();
            var keresettVicc = (from x in context.Jokes
                                where x.JokeSk == id
                                select x).FirstOrDefault();
            if (keresettVicc == null)
            {
                return NotFound($"Nincs #{id} azonosítóval vicc");
            }
            else
            {
                return Ok(keresettVicc);
            }
        }


        // GET api/jokes/szoveg/5
        [HttpGet("szoveg/{id}")]
        public IActionResult Get(string id)
        {
            Models.FunnyDatabaseContext context = new Models.FunnyDatabaseContext();
            var keresettVicc = (from x in context.Jokes
                                where x.JokeText.Contains(id)
                                select x).FirstOrDefault();
            if (keresettVicc == null)
            {
                return NotFound($"Nincs ilyen vicc: {id}");
            }
            else
            {
                return Ok(keresettVicc);
            }
        }

        // POST api/jokes
        [HttpPost]
        public void Post([FromBody] Models.Joke újVicc)
        {
            Models.FunnyDatabaseContext context = new Models.FunnyDatabaseContext();
            context.Jokes.Add(újVicc);
            context.SaveChanges();
        }

        // PUT api/<JokeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/jokes/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Models.FunnyDatabaseContext context = new Models.FunnyDatabaseContext();
            var törlendőVicc = (from x in context.Jokes
                                where x.JokeSk == id
                                select x).FirstOrDefault();
            context.Remove(törlendőVicc);
            context.SaveChanges();
        }
    }
}
