using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBAI.Logging;

namespace RBAI.Domain
{
    public static class DBConnection
    {
        public static Logger logger = null;

        static DBConnection()
        {
            logger = new Logger();
        }
        public static OleDbConnection OpenConnection()
        {
            OleDbConnection conn = null;
            try
            {
                logger.LogMsg(LogModes.REPO, LogLevel.INFO, "Opening DB Connection");
                string connectionString = ConfigurationManager.ConnectionStrings["RBAIAccessDBConnectionString"].ToString();
                conn = new OleDbConnection(connectionString);
                conn.Open();

            }
            catch (Exception exp)
            {
                logger.LogMsg(LogModes.REPO, LogLevel.ERROR, "Error in Db Connection Message : - " + exp.Message + " StackTrace:- " + exp.StackTrace);

            }
            return conn;
        }

        public static void CloseConnection(OleDbConnection conn)
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }

}
