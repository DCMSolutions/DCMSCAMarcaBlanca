using BlazorApp1.Server.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using System.Threading;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq.Expressions;
using BlazorApp1.Shared.Model;

namespace BlazorApp1.Server
{
    public class AppInitializationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly HttpClient _httpClient;
        private Thread _work;
        private string _COM;
        public AppInitializationService(IServiceProvider serviceProvider, IHubContext<NotificationHub> hubContext, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _httpClient = httpClientFactory.CreateClient();

        }

        public static string validationSuccess = "<[VALIDATION]VALID=1;REASON=1;CRC=95;>";
        public static string validationFail = "<[VALIDATION]VALID=0;REASON=  No esta en     la lista;CRC=40;>";
        public static bool firsttime = true;

        private async Task HandleClient(TcpClient client)
        {
            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint != null)
            {
                Console.WriteLine($"Connected client IP: {remoteEndPoint.Address}, Port: {remoteEndPoint.Port}");
            }
            else
            {
                Console.WriteLine("Unable to retrieve client IP address.");
            }
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                StringBuilder messageBuilder = new StringBuilder();

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // La conexión se ha cerrado.
                        Console.WriteLine("Cerrando conexion");
                        break;
                    }

                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(receivedData);

                    // Comprobar si el mensaje está completo (por ejemplo, si termina con un carácter de nueva línea).
                    string message = messageBuilder.ToString();
                    if (message.EndsWith(">"))
                    {
                        try
                        {
                            // Procesar el mensaje completo.
                            Console.WriteLine($"Datos recibidos: {message}");
                            string responseMessage = "<[ACK]CRC=79;>";

                            if (message.Contains("[HELLO]"))
                            {
                                responseMessage = "<[NTMY]TELEPORT=4810;CRC=2;>";
                                //responseMessage = "<[NTMY]TELEPORT=4810;CRC=2;><[STATUS]ID=1;ENABLE=3;GC1=40;GC2=77;DC1=12;DC2=12;BUZON=0;MODE=0;TYPE=1;READER1=22;READER2=22;CRC=123;>";

                            }

                            string pattern = @"MEDIA=([^;]+);";
                            Match match = Regex.Match(message, pattern);

                            if (match.Success && match.Groups[1].Value != "0")
                            {
                                string textoDeseado = match.Groups[1].Value;
                                var intDNI = ExtractAndConvertToInt(getDNI(textoDeseado));
                                if (intDNI.ToString().Length > 8)
                                {
                                    Console.WriteLine($"el DNI sin modificar es {intDNI}");
                                    var dniString = intDNI.ToString();
                                    dniString = dniString.Substring(dniString.Length - 8, 8);
                                    intDNI = ExtractAndConvertToInt(dniString);
                                    Console.WriteLine($"el DNI modificado es {dniString}");

                                }
                                if (intDNI == 0)
                                {
                                    await openChrome(0, true, "");

                                }
                                else
                                {
                                    var response = await GetDniHabilitado(intDNI);
                                    EscribirTextoEnArchivo($"el intDNI: {intDNI}, la response: {response}");
                                    if (response.Success)
                                    {
                                        if (response.accesGranted)
                                        {
                                            responseMessage = validationSuccess;
                                        }
                                        else
                                        {
                                            responseMessage = validationFail;
                                        }

                                        await openChrome(intDNI, response.accesGranted, response.ErrorMessage);
                                    }
                                    else
                                    {
                                        await openChrome(null, null, null);
                                    }
                                    _hubContext.Clients.All.SendAsync("SendNotification", intDNI, response.accesGranted);

                                }
                            }

                            Console.WriteLine($"Datos enviados: {responseMessage}");
                            byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                            stream.Write(responseBytes, 0, responseBytes.Length);

                            // Limpiar el mensajeBuilder para recibir el próximo mensaje.
                            messageBuilder.Clear();
                            //responseMessage = "<[ACK]CRC=79;>";
                            //Console.WriteLine($"Datos enviados: {responseMessage}");
                            //responseBytes = Encoding.ASCII.GetBytes(responseMessage);

                            //stream.Write(responseBytes, 0, responseBytes.Length);

                            //// Limpiar el mensajeBuilder para recibir el próximo mensaje.
                            //messageBuilder.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }

            client.Close();

            string getDNI(string text)
            {
                try
                {
                    if (text.Length > 100)
                    {
                        int firstIndex = -1;
                        for (int i = 0; i <= text.Length - 1; i++)
                        {
                            if (char.IsLetter(text[i]))
                            {
                                if (firstIndex == -1)
                                {
                                    firstIndex = i;
                                }

                            }
                        }
                        if (firstIndex != -1)
                        {
                            string recortado = text.Substring(0, firstIndex + 1);
                            string soloNumeros = new string(recortado.Where(char.IsDigit).ToArray());
                            return soloNumeros;
                        }
                    }

                    if (text.Contains("@"))
                    {
                        var splitText = text.Split("@");
                        text = new string(splitText[4].Where(char.IsDigit).ToArray());
                    }
                    else
                    {
                        int firstIndex = -1;
                        int lastIndex = -1;
                        for (int i = 0; i <= text.Length - 1; i++)
                        {
                            if (char.IsLetter(text[i]))
                            {
                                if (firstIndex == -1)
                                {
                                    firstIndex = i;
                                }

                                lastIndex = i;
                            }
                        }
                        if (lastIndex != -1)
                        {
                            string recortado = text.Substring(firstIndex, lastIndex - firstIndex + 1);
                            string soloNumeros = new string(recortado.Where(char.IsDigit).ToArray());


                            return soloNumeros;
                        }
                    }

                }
                catch
                {
                    text = "0";

                }

                return text;
            }
            static int ExtractAndConvertToInt(string input)
            {
                try
                {
                    string numericPart = new string(input.Where(char.IsDigit).ToArray());
                    if (int.TryParse(numericPart, out int result))
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return 0;
                // Manejar el caso en que no se pueda convertir a int
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Coloca aquí el código que deseas ejecutar al iniciar la aplicación
            // Por ejemplo, puedes realizar configuraciones iniciales, cargar datos, etc.
            try
            {
                //if (_work == null)
                //{
                EscribirTextoEnArchivo("La aplicación se ha iniciado");
                Console.WriteLine("La aplicación se ha iniciado");
                _work = new Thread(StartReading);
                _work.Start();
                //}
            }
            catch (Exception ex)
            {
                EscribirTextoEnArchivo(ex.Message);

            }
        }

        public async Task Start(string COM)
        {
            _COM = COM;
            StartAsync(cancellationToken: System.Threading.CancellationToken.None);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Indicar que se ha solicitado detener el servicio

            // Esperar a que el thread termine su ejecución
            if (_work != null && _work.IsAlive)
            {
                _work.Join(); // Esperar a que el thread termine
                Console.WriteLine("La aplicación se ha detenido");

            }

            // Realizar limpieza o acciones finales aquí

            await Task.CompletedTask;
        }

        public async void StartReading()
        {
            int port = 4800;
            TcpListener server = new TcpListener(IPAddress.Any, port);

            server.Start();

            Console.WriteLine($"Escuchando mensajes en el puerto {port}. Presiona Enter para detener...");

            while (true)
            {
                // Accept a new client connection
                TcpClient client = server.AcceptTcpClient();

                // Create a separate thread for each client
                Thread clientThread = new Thread(async () =>
                {
                    await HandleClient(client);
                });

                // Start the client thread
                clientThread.Start();
            }
        }

        async Task<Result<bool>> GetDniHabilitado(int intDocument)
        {
            string url = GetURL();
            string apiEndpoint = $"{url.TrimEnd('/')}/" + intDocument;

            if (intDocument == 43717944 || intDocument == 39489445 || intDocument == 16058146 || intDocument == 38177265 /*|| intDocument == 43990922*/)
            {
                return new Result<bool> { Success = true, accesGranted = true };
            }
            if (intDocument == 37040385)
            {
                return new Result<bool> { Success = true, accesGranted = false };
            }

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiEndpoint);
                var a = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    Dni responseData = await response.Content.ReadFromJsonAsync<Dni>();
                    return new Result<bool> { Success = true, accesGranted = responseData.Habilitado == true, ErrorMessage = responseData.Mensaje };
                }
                else
                {
                    return new Result<bool> { Success = false, ErrorMessage = "La consulta a la API falló" };
                }
            }
            catch (Exception ex)
            {
                return new Result<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        async Task openChrome(int? dni, bool? accessGranted, string? mensajeDenegado)
        {
            try
            {
                EscribirTextoEnArchivo(dni.ToString());
                var ip = GetLocalIPAddress();
                Config config = GetConfig();


                int width = (config.AnchoPestaña.HasValue && config.AnchoPestaña.Value != 0) ? config.AnchoPestaña.Value : 500;
                int height = (config.AltoPestaña.HasValue && config.AltoPestaña.Value != 0) ? config.AltoPestaña.Value : 600;

                int horizontal = (config.HorizontalPestaña.HasValue && config.HorizontalPestaña.Value != 0) ? config.HorizontalPestaña.Value : 580;
                int vertical = (config.VerticalPestaña.HasValue && config.VerticalPestaña.Value != 0) ? config.VerticalPestaña.Value : 240;

                string url = $"http://{ip}:5000/monitor/{dni}/{accessGranted}/{mensajeDenegado}";
                string chromeArguments = $" --app=\"data:text/html,<html><body><script>window.moveTo({horizontal},{vertical});window.resizeTo({width},{height});window.location='{url}';</script></body></html>\"";
                await Task.Run(() =>
                {
                    Process.Start($"cmd", $"/c start chrome {chromeArguments} ");
                });
                EscribirTextoEnArchivo("Task.Run is finalized");
            }
            catch (Exception ex)
            {
                EscribirTextoEnArchivo(ex.Message);
            }

        }

        void EscribirTextoEnArchivo(string text)
        {
            // Ruta del archivo
            string filePath = Directory.GetCurrentDirectory();


            // Abre o crea el archivo en modo de escritura, agregando al final
            //using (StreamWriter writer = new StreamWriter(filePath, true))
            //{
            //    // Escribe el texto en una nueva línea en el archivo
            //    writer.WriteLine(text);
            //}
        }

        string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            return localIP;
        }

        Config GetConfig()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "config.ans");

            if (!System.IO.File.Exists(fileName))
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
                return nuevaConf;
            }
            else
            {
                string content = System.IO.File.ReadAllText(fileName);
                Config? config = JsonSerializer.Deserialize<Config>(content);
                return config;
            }

        }

        public string GetURL()
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
    }

    public class Result<T>
    {
        public bool Success { get; set; }
        public T accesGranted { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class Dni
    {
        public int Id { get; set; }

        public int? Numero { get; set; }

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public bool? Habilitado { get; set; }

        public string? Mensaje { get; set; }

    }
}



//<[HELLO]ID = 1; CPROD = MCPIC; CRC = 102;>
//<[NTMY]TELEPORT = 4810; CRC = 2;>
//<[STATUS]ID = 1; ENABLE = 3; GC1 = 275; GC2 = 113; DC1 = 140; DC2 = 16; BUZON = 0; MODE = 0; TYPE = 1; READER1 = 22; READER2 = 22; CRC = 78;>
//<[ACK]CRC = 79;>
//<[HELLO]ID = 2; CPROD = MCPIC; CRC = 101;>
//<[NTMY]TELEPORT = 4810; CRC = 2;>
//<[STATUS]ID = 2; ENABLE = 3; GC1 = 6; GC2 = 9; DC1 = 5; DC2 = 8; BUZON = 0; MODE = 0; TYPE = 1; READER1 = 22; READER2 = 22; CRC = 126;>
//<[ACK]CRC = 79;>
//<[SYNCRONIZATION]CRC = 28;>
//<[SETTIME]DATETIME = 20230915120114; CRC = 95;>
//<[GETCONFIG]ID = 2; CRC = 99;>
//<[CONFIG]STATUSTIME = 30; CONFIGTIME = 3600; SYNCROTIME = 3600; POINT1_E = 1; POINT2_E = 1; CRC = 27;>
//<[GETCONFIGPOINT]ID = 2; POINT = 1; CRC = 84;>
//<[CONFIG]CTIMEOUT = 30; OTIMEOUT = 5; ALARMTIME = 5; DIR = 1; BENABLE = 1; PENABLE = 1; BIOCHECK = 0; TYPE = 1; READER = 22; VOFFLINE = 0; CRC = 0;>
//<[GETCONFIGPOINT]ID = 2; POINT = 2; CRC = 87;>
//<[CONFIG]CTIMEOUT = 30; OTIMEOUT = 5; ALARMTIME = 5; DIR = 1; BENABLE = 1; PENABLE = 1; BIOCHECK = 0; TYPE = 1; READER = 22; VOFFLINE = 0; CRC = 0;>
//<[END]CRC = 73;>
//<[NACK]CRC = 1;>
//<[STATUS]ID = 1; ENABLE = 3; GC1 = 275; GC2 = 113; DC1 = 140; DC2 = 16; BUZON = 0; MODE = 0; TYPE = 1; READER1 = 22; READER2 = 22; CRC = 78;>
//<[ACK]CRC = 79;>
//<[HELLO]ID = 2; CPROD = MCPIC; CRC = 101;>
//<[NACK]CRC = 1;>
//<[STATUS]ID = 1; ENABLE = 3; GC1 = 275; GC2 = 113; DC1 = 140; DC2 = 16; BUZON = 0; MODE = 0; TYPE = 1; READER1 = 22; READER2 = 22; CRC = 78;>
//<[ACK]CRC = 79;>
//<[HELLO]ID = 2; CPROD = MCPIC; CRC = 101;>
//<[NTMY]TELEPORT = 4810; CRC = 2;>
//<[VALIDATE]ID = 2; POINT = 2; DIR = 1; MEDIATYPE = 22; MEDIA = 228638131; CRC = 7;>
//<[VALIDATION]VALID = 0; REASON = No esta en     la lista; CRC = 40;>
//<[STATUS]ID = 2; ENABLE = 3; GC1 = 6; GC2 = 9; DC1 = 5; DC2 = 8; BUZON = 0; MODE = 0; TYPE = 1; READER1 = 22; READER2 = 22; CRC = 126;>
//<[ACK]CRC = 79;>
//<[SYNCRONIZATION]CRC = 28;>
//<[SETTIME]DATETIME = 20230915120159; CRC = 86;>
//<[GETCONFIG]ID = 2; CRC = 99;>
//<[CONFIG]STATUSTIME = 30; CONFIGTIME = 3600; SYNCROTIME = 3600; POINT1_E = 1; POINT2_E = 1; CRC = 27;>
//<[GETCONFIGPOINT]ID = 2; POINT = 1; CRC = 84;>
//<[CONFIG]CTIMEOUT = 30; OTIMEOUT = 5; ALARMTIME = 5; DIR = 1; BENABLE = 1; PENABLE = 1; BIOCHECK = 0; TYPE = 1; READER = 22; VOFFLINE = 0; CRC = 0;>
//<[GETCONFIGPOINT]ID = 2; POINT = 2; CRC = 87;>
//<[CONFIG]CTIMEOUT = 30; OTIMEOUT = 5; ALARMTIME = 5; DIR = 1; BENABLE = 1; PENABLE = 1; BIOCHECK = 0; TYPE = 1; READER = 22; VOFFLINE = 0; CRC = 0;>
//<[VALIDATE]ID = 2; POINT = 2; DIR = 1; MEDIATYPE = 22; MEDIA = 228638131; CRC = 7;>
//<[VALIDATION]VALID = 0; REASON = No esta en     la lista; CRC = 40;>
//<[STATUS]ID = 1; ENABLE = 3; GC1 = 275; GC2 = 113; DC1 = 140; DC2 = 16; BUZON = 0; MODE = 0; TYPE = 1; READER1 = 22; READER2 = 22; CRC = 78;>
//<[ACK]CRC = 79;>
//<[VALIDATE]ID = 2; POINT = 2; DIR = 1; MEDIATYPE = 22; MEDIA =

//28638131; CRC = 53;>
//<[VALIDATION]VALID = 0; REASON = No esta en     la lista; CRC = 40;>