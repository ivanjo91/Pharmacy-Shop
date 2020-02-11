using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Farmacia
{
    class ConectarBD
    {
        MySqlConnection conexion;
        MySqlCommand comando;
        MySqlDataReader datos;
        List<Medicamento> listaMedicamento = new List<Medicamento>();

        public ConectarBD()
        {
            conexion = new MySqlConnection();
            conexion.ConnectionString = "Server=remotemysql.com;Database=Pr1mdxAdrh;Uid=Pr1mdxAdrh;Pwd=fNBUrxid1O";
        }
        public List<Medicamento> listar()
        {
            conexion.Open();
            String cadenaSql = "select * from medicamento";
            comando = new MySqlCommand(cadenaSql, conexion);
            datos = comando.ExecuteReader();

            while (datos.Read())
            {
                Medicamento med = new Medicamento();
                med.Indice = Convert.ToInt16(datos["indice"]);
                med.Nombre = Convert.ToString(datos["nombre"]);
                med.Precio = Convert.ToDouble(datos["precio"]);
                med.Imagen = (byte[])datos["imagen"];
                med.Stockactual = Convert.ToInt16(datos["stockactual"]);
                med.Stockminimo = Convert.ToInt16(datos["stockminimo"]);
                listaMedicamento.Add(med);
            }
            conexion.Close();
            return listaMedicamento;
        }

        public void Insertar(String nombreMed, double precio, byte[] imagen, int stockMin, int stockActual)
        {
            conexion.Open();
            String cadenaSql = "insert into medicamento values(null, ?nom, ?precio, ?imagen, ?stockmin, ?stockactual)";
            comando = new MySqlCommand(cadenaSql, conexion);
            comando.Parameters.Add("?nom", MySqlDbType.VarChar).Value = nombreMed;
            comando.Parameters.Add("?precio", MySqlDbType.Double).Value = precio;
            comando.Parameters.Add("?imagen", MySqlDbType.Blob).Value = imagen;
            comando.Parameters.Add("?stockmin", MySqlDbType.Int16).Value = stockMin;
            comando.Parameters.Add("?stockactual", MySqlDbType.Int16).Value = stockActual;
            comando.ExecuteNonQuery();
            conexion.Close();
        }

        public int buscarUsuario(String user, String pass)
        {
            String sql = "select nivel from usuario where dni=?dni and clave=?cla";
            conexion.Open();
            comando = new MySqlCommand(sql, conexion);
            comando.Parameters.Add("?dni", MySqlDbType.String).Value = user;
            comando.Parameters.Add("?cla", MySqlDbType.String).Value = pass;
            MySqlDataReader datos = comando.ExecuteReader();
            int nivel=0;
            if (datos.Read())
            {
                nivel = Convert.ToInt16(datos["nivel"]);
            }
            conexion.Close();
            return nivel;
        }

        

        public void insertarFactura(List<Medicamento> listaCesta, string dni, double total)
        {
            string cadenaProductos = "";
            for (int i = 0; i<listaCesta.Count; i++)
            {
                cadenaProductos += listaCesta[i].Nombre + ", ";
            }
            conexion.Open();
            String cadenaSql = "insert into facturacion values(null, ?dni, ?cadenaProd, ?fecha, ?total)";
            comando = new MySqlCommand(cadenaSql, conexion);
            comando.Parameters.Add("dni", MySqlDbType.VarChar).Value = dni;
            comando.Parameters.Add("cadenaProd", MySqlDbType.VarChar).Value = cadenaProductos;
            comando.Parameters.Add("fecha", MySqlDbType.DateTime).Value = DateTime.Now;
            comando.Parameters.Add("total", MySqlDbType.Double).Value = total;
            comando.ExecuteNonQuery();
            conexion.Close();
        }

        internal void modificarMedicamento(Medicamento med)
        {
            conexion.Open();
            String cadenaSql = "update medicamento set nombre=?nom, precio=?pr, stockminimo=?sm," 
                +"stockactual=?sa, imagen=?im where indice=?id";
            comando = new MySqlCommand(cadenaSql, conexion);
            comando.Parameters.Add("?id", MySqlDbType.Int16).Value = med.Indice;
            comando.Parameters.Add("?nom", MySqlDbType.VarChar).Value = med.Nombre;
            comando.Parameters.Add("?pr", MySqlDbType.Double).Value = med.Precio;
            comando.Parameters.Add("?sm", MySqlDbType.Int16).Value = med.Stockminimo;
            comando.Parameters.Add("?sa", MySqlDbType.Int16).Value = med.Stockactual;
            comando.Parameters.Add("?im", MySqlDbType.Blob).Value = med.Imagen;
            comando.ExecuteNonQuery();
            conexion.Close();
        }

        public void lanzarActualizacion(List<Medicamento> listaCesta)
        {
            conexion.Open();
            for(int i=0; i<listaCesta.Count; i++)
            {
                string nombreMed = listaCesta[i].Nombre;
                String cadenaSql = "update medicamento set stockactual=stockactual-1 where nombre='"+nombreMed+"'";
                comando = new MySqlCommand(cadenaSql, conexion);
                comando.ExecuteNonQuery();
            }
            
            conexion.Close();
        }
    }
}
