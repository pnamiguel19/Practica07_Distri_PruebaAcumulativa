using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Protocolo
{
    public class Protocolo
    {
        // Envía un comando con parámetros por el flujo de red y recibe la respuesta.
        public static string HazOperacion(NetworkStream flujo, string comando, string[] parametros)
        {
            try
            {
                // Construye el mensaje: comando + parámetros separados por espacios
                string textoPedido = comando + " " + string.Join(" ", parametros);

                // Codifica el mensaje en bytes y lo envía al servidor
                byte[] bufferTx = Encoding.UTF8.GetBytes(textoPedido);
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Espera la respuesta del servidor
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                // Decodifica y retorna la respuesta
                return Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
            }
            catch
            {
                // Si ocurre un error (por ejemplo, desconexión), retorna null
                return null;
            }
        }

        // Procesa un pedido recibido y retorna la respuesta correspondiente
        public static string ResolverPedido(string pedido)
        {
            // Divide el pedido en comando y parámetros
            string[] partes = pedido.Trim().Split(' ');
            if (partes.Length == 0) return "NOK Comando inválido";

            string comando = partes[0];
            string[] parametros = partes.Skip(1).ToArray();

            // Procesa según el tipo de comando recibido
            switch (comando)
            {
                case "INGRESO":
                    // Valida que haya al menos usuario y contraseña
                    if (parametros.Length < 2)
                        return "NOK Faltan parámetros";

                    // Verifica credenciales
                    return (parametros[0] == "root" && parametros[1] == "admin20")
                        ? "OK ACCESO_CONCEDIDO"
                        : "NOK ACCESO_DENEGADO";

                case "CALCULO":
                    // Valida que tenga al menos tres parámetros
                    if (parametros.Length < 3)
                        return "NOK Faltan datos";

                    // Toma la placa y obtiene los días de restricción
                    string placa = parametros[2];
                    byte dias = ObtenerDiasRestriccion(placa);
                    return "OK " + dias;

                case "CONTADOR":
                    // Devuelve un valor fijo (puede ser reemplazado por un contador real)
                    return "OK 5";

                default:
                    // Si el comando no es reconocido
                    return "NOK Comando no reconocido";
            }
        }

        // Determina los días de restricción según el último dígito de la placa
        private static byte ObtenerDiasRestriccion(string placa)
        {
            // Valida que la placa no sea nula y que termine en un dígito
            if (string.IsNullOrEmpty(placa) || !char.IsDigit(placa[placa.Length - 1]))
                return 0;

            int digito = int.Parse(placa[placa.Length - 1].ToString());

            // Asocia cada par de dígitos con un día específico
            if (digito == 1 || digito == 2)
                return 0b00100000; // Lunes
            else if (digito == 3 || digito == 4)
                return 0b00010000; // Martes
            else if (digito == 5 || digito == 6)
                return 0b00001000; // Miércoles
            else if (digito == 7 || digito == 8)
                return 0b00000100; // Jueves
            else if (digito == 9 || digito == 0)
                return 0b00000010; // Viernes
            else
                return 0;
        }
    }
}
