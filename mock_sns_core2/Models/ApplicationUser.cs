using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using System.Threading.Tasks;

namespace mock_sns_core2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(256)]
        [Required]
        [Display(Name = "アプリユーザ名")]
        public string ApplicationUserName { get; set; }

        [MaxLength(2097152)]
        public byte[] Photo { get; set; }

        public async Task<ApplicationUser> getUserAsync(string UserName)
        {
            string sql = "SELECT * FROM \"AspNetUsers\" " +
                " where \"UserName\" = :UserName ";

            string constr = Startup.ConnectionString;
            using (var con = new OracleConnection(constr))
            {
                try
                {
                    con.Open();

                    var result = await con.QueryAsync<ApplicationUser>(sql, new { UserName = UserName });

                    foreach (var d in result)
                    {
                        return d;
                    }

                    return null;
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

        public async Task<IdentityResult> UpdateAsync()
        {
            string sql = "UPDATE \"AspNetUsers\" SET " +
                    " \"Email\" = :Email, " +
                    " \"NormalizedEmail\" = :NormalizedEmail, " +
                    " \"PhoneNumber\" = :PhoneNumber, " +
                    " \"ApplicationUserName\" = :ApplicationUserName, " +
                    " \"Photo\" = :Photo " +
                    " where \"Id\" = :Id";

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
                            Email = this.Email
                            ,
                            NormalizedEmail = this.NormalizedEmail
                            ,
                            PhoneNumber = this.PhoneNumber
                            ,
                            ApplicationUserName = this.ApplicationUserName
                            ,
                            Photo = this.Photo
                        }, tran);

                        if (result == 1)
                        {
                            tran.Commit();
                        }
                        else
                        {
                            throw new NullReferenceException("Not Found ID");
                        }

                        return IdentityResult.Success;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex);

                        return IdentityResult.Failed(new IdentityErrorDescriber().ConcurrencyFailure());
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }
    }
}
