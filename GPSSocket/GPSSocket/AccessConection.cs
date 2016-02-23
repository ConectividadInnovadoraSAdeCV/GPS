using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Net.NetworkInformation;

namespace PJAEstatus
{
    public enum Comandos { Select, Insert, Update, Delete, SelectDt, DeleteDt, SelectCombo, SelectComboCol };
    class AccessConeccion
    {
        public  SqlConnection coneccion;
        public string ConeccionString;
        private float iva;
        public AccessConeccion()
        {
            //*
            ConeccionString = "Data Source = localhost;initial catalog=weon; user id= RPCandle; password = Rosario13";
            /*/
            ConeccionString = "Data Source = weon;initial catalog=weon; user id= RPCandle; password = Rosario13";
            //*/
            coneccion = new SqlConnection(ConeccionString);
        }
        public string Server()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            try
            {
                PingReply reply = pingSender.Send("RP01", timeout, buffer, options);
            }
            catch(Exception)
            {
                return "weon.dynalias.com";
            }
            return "RP01";
        }
        private bool AbrirConeccion()
        {
            try
            {
                coneccion.Open();
            }
            catch (Exception )
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
            catch (Exception )
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
               // MessageBox.Show(ex.Message.ToUpper());
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
                string ObtieneIva = "SELECT Iva FROM [ve].[Iva]";
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
            catch (Exception )
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

        //public int GetBool(CheckBox cb)
        //{
        //    int aux;
        //    aux = cb.Checked ? 1 : 0;
        //    return aux;
        //}

        public Object BulkInsertWithTrigger(DataTable _DataTable, string TablaDestino)
        {


            SqlBulkCopy bc = new SqlBulkCopy(coneccion);
            string cmd = "SELECT * FROM " + TablaDestino + " WITH (NOLOCK) ";
            DataTable dtColumns = ObtieneTabla(cmd);
            DataTable dt = _DataTable.Copy();
            int CantCol = dt.Columns.Count;
            int indice = 0;

            for (int i = 0; i < CantCol; i++)
            {
                if (dtColumns.Columns.Contains(dt.Columns[indice].ColumnName))
                {
                    bc.ColumnMappings.Add(dt.Columns[indice].ColumnName, dt.Columns[indice].ColumnName);
                    indice++;
                }
                else
                {
                    dt.Columns.Remove(dt.Columns[indice].ColumnName);
                }
            }
            #region crea comando insert
            cmd = "INSERT INTO " + TablaDestino + " (";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                cmd += "[" + dt.Columns[i].ColumnName + "]";
                if (i != dt.Columns.Count - 1)
                {
                    cmd += ",";
                }
            }
            cmd += ") Values  ";
            //creo la parte del comando que necesita parametros
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cmd += "(";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    cmd += "@" + dt.Columns[j].ColumnName.Replace(" ", "").Replace("/", "") + i;
                    if (j != dt.Columns.Count - 1)
                    {
                        cmd += ",";
                    }
                }
                cmd += ")";
                if (i != dt.Rows.Count - 1)
                {
                    cmd += ",";
                }
            }
            // ya tengo el string comando falta generar el comand
            SqlCommand sqlComm = new SqlCommand(cmd, coneccion);
            //lleno las varables de los parametros
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    sqlComm.Parameters.AddWithValue("@" + dt.Columns[j].ColumnName.Replace(" ", "").Replace("/", "") + i, dt.Rows[i][dt.Columns[j]]);
                    cmd += "@" + dt.Columns[j].ColumnName.Replace(" ", "") + i;
                    if (j != dt.Columns.Count - 1)
                    {
                        cmd += ",";
                    }
                }
            }
            Object value = null;
            try
            {
                if (!AbrirConeccion())
                    return value;
                value = sqlComm.ExecuteNonQuery();
                if (value == null)
                    value = 0;
            }
            catch (Exception )
            {
                //MessageBox.Show(ex.Message.ToUpper());
                value = null;
            }
            finally
            {
                CerrarConeccion();
            }

            return value;
            #endregion

        }
        public bool BulkInsert(DataTable _DataTable, string TablaDestino)
        {
            SqlBulkCopy bc = new SqlBulkCopy(coneccion);
            string cmd = "SELECT * FROM " + TablaDestino + " WITH (NOLOCK) ";
            DataTable dtColumns = ObtieneTabla(cmd);
            DataTable dt = _DataTable.Copy();
            int CantCol = dt.Columns.Count;
            int indice = 0;

            for (int i = 0; i < CantCol; i++)
            {
                if (dtColumns.Columns.Contains(dt.Columns[indice].ColumnName))
                {
                    bc.ColumnMappings.Add(dt.Columns[indice].ColumnName, dt.Columns[indice].ColumnName);
                    indice++;
                }
                else
                {
                    dt.Columns.Remove(dt.Columns[indice].ColumnName);
                }
            }

            bc.DestinationTableName = TablaDestino;
            try
            {
                coneccion.Open();
                bc.WriteToServer(dt);
                return true;
            }
            catch (Exception )
            {
                //MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                coneccion.Close();
            }
        }
    }
}

