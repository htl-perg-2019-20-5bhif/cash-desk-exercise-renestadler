using CashDesk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashDesk
{

    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        private MemberDataContext memberDataContext;

        /// <inheritdoc />
        public async Task InitializeDatabaseAsync()
        {
            await Task.Run(() =>
            {
                if (memberDataContext == null)
                {
                    this.memberDataContext = new MemberDataContext();
                }
                else
                {
                    throw new InvalidOperationException("InitializeDatabaseAsync has already been called");
                }
            });
        }

        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            CheckMemberDataContext();
            Member member = new Member { FirstName = firstName, LastName = lastName, Birthday = birthday };
            try
            {
                memberDataContext.Members.Add(member);
                int result = await memberDataContext.SaveChangesAsync();
            }
            catch (InvalidOperationException e)
            {
                throw new DuplicateNameException("Member with the same last name already exists.", e);
            }
            return member.MemberNumber;
        }


        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber)
        {
            CheckMemberDataContext();
            memberDataContext.Members.Remove(memberDataContext.Members.First(member => member.MemberNumber == memberNumber));
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            CheckMemberDataContext();
            if (memberDataContext.Memberships.AsEnumerable().Any(m => m.Member.MemberNumber == memberNumber && (m.End == DateTime.MinValue || m.End > DateTime.Now)))
            {
                throw new AlreadyMemberException("The member is already an active member.");
            }
            Membership membership = new Membership { Member = await GetMemberAsync(memberNumber), Begin = DateTime.Now };
            memberDataContext.Memberships.Add(membership);
            await memberDataContext.SaveChangesAsync();
            return membership;
        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            CheckMemberDataContext();
            Member toAddMembership = memberDataContext.Members.First(member => member.MemberNumber == memberNumber);
            if (toAddMembership == null)
            {
                throw new ArgumentException("Unknown memberNumber");
            }
            if (!memberDataContext.Memberships.Any())
            {
                throw new NoMemberException("The member is currently not an active member.");
            }
            Membership membership = await GetMembershipAsync(memberNumber);
            membership.End = DateTime.Now;
            await memberDataContext.SaveChangesAsync();
            return membership;
        }

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            CheckMemberDataContext();
            Membership membership = await GetMembershipAsync(memberNumber);
            if (amount < 0)
            {
                throw new ArgumentException("Invalid amount");
            }
            Deposit deposit = new Deposit { Amount = amount, Membership = membership };
            memberDataContext.Deposits.Add(deposit);
            await memberDataContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            CheckMemberDataContext();
            var deposit = memberDataContext.Deposits.AsEnumerable()
                .GroupBy(p => new { Year = p.Membership.Begin.Year, Member = p.Membership.Member })
                .Select(p => new DepositStatistics { Member = p.Key.Member, Year = p.Key.Year, TotalAmount = p.Sum(t => t.Amount) });
            return deposit;
        }

        /// <inheritdoc />
        public void Dispose() { }

        public void CheckMemberDataContext()
        {
            if (memberDataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called before");
            }
        }

        private async Task<Member> GetMemberAsync(int memberNumber)
        {
            var member = memberDataContext.Members.First(memberQuery => memberQuery.MemberNumber == memberNumber);
            if (member == null)
                throw new ArgumentException("Unknown memberNumber");

            return member;
        }

        private async Task<Membership> GetMembershipAsync(int memberNumber)
        {
            var membership = memberDataContext.Memberships.AsEnumerable().Any(m => m.Member.MemberNumber == memberNumber && (m.End == DateTime.MinValue || m.End > DateTime.Now));
            if (!membership)
                throw new NoMemberException("The member is currently not an active member.");
            return memberDataContext.Memberships.AsEnumerable().First(m => m.Member.MemberNumber == memberNumber && (m.End == DateTime.MinValue || m.End > DateTime.Now));
        }
    }
}
