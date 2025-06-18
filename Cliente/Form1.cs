using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        // Cliente TCP y flujo de red
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent(); // Inicializa los componentes gráficos
        }

        // Evento que se ejecuta al cargar el formulario
        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Intenta conectarse al servidor local en el puerto 8080
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión: " + ex.Message, "ERROR");
            }

            // Desactiva los controles hasta que se inicie sesión
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
        }

        // Evento del botón "Iniciar sesión"
        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave = txtPassword.Text.Trim();

            if (usuario == "" || clave == "")
            {
                MessageBox.Show("Se requiere usuario y contraseña", "ADVERTENCIA");
                return;
            }

            // Envia el comando "INGRESO" al servidor con usuario y clave
            string respuesta = Protocolo.Protocolo.HazOperacion(flujo, "INGRESO", new[] { usuario, clave });

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            // Si el acceso es concedido, habilita el panel de consulta
            if (respuesta.StartsWith("OK") && respuesta.Contains("ACCESO_CONCEDIDO"))
            {
                panLogin.Enabled = false;
                panPlaca.Enabled = true;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else
            {
                MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                txtUsuario.Focus();
            }
        }

        // Evento del botón "Consultar restricción"
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text.Trim();
            string marca = txtMarca.Text.Trim();
            string placa = txtPlaca.Text.Trim();

            // Envia el comando "CALCULO" con modelo, marca y placa
            string respuesta = Protocolo.Protocolo.HazOperacion(flujo, "CALCULO", new[] { modelo, marca, placa });

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.StartsWith("NOK"))
            {
                MessageBox.Show("Error en la solicitud", "ERROR");
                LimpiarChecks();
                return;
            }

            // Divide la respuesta y extrae el valor binario de los días
            string[] partes = respuesta.Split(' ');
            if (partes.Length < 2)
            {
                MessageBox.Show("Respuesta inválida", "ERROR");
                return;
            }

            MessageBox.Show("Se recibió: " + respuesta, "INFORMACIÓN");

            if (byte.TryParse(partes[1], out byte resultado))
            {
                // Marca los checkboxes según los bits recibidos
                chkLunes.Checked = (resultado & 0b00100000) != 0;
                chkMartes.Checked = (resultado & 0b00010000) != 0;
                chkMiercoles.Checked = (resultado & 0b00001000) != 0;
                chkJueves.Checked = (resultado & 0b00000100) != 0;
                chkViernes.Checked = (resultado & 0b00000010) != 0;
            }
            else
            {
                MessageBox.Show("No se pudo interpretar el resultado", "ERROR");
            }
        }

        // Evento del botón "Número de consultas"
        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            // Envia el comando "CONTADOR" al servidor
            string respuesta = Protocolo.Protocolo.HazOperacion(flujo, "CONTADOR", new[] { "consulta" });

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.StartsWith("OK"))
            {
                MessageBox.Show("Número de solicitudes: " + respuesta.Substring(3), "INFORMACIÓN");
            }
            else
            {
                MessageBox.Show("Error al consultar número de solicitudes", "ERROR");
            }
        }

        // Evento que se ejecuta al cerrar el formulario
        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            flujo?.Close();   // Cierra el flujo si está activo
            remoto?.Close();  // Cierra la conexión con el servidor
        }

        // Método auxiliar para desmarcar todos los checkboxes
        private void LimpiarChecks()
        {
            chkLunes.Checked = false;
            chkMartes.Checked = false;
            chkMiercoles.Checked = false;
            chkJueves.Checked = false;
            chkViernes.Checked = false;
        }
    }
}
