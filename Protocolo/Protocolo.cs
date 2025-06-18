using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Protocolo
{
    // ************************************************************************
    // Practica 07
    // Miguel Avilez
    // Fecha de realización: 11/06/2025
    // Fecha de entrega: 18/06/2025
    // Resultados:
    // * Se modifica el código del cliente y del servidor para que los métodos HazOperación, ResolverPedido sean implementados en la clase
    // Protocolo. Tanto el cliente como el servidor deben usar Protocolo, ya no Pedido y Respuesta.
    // Conclusiones:
    // Miguel Avilez
    // * En síntesis, esta herramienta potencia el trabajo colaborativo al permitir que los equipos trabajen de manera
    // simultánea y ordenada sobre un mismo proyecto, sin importar su ubicación geográfica. Gracias a su sistema de ramas y
    // fusiones, cada integrante puede desarrollar nuevas funcionalidades o corregir errores sin afectar el trabajo de los demás,
    // asegurando así un desarrollo más seguro y estructurado.
    // * Además, actúa como una plataforma de respaldo y de historial del proyecto, permitiendo recuperar versiones anteriores
    // del código y entender cómo ha evolucionado.Esta trazabilidad no solo mejora la calidad del software,
    // sino que también facilita la incorporación de nuevos miembros al equipo, quienes pueden revisar el historial de cambios y
    // entender rápidamente el estado actual del proyecto.

    // Recomendaciones:
    // Miguel Avilez
    // * Se debe utilizar pull requests para revisar y discutir los cambios antes de integrarlos a la rama principal, ya
    // que esto fomenta la colaboración, mejora la calidad del código y permite detectar errores o inconsistencias a tiempo.
    // * Se recomienda, documentar adecuadamente tu proyecto utilizando el archivo README y, si es posible, una wiki, ya que
    // esto facilita la comprensión del objetivo, el uso y la estructura del código tanto para el equipo como para futuros colaboradores.

    // ************************************************************************
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
