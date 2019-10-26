using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashDesk.Model
{
    class Deposit : IDeposit
    {
        [NotMapped]
        IMembership IDeposit.Membership
        {
            get
            {
                return Membership;
            }
        }
        public int DepositId { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
