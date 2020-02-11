using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessagingToolkit.Barcode;
using System.Net.Mail;
using System.Net;

namespace Farmacia
{
    public partial class Form1 : Form
    {
        ConectarBD cnx;
        List<Medicamento> listaMedicamentos = new List<Medicamento>();
        List<Medicamento> listaCesta = new List<Medicamento>();
        List<Medicamento> listaEncontrados = new List<Medicamento>();
        String pdfTicket;
        double total = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add("Total: ", total);
            cnx = new ConectarBD();
            listaMedicamentos = cnx.listar();
            cargarTPV(listaMedicamentos);
        }

        private void cargarTPV(List<Medicamento> listaMedicamentos)
        {
            int nFilas = Convert.ToInt16(numericUpDown1.Value);
            int nColumnas = Convert.ToInt16(numericUpDown2.Value);
            int alto = tabControl1.Height - 50;
            int ancho = tabControl1.Width - 50;
            int dimension = nFilas * nColumnas;
            int nPantallas;
            if (listaMedicamentos.Count % dimension == 0)
            {
                nPantallas = listaMedicamentos.Count / dimension;
            }
            else
            {
                nPantallas = (listaMedicamentos.Count / dimension) + 1;
            }

            //Construir un tabpage cada 16 medicamentos
            int indiceLista = 0;
            for (int indicePaneles = 0; indicePaneles <= nPantallas; indicePaneles++)
            {
                TabPage tp = new TabPage(Convert.ToString(indicePaneles + 1));
                tabControl1.Controls.Add(tp);
                TableLayoutPanel tlp = new TableLayoutPanel();
                tlp.AutoSize = true;
                tlp.RowCount = nFilas;
                tlp.ColumnCount = nColumnas;
                tp.Controls.Add(tlp);

                for (int i = 0; i < dimension; i++)
                {
                    Button botonX = new Button();
                    botonX.Height = alto / nFilas;
                    botonX.Width = ancho / nColumnas;
                    botonX.Tag = indiceLista;
                    botonX.Click += new EventHandler((sender, e)=>agregarProducto(sender, e, listaMedicamentos));
                    botonX.MouseHover += new EventHandler((sender, e)=>mostrar_Informacion(sender, e, listaMedicamentos));
                    //Cargar la imagen a través de un objeto MemoryStream
                    MemoryStream ms = new MemoryStream(listaMedicamentos[indiceLista].Imagen);
                    botonX.BackgroundImage = Image.FromStream(ms);
                    //Ajustar la imagen a tamaño del boton
                    botonX.BackgroundImageLayout = ImageLayout.Stretch;
                    //Añadir boton al contenedor
                    tlp.Controls.Add(botonX);
                    indiceLista++;
                }
            }
        }

        private void agregarProducto(object sender, EventArgs e, List<Medicamento> listaMed)
        {
            Button boton = (Button)sender;
            int posicion = Convert.ToInt16(boton.Tag);
            if (listaMed[posicion].Stockactual > 0)
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.RowCount - 1);
                dataGridView1.Rows.Add(listaMed[posicion].Nombre, String.Format("{0:0.00}", listaMed[posicion].Precio));
                total += Math.Round(listaMed[posicion].Precio, 2);
                dataGridView1.Rows.Add("Total: ", total);
                listaCesta.Add(listaMed[posicion]);
                lbTotal.Text = Convert.ToString(total);
            }
            else
            {
                MessageBox.Show("No queda "+ listaMed[posicion].Nombre + " en el almacén ");
            }
        }

        private void mostrar_Informacion(object sender, EventArgs e, List<Medicamento> listaMed)
        {
            Button boton = (Button)sender;
            int posicion = Convert.ToInt16(boton.Tag);
            lbNombre.Text = listaMed[posicion].Nombre;
            lbPrecio.Text = Convert.ToString(Math.Round(listaMed[posicion].Precio, 2));
            lbStockMin.Text = Convert.ToString(listaMed[posicion].Stockactual);
            lbStockActual.Text = Convert.ToString(listaMed[posicion].Stockminimo);
        }



        private void btnRecargar_Click(object sender, EventArgs e)
        {
            tabControl1.Controls.Clear();
            listaMedicamentos.Clear();
            listaMedicamentos = cnx.listar();
            try
            {
                cargarTPV(listaMedicamentos);
            } catch (Exception ex)
            {

            }

        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            pdfTicket=imprimirTicket();
            actualizarTabla();
            insertarFactura();
            mandarMail(pdfTicket);
            
            limpiar();
        }

        private void mandarMail(String pdfTicket)
        {
            try
            {
                string email = "ivan77program@gmail.com";
                string password = txtPasswordEmpresa.Text;

                var loginInfo = new NetworkCredential(email, password);
                var msg = new MailMessage();
                var smtpClient = new SmtpClient("smtp.gmail.com", 25);

                msg.From = new MailAddress(email);
                msg.To.Add(new MailAddress(txtCliente.Text));
                msg.Subject = "Factura";
                msg.Body = "contenido";
                Attachment ficheroAdjunto = new Attachment(pdfTicket);
                msg.Attachments.Add(ficheroAdjunto);

                msg.IsBodyHtml = true;

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = loginInfo;
                smtpClient.Send(msg);
                smtpClient.Dispose();
            }catch(Exception ex)
            {

            }
            
        }

        private void insertarFactura()
        {
            cnx.insertarFactura(listaCesta, txbUser.Text, total);
        }

        private void actualizarTabla()
        {
            cnx.lanzarActualizacion(listaCesta);
            listaMedicamentos.Clear();
            listaMedicamentos = cnx.listar();
        }

        private String imprimirTicket()
        {
            //Crear Tabla iTextSharp  desde una tabla de datos (datagridView)
            PdfPTable pdfTable = new PdfPTable(dataGridView1.ColumnCount);

            //padding
            pdfTable.DefaultCell.Padding = 3;

            //ancho que va a ocupar la tabla en el pdf
            pdfTable.WidthPercentage = 80;

            //alineación
            pdfTable.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;

            //borde de las tablas
            pdfTable.DefaultCell.BorderWidth = 1;
            //Añadir fila de cabecera
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                PdfPCell cell = new PdfPCell(new iTextSharp.text.Phrase(column.HeaderText));
                cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);
                pdfTable.AddCell(cell);
            }

            //añadir filas
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    try
                    {
                        pdfTable.AddCell(cell.Value.ToString());
                    }
                    catch { }
                }
            }
            pdfTable.AddCell(DateTime.Now.ToString("dd-MM-yyyy"));
            pdfTable.AddCell(lbNombre.Text);

            //Exportar a pdf (ruta por defect
            string folderPath = "C:\\ticket\\";

            //si no existe el directoria se crea
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string nombreTicket = DateTime.Now.ToString("DD-MM-yy_HH-mm-ss") + ".pdf";
            folderPath += nombreTicket;
            using (FileStream stream = new FileStream(folderPath, FileMode.Create))
            {
                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A6, 10f, 10f, 10f, 0f);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                pdfDoc.Add(pdfTable);
                pdfDoc.Close();
                stream.Close();
            }
            Process pc = new Process();
            pc.StartInfo.FileName = folderPath;
            pc.Start();
            return folderPath;
        }

        private void limpiar()
        {
            listaCesta.Clear();
            dataGridView1.Rows.Clear();
            total = 0;
            lbTotal.Text = "0";
            dataGridView1.Rows.Add("Total: ", 0);
        }

        private void btnVaciar_Click(object sender, EventArgs e)
        {
            limpiar();
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("¿Quieres eliminar producto de la cesta?", "TPV Farmacia", MessageBoxButtons.YesNo);
            int posicion = dataGridView1.CurrentRow.Index;
            if (posicion != dataGridView1.RowCount - 1)
            {
                if (resultado == DialogResult.Yes)
                {
                    double precioParcial = listaCesta[posicion].Precio;
                    total = total - precioParcial;
                    listaCesta.RemoveAt(posicion);
                    dataGridView1.Rows.RemoveAt(posicion);
                    lbTotal.Text = String.Format("{0:0.00}", total);
                    dataGridView1.Rows.RemoveAt(dataGridView1.RowCount - 1);
                    dataGridView1.Rows.Add("Total: ", Math.Round(total, 2));
                }
            }
        }

        private void txbPass_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int nivel = cnx.buscarUsuario(txbUser.Text, txbPass.Text);
                if (nivel == 0)
                {
                   MessageBox.Show("Usuario No existe");
                }
                else
                {
                    if (nivel == 1)
                    {
                        MessageBox.Show("Nivel administrador");
                        btnGestion.Visible = true;
                    }
                    if (nivel == 2)
                    {
                        MessageBox.Show("Usuario dependiente");

                    }
                    lbUser.Text = txbUser.Text;
                    txbUser.Visible = false;
                    txbPass.Visible = false;
                    btnCerrarSesion.Visible = true;
                    btnPagar.Visible = true;
                    btnVaciar.Visible = true;
                }
                
            }
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            txbUser.Text = "";
            txbPass.Text = "";
            txbUser.Visible = true;
            txbPass.Visible = true;
            lbUser.Text = "";
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult salir = MessageBox.Show("¿Quieres salir?", "TPV Farmacia", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(salir == DialogResult.Yes)
            {
                Close();
            }
        }

        private void btnGestion_Click(object sender, EventArgs e)
        {
            Gestion fg = new Gestion();
            fg.Show();
        }

        private void txbBuscador_TextChanged(object sender, EventArgs e)
        {
            tabControl1.Controls.Clear();
            listaEncontrados.Clear();

            foreach(Medicamento med in listaMedicamentos)
            {
                if(med.Nombre.StartsWith(txbBuscador.Text, true, null))
                {
                    listaEncontrados.Add(med);
                }
            }
            try
            {
                cargarTPV(listaEncontrados);
            }catch(Exception ex)
            {

            }
            
            txbBuscador.Focus();
        }

        
    }
}
