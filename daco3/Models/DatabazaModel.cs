using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    
    public class DatabazaModel
    {
        
        OracleConnection conection;
        [Required]
        public string Meno { get; set ; }
        [Required]
        public string Heslo { get ; set ; }
        public int ID { get ; set; }
        public List<Zaznam> Data { get ; set; }

        public DatabazaModel()
        {
            string conStr = "Data Source=(DESCRIPTION =(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST = obelix.fri.uniza.sk)(PORT = 1521)))"
                + "(CONNECT_DATA =(SERVICE_NAME = orcl2.fri.uniza.sk)));User ID=uhrin9;Password=dodo2331996;";
            this.conection = new OracleConnection(conStr);
            Data = new List<Zaznam>();
            try
            {
                this.conection = new OracleConnection(conStr);
                this.conection.Open();
            }
            catch (Exception e)
            {
            }

        }
        public bool Prihlas() {/*
            OracleCommand orclCom = new OracleCommand();
            orclCom.Connection = conection;
            OracleDataReader orclReader = null;
            orclCom.CommandText = "insert into pouzivatel (meno,heslo) values('" + zaznam.Meno + "',' ')";
            orclCom.ExecuteNonQuery();

            orclCom.CommandText = "select id_pouzivatela from pouzivatel where meno = '" + zaznam.Meno + "'";
            orclReader = orclCom.ExecuteReader();
        
             */
            OracleCommand orclCom = new OracleCommand();
            orclCom.Connection = conection;
            orclCom.CommandText = "select id_pouzivatela from pouzivatel where meno='" + Meno + "' and heslo = '" + Heslo + "'";
            OracleDataReader orclReader = orclCom.ExecuteReader();
            if (orclReader.Read()) {
                this.ID = Int32.Parse(orclReader["id_pouzivatela"].ToString());
                orclReader.Close();
                return true;
            }
            orclReader.Close();
            return false;
        }
     
        public void odpoj()
        {
            conection.Clone();
        }
        public void selectData() {
            OracleCommand orclCom = new OracleCommand();
            orclCom.Connection = conection;
            orclCom.CommandText = "select meno,cas,typ"
                                +" from zaznam z join pouzivatel p on(p.ID_POUZIVATELA= z.POUZIVATEL) "
                                +"order by cas desc "
                                +"fetch first 40 rows only ";
            OracleDataReader orclReader = orclCom.ExecuteReader();
            while (orclReader.Read()) {

                string meno = orclReader["meno"].ToString();
                string cas = orclReader["cas"].ToString();
                string typ = orclReader["typ"].ToString().Replace(" ", "");
                Data.Add(new Zaznam(meno, cas.Substring(0, 10), cas.Substring(11, 5), typ));
            }          
            orclReader.Close();

        }
    }

}