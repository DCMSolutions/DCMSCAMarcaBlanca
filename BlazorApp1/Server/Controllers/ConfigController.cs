using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using BlazorApp1.Shared.Model;

namespace BlazorApp1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private string fileName = Path.Combine(Directory.GetCurrentDirectory(), "config.ans");

        [HttpGet]
        public Config GetConfig()
        {
            try
            {
                if (!System.IO.File.Exists(fileName))
                {
                    Config conf = CrearNueva();
                    return conf;
                }
                else
                {
                    string content = System.IO.File.ReadAllText(fileName);
                    Config? config = JsonSerializer.Deserialize<Config>(content);
                    return config;
                }
            }
            catch
            {
                throw new Exception("Hubo un error al buscar la configuración");
            }
        }

        [HttpPost]
        public async Task<bool> EditConfig([FromBody] Config config)
        {
            try
            {
                if (!System.IO.File.Exists(fileName))
                {
                    CrearConConfig(config);
                    return true;
                }
                else
                {
                    string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(fileName, json);
                    return true;
                }
            }
            catch
            {
                throw new Exception("No se pudo editar la configuración");
            }
        }

        Config CrearNueva()
        {
            Config nuevaConf = new()
            {
                COM = "COM38",
                HasMensajeDenegado = false,
                MensajeDenegadoDefault = "Lectura inválida.",
                MensajeDisconnect = "No hay conexión al servidor.",
                TiempoDesconectado = 5000,
                TiempoAceptado = 5000,
                TiempoDenegado = 5000,
                AltoPestaña = 600,
                AnchoPestaña = 500,
            };

            string json = JsonSerializer.Serialize(nuevaConf, new JsonSerializerOptions { WriteIndented = true });
            using StreamWriter sw = System.IO.File.CreateText(fileName);
            sw.Write(json);

            return nuevaConf;
        }
        void CrearConConfig(Config config)
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            using StreamWriter sw = System.IO.File.CreateText(fileName);
            sw.Write(json);
        }
    }
}
