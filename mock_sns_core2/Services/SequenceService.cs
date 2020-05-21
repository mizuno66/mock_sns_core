using System;
using Oracle.ManagedDataAccess.Client;

namespace mock_sns_core2.Services
{
    public class SequenceService
    {
        public enum seqType
        {
            Article,
            ArticleContents,
        }
        public SequenceService()
        {
        }

        public decimal getNextVal(seqType stype)
        {
            string sql = "select SEQ_{ seq_name }.nextval from dual";
            switch (stype)
            {
                case seqType.Article:
                    sql = sql.Replace("{ seq_name }", "Article");
                    break;
                case seqType.ArticleContents:
                    sql = sql.Replace("{ seq_name }", "ArticleContents");
                    break;
            }

            string constr = Startup.ConnectionString;
            try
            {
                using (var con = new OracleConnection(constr))
                {
                    con.Open();
                    using (var command = new OracleCommand(sql, con))
                    {
                        var ret = (decimal)0;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ret = (decimal)reader[0];
                            }

                            con.Close();
                            return ret;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
