using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo; // Se importa el namespace que contiene las clases Pedido y Respuesta.

namespace Servidor
{
    class Servidor
    {
        // Escuchador TCP para aceptar conexiones.
        private static TcpListener escuchador;

        // Diccionario para llevar el conteo de solicitudes por cliente (clave: dirección IP y puerto).
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            try
            {
                // Se crea el servidor escuchando en cualquier dirección IP en el puerto 8080.
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 5000...");

                while (true)
                {
                    // Acepta una conexión entrante.
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());

                    // Se crea un nuevo hilo para manejar al cliente conectado.
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " + ex.Message);
            }
            finally
            {
                // Se detiene el servidor si ocurre una excepción o al finalizar.
                escuchador?.Stop();
            }
        }

        // Método que se ejecuta por cada cliente conectado en un hilo separado.
        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream(); // Se obtiene el flujo de datos del cliente.
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024]; // Buffer de recepción.
                int bytesRx;

                // Se mantiene escuchando mientras el cliente envíe datos.
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // Se decodifica el mensaje recibido.
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                    // Se procesa el mensaje para convertirlo en un objeto Pedido.
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    Console.WriteLine("Se recibió: " + pedido);

                    // Se obtiene la dirección del cliente (IP y puerto).
                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();

                    // Se resuelve el pedido y se genera una respuesta.
                    Respuesta respuesta = ResolverPedido(pedido, direccionCliente);
                    Console.WriteLine("Se envió: " + respuesta);

                    // Se envía la respuesta al cliente.
                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                // Se cierran los recursos al terminar.
                flujo?.Close();
                cliente?.Close();
            }
        }

        // Método que analiza el Pedido y genera una Respuesta adecuada.
        private static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            // Respuesta por defecto si el comando no es reconocido.
            Respuesta respuesta = new Respuesta
            {
                Estado = "NOK",
                Mensaje = "Comando no reconocido"
            };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    // Validación simple de usuario y contraseña.
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        // Se simula aleatoriamente el acceso concedido o negado.
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta { Estado = "OK", Mensaje = "ACCESO_CONCEDIDO" }
                            : new Respuesta { Estado = "NOK", Mensaje = "ACCESO_NEGADO" };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    // Se espera modelo, marca y placa como parámetros.
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];

                        // Validar formato de placa (por ejemplo: ABC1234).
                        if (ValidarPlaca(placa))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };

                            // Se incrementa el contador de solicitudes del cliente.
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    // Se devuelve el número de solicitudes previas del cliente.
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = listadoClientes[direccionCliente].ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }

            return respuesta;
        }

        // Verifica si la placa tiene el formato correcto: 3 letras seguidas de 4 dígitos.
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        // Obtiene un valor codificado (bitmask) que representa un día de la semana según el último dígito.
        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));

            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }

        // Aumenta el contador de solicitudes de un cliente, o lo inicializa si no existe.
        private static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++;
            }
            else
            {
                listadoClientes[direccionCliente] = 1;
            }
        }
    }
}
