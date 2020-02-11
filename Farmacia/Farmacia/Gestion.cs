using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Farmacia
{
    public partial class Gestion : Form
    {
        ConectarBD cnx;
        List<Medicamento> listaMedicamentos = new List<Medicamento>();
        String nombreImagen;
        Medicamento med = new Medicamento();

        public Gestion()
        {
            InitializeComponent();
        }

        private void Gestion_Load(object sender, EventArgs e)
        {
            cnx = new ConectarBD();
            listaMedicamentos = cnx.listar();
            dataGridView1.DataSource = listaMedicamentos;
        }

        private void btnInsertar_Click(object sender, EventArgs e)
        {
            try
            {
                FileStream fs = new FileStream(nombreImagen, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] bloque = br.ReadBytes((int)fs.Length);
                cnx.Insertar(txtNombre.Text, Convert.ToDouble(txtPrecio.Text), bloque, Convert.ToInt16(txtStockMin.Text), Convert.ToInt16(txtStockActual.Text));
            }catch(Exception ex)
            {
                MessageBox.Show("Error al insertar medicamento");
            }
            
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op1 = new OpenFileDialog();
            op1.Filter = "imagenes|*.jpg;*.png";
            if (op1.ShowDialog()==DialogResult.OK)
            {
                nombreImagen = op1.FileName;
                pictureBox1.Image = Image.FromFile(nombreImagen);

            }
        }

        

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            txtNombreMod.Text = listaMedicamentos[dataGridView1.CurrentRow.Index].Nombre;
            txtPrecioMod.Text = Convert.ToString(listaMedicamentos[dataGridView1.CurrentRow.Index].Precio);
            txtStockMinMod.Text = Convert.ToString(listaMedicamentos[dataGridView1.CurrentRow.Index].Stockminimo);
            txtStockActualMod.Text = Convert.ToString(listaMedicamentos[dataGridView1.CurrentRow.Index].Stockactual);
            MemoryStream ms = new MemoryStream(listaMedicamentos[dataGridView1.CurrentRow.Index].Imagen);
            pictureBoxMod.Image = Image.FromStream(ms);
            med.Imagen = listaMedicamentos[dataGridView1.CurrentRow.Index].Imagen;
            lbIndice.Text = Convert.ToString(listaMedicamentos[dataGridView1.CurrentRow.Index].Indice);
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            med.Indice = Convert.ToInt16(lbIndice.Text);
            med.Nombre = txtNombreMod.Text;
            med.Precio = Convert.ToDouble(txtPrecioMod.Text);
            med.Stockminimo = Convert.ToInt16(txtStockMinMod.Text);
            med.Stockactual = Convert.ToInt16(txtStockActualMod.Text);
            cnx.modificarMedicamento(med);
            listaMedicamentos.Clear();
            listaMedicamentos = cnx.listar();
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = listaMedicamentos;
        }

        private void pictureBoxMod_Click(object sender, EventArgs e)
        {
            OpenFileDialog op1 = new OpenFileDialog();
            String imagen;
            try
            {
                if (op1.ShowDialog() == DialogResult.OK)
                {
                    imagen = op1.FileName;
                    pictureBoxMod.Image = Image.FromFile(imagen);

                    FileStream fs = new FileStream(imagen, FileMode.Open, FileAccess.Read);
                    long tamanio = fs.Length;
                    BinaryReader br = new BinaryReader(fs);
                    byte[] bloque = br.ReadBytes((int)fs.Length);
                    fs.Read(bloque, 0, Convert.ToInt32(tamanio));

                    med.Imagen = bloque;
                }
            }catch(Exception ex)
            {

            }
            
        }
    }
}
