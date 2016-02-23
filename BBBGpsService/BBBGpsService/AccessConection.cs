using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace RPCandle
{
    class AccessConeccion
    {
        public SqlConnection coneccion;
        public string ConeccionString;
        private float iva;
        public AccessConeccion()
        {
            //*
            ConeccionString = "Data Source = weon;initial catalog=weon; user id= RPCandle; password = Rosario13";
            /*/
            ConeccionString = "Data Source = localhost;initial catalog=weon; user id= RPCandle; password = Rosario13";
            //*/
            
            coneccion = new SqlConnection(ConeccionString);
        }
        private bool AbrirConeccion()
        {
            try
            {
                coneccion.Open();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToUpper());
                return false;
            }
            return true;
        }
        public bool CerrarConeccion()
        {
            try
            {
                coneccion.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToUpper());
                return false;
            }
            return true;
        }
        public DataTable ObtieneTabla(string Comando)
        {
            DataTable value;
            value = new DataTable();
            SqlDataAdapter da;

            try
            {
                da = new SqlDataAdapter(Comando, coneccion);
                da.Fill(value);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToUpper());
            }

            return value;
        }
        public DataRow GetFirstRow(string Comando)
        {
            DataTable dt = ObtieneTabla(Comando);
            if (dt == null)
                return null;
            DataRow r;
            try
            {
                r = dt.Rows[0];
            }
            catch (Exception)
            {
                //MessageBox.Show("No hay datos");
                return null;
            }
            return r;
        }
        public Object ExecutaEscalar(string Comando)
        {
            Object value;
            SqlCommand cmd;
            value = null;
            try
            {
                if (!AbrirConeccion())
                    return value;
                cmd = new SqlCommand(Comando, coneccion);
                value = cmd.ExecuteScalar();
                if (value == null)
                    value = 0;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToUpper());
                value = null;
            }
            finally
            {
                CerrarConeccion();
            }

            return value;
        }

        public string ToFecha(DateTime _Fecha)
        {
            string fecha = "NULL";
            DateTime f = _Fecha;
            string m = '0' + f.Month.ToString(); ;
            string d = '0' + f.Day.ToString();
            string h = '0' + f.Hour.ToString();
            string mm = '0' + f.Minute.ToString();

            m = m.Substring(m.Length - 2);
            d = d.Substring(d.Length - 2);
            h = h.Substring(h.Length - 2);
            mm = mm.Substring(mm.Length - 2);

            

            if (_Fecha != null && _Fecha != DateTime.MinValue)
            {
                fecha = "CONVERT(DATETIME,'" + f.Year+ "-" + f.Month + "-" + f.Day  + " " + f.Hour + ":" + f.Minute + "',102)";
            }
            return fecha;
        }

        public float  Iva
        {
            get
            {
                string ObtieneIva = "SELECT Iva FROM [ct].[Iva]";
                string aux = "";
                aux = ExecutaEscalar(ObtieneIva).ToString();
                iva = 0;
                aux = aux.Replace(",", ".");
                float.TryParse(aux, out iva);
                return iva;
            }
            set
            {
                string NuevoIva = "INSERT INTO [ct].[Iva] (Iva, Fecha) VALUES (" + value + ", " + ToFecha(DateTime.Today) + ")";
                ExecutaEscalar(NuevoIva);
            }
        }

        public SqlDataReader GetDataReader(string Comando)// TODA VEZ QUE SE OBTIENE UN DATA READER SE MANTIENE LA CONECCION ABIERTA DEBE CERRARSE
        {
            SqlDataReader dr;
            SqlCommand cmd;
            dr = null;
            try
            {
                AbrirConeccion();
                cmd = new SqlCommand(Comando, coneccion);
                dr = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToUpper());
            }
            return dr;
        }
        public void ActualizaTabla(string cmdSelect, DataTable Tabla)
        {
            SqlDataAdapter da = new SqlDataAdapter(cmdSelect, coneccion);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            da.Update(Tabla);
        }

    }
}

