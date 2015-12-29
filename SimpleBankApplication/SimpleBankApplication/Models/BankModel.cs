using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;


namespace SimpleBankApplication.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("name=mysimplebankdb")
        {
            Database.SetInitializer<DatabaseContext>(null);
        }
        public DbSet<BankAccount> bank_account_list { get; set; }
        public DbSet<TransactionDetails> transaction_details_list { get; set; }
    }
    [Table("BankAccount")]
    public class BankAccount
    {
        [Key]
        [Required]
        [Display(Name="User Name")]
        [StringLength(50)]
        public string UserName { get; set; }
        [Display(Name = "Account Number")]        
        public decimal AcNumber { get; set; }
        [Required]
        [Display(Name="Name")]
        [StringLength(100)]
        public string Name { get; set; }
        [Display(Name="Address1")]
        [StringLength(100)]
        public string Address1 { get; set; }
        [Display(Name = "Address2")]
        [StringLength(100)]
        public string Address2 { get; set; }
        [Required]
        [Display(Name="Mobile No")]
        public decimal Mobile { get; set; }
        [Required]
        [Display(Name="E-Mail Id")]
        [StringLength(200)]
        [EmailAddress(ErrorMessage="Invalid E-mail")]
        public string Email { get; set; }
        public DateTime CreatedDateTime { get; set; }
        [Display(Name="Account Type")]
        [Required]
        public decimal? AccountType { get; set; }
    }    
    [Table("TransactionDetails")]
    public class TransactionDetails
    {
        [Key]
        public decimal TransactionId { get; set; }
        [Display(Name="User Name")]
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [Display(Name="Transaction Mode")]
        public decimal? TransactionMode { get; set; }
        [Required]
        [Display(Name="Amount")]        
        public decimal Amount { get; set; }
        [StringLength(100)]
        public string DepositorName { get; set; }
        [Display(Name="Transaction Date & Time")]
        public DateTime TransactionDateTime { get; set; }
        [StringLength(50)]
        public string TransferFromUserName { get; set; }
        [StringLength(50)]
        public string TransferToUserName { get; set; }
    }

    public class TransactionModedetails
    {
        public decimal TransModeId { get; set; }
        public string TransModeName { get; set; }
    }

    public class LoginDetails
    {
        [Display(Name="User Name")]
        [Required(ErrorMessage="*")]
        public string UserName { get; set; }
        [Display(Name="Password")]
        [Required(ErrorMessage="*")]
        [DataType(DataType.Password)]
        public string Password { get; set; }             
    }

    public class BankStatement
    {
        public string UserName { get; set; }
        public decimal AcNumber { get; set; }
        public string DateTime { get; set; }
        public string Description { get; set; }
        public string CreditAmount { get; set; }
        public string DebitAmount { get; set; }

    }
}