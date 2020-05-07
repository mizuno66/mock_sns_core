using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Dapper;

using mock_sns_core2.Services;
using System.Threading.Tasks;

namespace mock_sns_core2.Models
{
    public class Article
    {
        public ulong Id { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime PostDate { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Article()
        {
        }

        public Article get(string sql, object param)
        {
            string constr = Startup.ConnectionString;
            using (var con = new OracleConnection(constr))
            {
                try
                {
                    con.Open();

                    var result = con.Query<Article>(sql, param);

                    Article ret = null;
                    foreach (var d in result)
                    {
                        ret = d;
                        break;
                    }

                    con.Close();

                    return ret;
                }
                catch (OracleException ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                finally
                {
                    con?.Close();
                }

            }
        }

        public async Task<IEnumerable<Article>> search(string sql, object param)
        {
            string constr = Startup.ConnectionString;
            using (var con = new OracleConnection(constr))
            {
                try
                {
                    con.Open();

                    var result = await con.QueryAsync<Article, ApplicationUser, Article>(sql,
                        (article, user) =>
                        {
                            article.User = user;
                            return article;
                        },
                        param);

                    con.Close();

                    return result;

                }catch(OracleException ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                finally
                {
                    con?.Close();
                }

            }
        }

        public async Task<int> insert()
        {
            string sql = "INSERT INTO Article VALUES(:Id, :UserId, :PostDate, :Text, :CreatedAt, :UpdatedAt)";
            string constr = Startup.ConnectionString;
            using (var con = new OracleConnection(constr))
            {
                con.Open();

                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        SequenceService seq = new SequenceService();
                        var Id = seq.getNextVal(SequenceService.seqType.Article);
                        var now = DateTime.Now;
                        var result = await con.ExecuteAsync(sql, new
                        {
                            Id = Id
                            ,
                            UserId = this.User.Id
                            ,
                            PostDate = this.PostDate
                            ,
                            Text = this.Text
                            ,
                            CreatedAt = now
                            ,
                            UpdatedAt = now
                        }, tran);

                        tran.Commit();

                        this.Id = Decimal.ToUInt64(Id);

                        return result;
                    }
                    catch (OracleException ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex);
                        throw;
                    }
                    finally
                    {
                        con?.Close();
                    }
                }
            }
        }

        public async Task<int> update()
        {
            if (this.Id == 0)
            {
                return -1;
            }
            string sql = "UPDATE Article SET Text = :Text, UpdatedAt = :UpdatedAt where Id = :Id)";
            string constr = Startup.ConnectionString;
            using (var con = new OracleConnection(constr))
            {
                con.Open();

                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        var result = await con.ExecuteAsync(sql, new
                        {
                            Id = this.Id
                            ,
                            Text = this.Text
                            ,
                            UpdatedAt = DateTime.Now
                        }, tran);

                        tran.Commit();

                        return result;
                    }
                    catch (OracleException ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex);
                        throw;
                    }
                    finally
                    {
                        con?.Close();
                    }
                }
            }
        }
    }
}
