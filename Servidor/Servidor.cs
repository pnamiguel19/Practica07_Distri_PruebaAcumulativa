using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Protocolo; // Se importa la clase Protocolo para resolver los pedidos recibidos

namespace Servidor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Crea un servidor TCP que escucha en el puerto 8080 en cualquier interfaz de red
            TcpListener servidor = new TcpListener(IPAddress.Any, 8080);
            servidor.Start(); // Inicia el servidor
            Console.WriteLine("Servidor iniciado en puerto 8080");

            while (true) // Bucle infinito para aceptar múltiples clientes
            {
                Console.WriteLine("\nEsperando cliente...");

                // Espera y acepta la conexión de un cliente
                TcpClient cliente = servidor.AcceptTcpClient();
                Console.WriteLine("Cliente conectado: " + cliente.Client.RemoteEndPoint);

                // Obtiene el flujo de datos para comunicarse con el cliente
                NetworkStream flujo = cliente.GetStream();

                try
                {
                    byte[] bufferRx = new byte[1024]; // Buffer para recibir datos
                    int bytesLeidos;

                    // Mientras el cliente siga enviando datos, se mantiene la conexión activa
                    while ((bytesLeidos = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                    {
                        // Decodifica los datos recibidos en una cadena de texto
                        string pedido = Encoding.UTF8.GetString(bufferRx, 0, bytesLeidos);
                        Console.WriteLine("Pedido recibido: " + pedido);

                        // Procesa el pedido recibido utilizando la lógica del protocolo
                        string respuesta = Protocolo.Protocolo.ResolverPedido(pedido);
                        Console.WriteLine("Respuesta enviada: " + respuesta);

                        // Codifica la respuesta y la envía al cliente
                        byte[] bufferTx = Encoding.UTF8.GetBytes(respuesta);
                        flujo.Write(bufferTx, 0, bufferTx.Length);
                    }

                    // Cuando el cliente deja de enviar datos, se cierra la conexión
                    Console.WriteLine("Cliente desconectado.");
                }
                catch (Exception ex)
                {
                    // Captura cualquier error durante la comunicación
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    // Cierra el flujo y la conexión con el cliente
                    flujo.Close();
                    cliente.Close();
                }
            }
        }
    }
}
