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
        public long Id { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime PostDate { get; set; }
        public string Text { get; set; }
        public List<ArticleContents> Contents { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        private const string temp_sql = "select * from Article " +
                " inner join \"AspNetUsers\" " +
                " On Article.UserId = \"AspNetUsers\".\"Id\" " +
                " left join ArticleContents " +
                " On Article.Id = ArticleContents.ArticleId ";

        public Article()
        {
        }

        public async Task<Article> getAsync(DbConnectionService dbcs, string sql, object param)
        {
            string constr = Startup.ConnectionString;
            var result = await dbcs.GetConnection().QueryAsync<Article, ApplicationUser, ArticleContents, Article>(temp_sql + sql,
                (article, user, articleContents) =>
                {
                    article.User = user;

                    if (article.Contents == null)
                    {
                        article.Contents = new List<ArticleContents>();
                    }

                    if (articleContents != null)
                    {
                        article.Contents.Add(articleContents);
                    }
                    return article;
                },
                param);

            foreach(var a in result)
            {
                return a;
            }

            throw new InvalidOperationException("Error:Not Found Article");
        }

        public async Task<IEnumerable<Article>> search(DbConnectionService dbcs, string sql, object param)
        {
            string constr = Startup.ConnectionString;

            var tmpDict = new Dictionary<long, Article>();
            var result = await dbcs.GetConnection().QueryAsync<Article, ApplicationUser, ArticleContents, Article >(temp_sql + sql,
                (article, user, articleContents) =>
                {
                    Article art;

                    if(!tmpDict.TryGetValue(article.Id, out art))
                    {
                        tmpDict.Add(article.Id, art = article);
                    }

                    if(art.User == null)
                    {
                        art.User = user;
                    }

                    if (art.Contents == null)
                    {
                        art.Contents = new List<ArticleContents>();
                    }

                    if (articleContents != null)
                    {
                        
                        art.Contents.Add(articleContents);
                    }

                    return art;
                },
                param
                ,splitOn: "Id");

            return tmpDict.Values;
        }

        public async Task<int> insert(DbConnectionService dbcs)
        {
            string sql = "INSERT INTO Article VALUES(:Id, :UserId, :PostDate, :Text, :CreatedAt, :UpdatedAt)";

            SequenceService seq = new SequenceService();
            var Id = seq.getNextVal(SequenceService.seqType.Article);
            var now = DateTime.Now;
            var result = await dbcs.GetConnection().ExecuteAsync(sql, new
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
            }, dbcs.GetTransaction());

            if(result == 1)
            {
                this.Id = Decimal.ToInt64(Id);

                List<Task<int>> task = new List<Task<int>>();
                foreach (var artC in this.Contents)
                {
                    artC.ArticleId = this.Id;
                    task.Add(artC.insert(dbcs));
                }
                int[] retC = await Task.WhenAll(task);

                foreach (var r in retC)
                {
                    if (r == 0)
                    {
                        throw new InvalidOperationException("Error:Insert ArticleContents");
                    }
                    result += r;
                }

                return result;
            }
            else
            {
                throw new InvalidOperationException("Error:Insert Article");
            }
        }

        public async Task<int> update(DbConnectionService dbcs)
        {
            if (this.Id == 0)
            {
                throw new InvalidOperationException("Error:Id is Null");
            }
            string sql = "UPDATE Article SET Text = :Text, UpdatedAt = :UpdatedAt where Id = :Id)";
            var result = await dbcs.GetConnection().ExecuteAsync(sql, new
            {
                Id = this.Id
                ,
                Text = this.Text
                ,
                UpdatedAt = DateTime.Now
            }, dbcs.GetTransaction());

            if(result == 0)
            {
                throw new InvalidOperationException("Error:Update Article");
            }

            return result;
        }
    }
}
