using System.Threading;

namespace BlazorApp1.Server.Test
{
    public class MessageService : IHostedService
    {
        private readonly string _messageQueue;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ILogger<MessageService> logger)
        {
            
            _logger = logger;
        }
        public async Task test(string messageQueue)
        {
            Console.WriteLine("HOLA GANE");
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Aquí puedes iniciar cualquier proceso que quieras ejecutar en segundo plano.
            // Puedes usar _messageQueue para recibir mensajes y _logger para registrar información.

            // Ejemplo: Iniciar un bucle de procesamiento de mensajes.
            Task.Run(ProcessMessages);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Realiza tareas de limpieza si es necesario al detener el servicio.
            return Task.CompletedTask;
        }

        private async Task ProcessMessages()
        {
            // Lógica para procesar mensajes desde la cola y escribirlos en un archivo de texto.
            // Puedes usar _messageQueue para recibir mensajes y _logger para registrar información.

            
        }
    }

}
