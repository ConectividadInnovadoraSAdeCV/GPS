using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using RPCandle;

namespace BBBGpsService
{
    /// <summary>
    /// Summary description for BBBGps
    /// </summary>
    [WebService(Namespace = "http://weon.mx/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class BBBGps : System.Web.Services.WebService
    {

        [WebMethod(Description = "Recibe las coordenadas actuales")]
        //*/
        public bool SendCoordenates(string Camion, double Latitud, double Longitud, string Pasajeros, string FechaHora, string Pwd)
        {
            string lat, lon;
            lat = Latitud.ToString();
            lon = Longitud.ToString();
            lat = lat.Replace(",", ".");
            lon = lon.Replace(",", ".");
            if (Pwd != "weonrules")
            {
                throw new Exception("No eres un usuario autorizado");
            }
            AccessConeccion ac = new AccessConeccion();
            string cmd = "INSERT INTO gps.Coordenadas " +
                                "(Camion,Latitud,Longitud,Pasajeros,Fecha) " +
                         "VALUES " +
                                "('" + Camion + "'," + lat + "," + lon + ",'" + Pasajeros + "',CAST('" + FechaHora + "' AS DATETIME))";
            return ac.ExecutaEscalar(cmd) != null;

        }
        [WebMethod(Description = "Recibe mesages del sitema")]
        public bool SendMesages(string Camion, string FechaHora,int Mesg,string Pwd)
        {
            if (Pwd != "weonrules")
            {
                throw new Exception("No eres un usuario autorizado");
            }
            AccessConeccion ac = new AccessConeccion();
            string cmd = "INSERT INTO [dbo].[MensagesInternos] "+
                                    "([Fecha],[Camion],[Mensage]) " +
                         "VALUES " +
                                "(CAST('" + FechaHora + "' AS DATETIME),'" + Camion + "'," + Mesg + ")";
            return ac.ExecutaEscalar(cmd) != null;

        }
        /*/
        public string SendCoordenates(string Camion, double Latitud, double Longitud, string Pasajeros, string FechaHora, string Pwd)
        {
            string lat, lon;
            lat = Latitud.ToString();
            lon = Longitud.ToString();
            lat = lat.Replace(",", ".");
            lon = lon.Replace(",", ".");
            if (Pwd != "weonrules")
            {
                throw new Exception("No eres un usuario autorizado");
            }
            AccessConeccion ac = new AccessConeccion();
            string cmd = "INSERT INTO gps.Coordenadas " +
                                "(Camion,Latitud,Longitud,Pasajeros,Fecha) " +
                         "VALUES " +
                                "('" + Camion + "'," + lat + "," + lon + ",'" + Pasajeros + "',CAST('" + FechaHora + "' AS DATETIME))";
            //return ac.ExecutaEscalar(cmd) != null;
            return cmd;                    
        }
        //*/
    }
}
