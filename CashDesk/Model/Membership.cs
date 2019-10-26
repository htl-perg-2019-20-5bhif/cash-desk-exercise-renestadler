using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashDesk.Model
{
    class Membership : IMembership
    {
        public int MembershipId { get; set; }

        [Required]
        public Member Member { get; set; }

        [NotMapped]
        IMember IMembership.Member
        {
            get
            {
                return Member;
            }
        }

        [Required]
        public DateTime Begin { get; set; }

        public DateTime End { get; set; }
    }
}
