using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BlazorApp1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        [HttpGet]
        public string Get(string? url)
        {
            if (url == null)
            {
                string fileName = Path.Combine(Directory.GetCurrentDirectory(), "url.ans");

                if (!System.IO.File.Exists(fileName))
                {
                    url = "https://www.vicentelopez.gov.ar/mibarriogestion/deportesws/api/access?documentNumber=";
                    using (StreamWriter sw = System.IO.File.CreateText(fileName))
                    {
                        sw.Write(url);
                    }
                }
                else
                {
                    url = System.IO.File.ReadAllText(fileName);

                }
            }
            else
            {
                Console.WriteLine(url);
                string fileName = Path.Combine(Directory.GetCurrentDirectory(), "url.ans");
                try
                {

                    using (StreamWriter sw = System.IO.File.CreateText(fileName))
                    {
                        sw.Write(url);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al leer el archivo url.ans: " + ex.Message);
                }
            }
            return url;
        }
        [HttpGet("com")]
        public int GetCom()
        {
            int com;
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "lastCOM.ans");

            if (!System.IO.File.Exists(fileName))
            {
                using (StreamWriter sw = System.IO.File.CreateText(fileName))
                {
                    com = 0;
                    sw.Write($"COM{com}");
                }
            }
            else
            {
                string read = System.IO.File.ReadAllText(fileName);
                string comPort = read.TrimStart("COM".ToCharArray());
                com = (Convert.ToInt32(comPort));

            }
            return com;
        }
    }
}
