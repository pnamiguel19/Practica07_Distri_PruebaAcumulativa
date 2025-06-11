using System.Linq; 

namespace Protocolo
{
    // Clase que representa una solicitud o instrucción recibida (Pedido).
    public class Pedido
    {
        // Propiedad que almacena el comando principal de la solicitud.
        public string Comando { get; set; }

        // Propiedad que almacena los parámetros asociados al comando.
        public string[] Parametros { get; set; }

        // Método estático que procesa un mensaje recibido y lo convierte en un objeto Pedido.
        public static Pedido Procesar(string mensaje)
        {
            // Se divide el mensaje por espacios.
            var partes = mensaje.Split(' ');

            // Se crea un nuevo objeto Pedido con el primer elemento como comando y el resto como parámetros.
            return new Pedido
            {
                Comando = partes[0].ToUpper(),         // Se convierte el comando a mayúsculas.
                Parametros = partes.Skip(1).ToArray()  // Se omite el primer elemento (el comando) y se guarda el resto como parámetros.
            };
        }

        // Sobrescribe el método ToString para mostrar el comando seguido de los parámetros separados por espacio.
        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    // Clase que representa una respuesta a un Pedido.
    public class Respuesta
    {
        // Propiedad que indica el estado de la respuesta (por ejemplo, "OK" o "ERROR").
        public string Estado { get; set; }

        // Propiedad que contiene un mensaje adicional que acompaña al estado.
        public string Mensaje { get; set; }

        // Sobrescribe el método ToString para mostrar el estado seguido del mensaje.
        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }
}
