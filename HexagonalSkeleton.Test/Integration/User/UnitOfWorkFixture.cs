using AutoFixture;
using EntityFrameworkCore.AutoFixture.InMemory;
using EntityFrameworkCore.AutoFixture.Sqlite;
using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace HexagonalSkeleton.Test.Integration.User
{
    public class UnitOfWorkFixture : IUnitOfWorkFixture
    {
        

        public UnitOfWork GenerateUnitOfWorkMock(int count, out IEnumerable<UserEntity> users)
        {
            var fixture = new Fixture().Customize(new InMemoryCustomization());
            users = new Fixture().CreateMany<UserEntity>(count).ToList();

            //foreach (var user in users)
            //{
            //    user.PasswordRaw = Guid.NewGuid().ToString();
            //    user.PasswordSalt = PasswordHasher.GenerateSalt();
            //    user.PasswordHash = PasswordHasher.ComputeHash(user.PasswordRaw, user.PasswordSalt, Settings.Value.Pepper);
            //}

            var appDbContext = fixture.Create<AppDbContext>();
            appDbContext.AddRange(users);
            appDbContext.SaveChanges();
            appDbContext.ChangeTracker.Clear();

            return new UnitOfWork(appDbContext);
        }
    }
}
