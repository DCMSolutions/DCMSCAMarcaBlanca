using BlazorApp1.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

namespace BlazorApp1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        [HttpGet]
        public string Get(string? url)
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "config.ans");
            if (!System.IO.File.Exists(fileName))
            {
                return "";
            }
            else
            {
                string content = System.IO.File.ReadAllText(fileName);
                Config? config = JsonSerializer.Deserialize<Config>(content);

                if (config != null && config.URLActual != null && config.URLActual != "")
                {
                    return config.URLActual;
                }
                else if (config != null && config.URLDefault != null)
                {
                    return config.URLDefault;
                }
                return "";
            }
        }


        [HttpGet("com")]
        public int GetCom()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "config.ans");
            if (!System.IO.File.Exists(fileName))
            {
                return 38;
            }
            else
            {
                string content = System.IO.File.ReadAllText(fileName);
                Config? config = JsonSerializer.Deserialize<Config>(content);
                if(config != null && config.COM.StartsWith("COM"))
                {
                    string numberPart = config.COM.Substring(3);

                    if (int.TryParse(numberPart, out int result))
                    {
                        return result;
                    }
                }
                return 38;
            }
        }
    }
}
