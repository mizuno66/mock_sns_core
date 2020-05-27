using System;
using System.Threading.Tasks;

using Oracle.ManagedDataAccess.Client;
using Dapper;

using mock_sns_core2.Services;
using System.Collections.Generic;

namespace mock_sns_core2.Models
{
    public class ArticleContents
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long ArticleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ArticleContents()
        {
        }

        public string getContentType()
        {
            return this.ContentType.Substring(0, this.ContentType.IndexOf("/"));
        }

        public string getExtension()
        {
            return "." + this.ContentType.Substring(this.ContentType.IndexOf("/") + 1);
        }

        public async Task<IEnumerable<ArticleContents>> GetListAsync(long articleId)
        {
            string sql = "select * from ArticleContents " +
                " where ArticleId = :ArticleId " +
                " order by ArticleId  ";

            using(var con = new OracleConnection(Startup.ConnectionString))
            {
                con.Open();
                var result = await con.QueryAsync<ArticleContents>(sql,
                    new
                    {
                        ArticleId = articleId
                    });

                con.Close();

                return result;
            }
        }

        public async Task<int> insert(DbConnectionService dbcs)
        {
            string sql = "INSERT INTO ArticleContents VALUES(:Id, :FileName, :ContentType, :ArticleId, :CreatedAt, :UpdatedAt)";

            SequenceService seq = new SequenceService();
            var Id = seq.getNextVal(SequenceService.seqType.ArticleContents);
            var now = DateTime.Now;

            var result = await dbcs.GetConnection().ExecuteAsync(sql, new
            {
                Id = Id
                ,
                FileName = this.FileName
                ,
                ContentType = this.ContentType
                ,
                ArticleId = this.ArticleId
                ,
                CreatedAt = now
                ,
                UpdatedAt = now
            }, dbcs.GetTransaction()); ;
                
            return result;
        }
    }
}
