using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using RBAI.Domain;
using RBAI.Logging;

namespace RBAI.Repository
{
    public class InventoryRepository
    {
        public static Logger RepoLogger = null;

        static InventoryRepository()
        {
            RepoLogger = new Logger();    
        }

        public static bool AddPartNumber(InventoryItem item)
        {
            bool status = false;
            OleDbConnection repoConnection = null;
            OleDbTransaction tran = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();

                tran = repoConnection.BeginTransaction();
                string commandStatement = "UPDATE  Inventory SET CurrentStock = CurrentStock + " + item.CurrentStock +
                                          " WHERE PartNumber = '" + item.PartNumber+"'";
                string dailyfactCommmanStatemetn =
                    "INSERT INTO InventoryDailyFacts (PartNumber,Quantity,PalletNo,TransactionDate,IsAdd) VALUES('" +
                    item.PartNumber + "'," + item.CurrentStock +",'"+item.PalletNo +"','" + DateTime.Now.ToShortDateString() + "',1)";

                RepoLogger.LogMsg(LogModes.REPO, LogLevel.INFO, " AddPartNumber SQL " + commandStatement);
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.INFO, " AddPartNumber SQL " + dailyfactCommmanStatemetn);
             
                var sqlCommand = new OleDbCommand();
                sqlCommand.Transaction = tran;
                sqlCommand.CommandText = commandStatement;
                sqlCommand.Connection = repoConnection;
                int rows = sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = dailyfactCommmanStatemetn;
                int drow = sqlCommand.ExecuteNonQuery();

                if (rows > 0 && drow > 0)
                    status = true;
                tran.Commit();
            }
            catch (Exception exp)
            {
                tran.Rollback();
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                    "Error while adding AddPartNumber- " + exp.Message + " StackTrace:- " + exp.StackTrace);
                status = false;
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return status;
        }

        public static bool RestorePartNumber(InventoryItem item)
        {
            bool status = false;
            OleDbConnection repoConnection = null;
            OleDbTransaction tran = null;
            try
            {

                repoConnection = DBConnection.OpenConnection();

                tran = repoConnection.BeginTransaction();

                string commandStatement = "UPDATE  Inventory SET CurrentStock = CurrentStock - " + item.CurrentStock + " WHERE PartNumber = '" + item.PartNumber + "'";

                string dailyfactCommmanStatemetn =
                  "INSERT INTO InventoryDailyFacts (PartNumber,Quantity,PalletNo,InvoiceNo,TransactionDate,IsRestore) VALUES('" +
                  item.PartNumber + "'," + item.CurrentStock + ",'" + item.PalletNo + "','" + item.InvoiceNo + "','" + DateTime.Now.ToShortDateString() + "',1)";

                RepoLogger.LogMsg(LogModes.REPO, LogLevel.INFO, " AddPartNumber SQL " + commandStatement);
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.INFO, " AddPartNumber SQL " + dailyfactCommmanStatemetn);

                OleDbCommand sqlCommand = new OleDbCommand();
                sqlCommand.Transaction = tran;
                sqlCommand.CommandText = commandStatement;
                sqlCommand.Connection = repoConnection;
                int rows = sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = dailyfactCommmanStatemetn;
                int drow = sqlCommand.ExecuteNonQuery();

                if (rows > 0 && drow > 0)
                    status = true;
                tran.Commit();
            }
            catch (Exception exp)
            {
                if (tran != null) tran.Rollback();
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                    "Error while adding AddPartNumber- " + exp.Message + " StackTrace:- " + exp.StackTrace);
                status = false;
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return status;
        }
        public static bool ValidateAdminUser(string user, string pass)
        {
            bool valid = false;
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                var cmd =
                    new OleDbCommand(
                        "SELECT count(*) from Logins where Username='" + user + "' AND Password='" + pass + "'",
                        repoConnection);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting ValidateAdminUser - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return valid;

        }

        public static bool ValidateInvoiceTransaction(string palletnumber, InventoryTransactionType transType,string invoiceNumber="$$$$")
        {
            bool valid = false;
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                OleDbCommand cmd = null;
                if (transType == InventoryTransactionType.Add)
                {
                    cmd = new OleDbCommand(
                        string.Format("SELECT count(*) from InventoryDailyFActs where PalletNo='{0}'", palletnumber),
                        repoConnection);
                }
                else
                {
                   cmd= new OleDbCommand(
                      string.Format("SELECT count(*) from InventoryDailyFActs where InvoiceNo='{0}'", invoiceNumber),
                      repoConnection);
                }
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    valid = false;
                }
                else
                {
                    valid = true;
                }
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting ValidateInvoiceTransaction - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return valid;
        }
        public static RestoreTransactionStatus ValidateRestoreTransaction(InventoryItem item,InventoryTransactionType transType)
        {
            bool valid = false;
            string message = "";
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                OleDbCommand cmd = null;
              
                    cmd = new OleDbCommand(
                       string.Format("SELECT count(*) from InventoryDailyFActs where PalletNo='{0}'", item.PalletNo),
                       repoConnection);
                
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                cmd = new OleDbCommand(
                     string.Format("SELECT count(*) from InventoryDailyFActs where InvoiceNo='{0}'", item.InvoiceNo),
                     repoConnection);
                int count1 = Convert.ToInt32(cmd.ExecuteScalar());

                cmd = new OleDbCommand(
                    string.Format("SELECT count(*) from Inventory where CurrentStock > {0} and PartNumber='{1}'", item.CurrentStock,item.PartNumber),
                    repoConnection);

                int resultCount = Convert.ToInt32(cmd.ExecuteScalar());
                
                if (count > 0 && count1 == 0 && resultCount!=0)
                {
                    valid = false;
                    message = "Pallet number /Invoice Number not exsit ! Please try again";

                }
                else if (count > 0 && count1 > 0)
                {
                    valid = false;
                    message = "Invoice Number already exist";
                }
                else if (count==0 && count1==0)
                {
                    valid = false;
                    message = "Pallet number not exsit ! Please try again";
                }
                else if (resultCount ==0)
                {
                    valid = false;
                    message = "No sufficinet quantity availlable for the partNumber "+ item.PartNumber;
                }
                else if (count > 0 && count1 == 0)
                {
                    valid = true;
                    message = "";
                }
                else
                {
                    message = "Pallet number /Invoice Number not exsit ! Please try again";
                    valid = false;
                }
                
                return new RestoreTransactionStatus() { Message = message, Valid = valid };
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting ValidateInvoiceTransaction - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return new RestoreTransactionStatus() { Message = "Something went wrong ! Please check the administrator", Valid = false };
            
        }
        public static bool ValidateAdminOtp(string otpPass)
        {
            bool valid = false;
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                var cmd =
                    new OleDbCommand(
                        "SELECT count(*) from Logins where OneTimePassword='" + otpPass + "'",
                        repoConnection);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting GetStoreLocations - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return valid;

        }
        public static int GetCurrentStockByPartNumber(string partNumber)
        {
            int stock = 0;
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                var cmd =
                    new OleDbCommand(
                        "SELECT CurrentStock from Inventory where PartNumber='" + partNumber + "'",
                        repoConnection);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                stock = count;
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting GetStoreLocations - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return stock;

        }
        public static bool CheckPartNumberExist(InventoryItem item)
        {
            bool status = false;
            OleDbConnection repoConnection = null;
            int NoOfParts = 0;
            try
            {
                repoConnection = DBConnection.OpenConnection();

                string commandtext = "SELECT count(*) From [Inventory] WHERE  [PartNumber] = '" + item.PartNumber + "'" ;
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.INFO, "SQL :- " + commandtext);
                OleDbCommand sqlCommand = new OleDbCommand();
                sqlCommand.CommandText = commandtext;
                sqlCommand.Connection = repoConnection;
                NoOfParts = Convert.ToInt32(sqlCommand.ExecuteScalar());

                if (NoOfParts >= 1)
                    status = true;


            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting CheckPartNumberExist - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                status = false;
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return status;

        }

        public static bool InsertPartNumberAdmin(InventoryItem item)
        {
            bool isExist = false;
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                var cmdExcel = new OleDbCommand();
                cmdExcel.Connection = repoConnection;
                cmdExcel.CommandText = string.Format("INSERT INTO Inventory (PartNumber,Customer,Description,[Min],[Max]) VALUES('{0}','{1}','{2}',{3},{4})",item.PartNumber,item.Customer,item.Description,item.Min,item.Max);
                int rows = Convert.ToInt32(cmdExcel.ExecuteNonQuery());

                if (rows > 0)
                    isExist = true;
                else
                    isExist = false;
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                 "Error while Getting InsertPartNumberAdmin - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                isExist = false;
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return isExist;
        }
        public static List<string> GetPartNumbers()
        {
            List<string> listColumns = new List<string>();
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                OleDbCommand cmdExcel = new OleDbCommand();
                cmdExcel.Connection = repoConnection;
                cmdExcel.CommandText = "SELECT PartNumber From [Inventory] ";
                OleDbDataReader reader = cmdExcel.ExecuteReader();
                while (reader.Read())
                {
                    listColumns.Add(reader[0].ToString());
                }
                reader.Close();
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting GetStoreLocations - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return listColumns;

        }

        public static Queue<InventoryItem> GetAllNonZeroItems()
        {
            Queue<InventoryItem> listInventoryItems = new Queue<InventoryItem>();
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                
                string dynamicSQL = "SELECT  PartNumber,  Description, Customer,Sum(Min) as Min , SUM(Max) as Max,SUM(CurrentStock) as CurrentStock FROM Inventory WHERE CurrentStock > 0 GROUP BY PartNumber,  Description, Customer ORDER BY PartNumber ASC";


                OleDbCommand sqlCommand = new OleDbCommand();
                sqlCommand.CommandText = dynamicSQL;
                sqlCommand.Connection = repoConnection;
                OleDbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    var cls = new InventoryItem();
                    cls.PartNumber = reader["PartNumber"].ToString();
                    cls.Description = reader["Description"].ToString();
                    cls.Customer = reader["Customer"].ToString();
                    cls.Min = Convert.ToInt32(reader["Min"].ToString());
                    cls.Max = Convert.ToInt32(reader["Max"].ToString());
                    cls.CurrentStock = Convert.ToInt32(reader["CurrentStock"].ToString());
                    listInventoryItems.Enqueue(cls);
                }
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting GetAllNonZeroItems  - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return listInventoryItems;
        }

        public static List<InventoryDaiyFact> GetInventoryReport(string fromDate, string toDate)
        {
            var listInventoryItems = new List<InventoryDaiyFact>();
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();

                string dynamicSQL =
                    "SELECT TransactionDate,PartNumber,IsAdd, SUM(Quantity) AS Qty FROM InventoryDailyFacts WHERE TransactionDate   Between  Format(#" +
                    fromDate + "#, 'dd/mm/yyyy') And   Format(#" + toDate +
                    "#, 'dd/mm/yyyy')  GROUP BY TransactionDate, PartNumber,IsAdd ORDER BY TransactionDate desc";


                var sqlCommand = new OleDbCommand();
                sqlCommand.CommandText = dynamicSQL;
                sqlCommand.Connection = repoConnection;
                OleDbDataReader reader = sqlCommand.ExecuteReader();
                while (reader != null && reader.Read())
                {
                    DateTime date = Convert.ToDateTime(reader[0].ToString());
                    string Parttime = reader[1].ToString();
                    string transType = Convert.ToBoolean(reader[3].ToString()) == true ? "Add" : "Restore";
                    int quantity = Convert.ToInt32(reader[4].ToString());
                    var cls = new InventoryDaiyFact();
                    cls.PartNumber = Parttime;
                    cls.TransactionDate = date;
                    cls.IsAddOrTrans = transType;
                    cls.Quantity = quantity;
                    listInventoryItems.Add(cls);
                }
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                  "Error while Getting GetInventoryReport  - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                throw;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return listInventoryItems;
        }
        public static bool ResetInventoryItemsToZero()
        {
            OleDbConnection repoConnection = null;
            try
            {
                repoConnection = DBConnection.OpenConnection();
                
                

                string resetBasrQuery = "UPDATE INVENTORY SET CurrentStock = 0";

                OleDbCommand sqlCommand = new OleDbCommand();
                sqlCommand.CommandText = resetBasrQuery;
                sqlCommand.Connection = repoConnection;
                sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception exp)
            {
                RepoLogger.LogMsg(LogModes.REPO, LogLevel.ERROR,
                    "Error while Getting ResetInventoryItemsToZero - " + exp.Message + " StackTrace:- " + exp.StackTrace);
                return false;
            }
            finally
            {
                DBConnection.CloseConnection(repoConnection);
            }
            return false;
        }
    }
}
