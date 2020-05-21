using System;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace mock_sns_core2.Services
{
    public class DbConnectionService
    {
        private string constr = Startup.ConnectionString;
        private OracleConnection con { get; set; }
        private OracleTransaction tran { get; set; }

        public DbConnectionService()
        {
            this.con = new OracleConnection(constr);
        }

        public void BeginTran()
        {
            if(this.tran == null)
            {
                this.tran = con.BeginTransaction();
            }
        }

        public void CommitTran()
        {
            if(tran == null)
            {
                throw new InvalidOperationException("Not Found Tranzaction");
            }
            tran.Commit();
            tran = null;
        }

        public void RollbackTran()
        {
            if(tran == null)
            {
                throw new InvalidOperationException("Not Found Tranzaction");
            }
            tran.Rollback();
            tran = null;
        }

        public void Open()
        {
            con.Open();
        }

        public void Close()
        {
            con.Close();
            con = null;
        }

        public OracleConnection GetConnection()
        {
            return this.con;
        }

        public OracleTransaction GetTransaction()
        {
            return this.tran;
        }
    }
}
