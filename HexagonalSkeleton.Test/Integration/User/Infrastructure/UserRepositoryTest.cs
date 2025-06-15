using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Test.Integration.User.Infrastructure
{
    public class UserRepositoryTest(UnitOfWorkFixture fixture) : IClassFixture<UnitOfWorkFixture>
    {
        //[Fact]
        //public void FindAll_Should_Return_All_Entities_Without_Deleted_Ones()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

        //    // Act
        //    var result = unitOfWork.Users.FindAll();

        //    // Assert
        //    var expectedResult = users.Where(w => !w.IsDeleted).Cast<UserEntity>().ToList();
        //    result.Should().HaveCount(expectedResult.Count);
        //    result.Should().BeEquivalentTo(expectedResult);
        //}

        [Fact]
        public async Task FindAllAsync_Should_Return_All_Entities_Without_Deleted_Ones()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

            var result = await unitOfWork.Users.GetAllUsersAsync();

            // Assert
            var expectedResult = users.Where(w => !w.IsDeleted).ToList();
            result.Should().HaveCount(expectedResult.Count);
        }

        //[Fact]
        //public void Find_Should_Return_Entities_Matching_The_Predicate()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

        //    Expression<Func<UserEntity, bool>> predicate = (x) => x.IsDeleted;

        //    var expectedResults = users.Where(predicate.Compile()).ToArray();

        //    var result = unitOfWork.Users.Find(predicate);

        //    // Assert
        //    result.Should().BeEquivalentTo(expectedResults);
        //}

        //[Fact]
        //public async Task FindAsync_Should_Return_Entities_Matching_The_Predicate()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

        //    Expression<Func<UserEntity, bool>> predicate = (x) => x.IsDeleted;
        //    var expectedResults = users.Where(predicate.Compile()).ToArray();

        //    // Act
        //    var result = await unitOfWork.Users.FindAsync(predicate);

        //    // Assert
        //    result.Should().BeEquivalentTo(expectedResults);
        //}

        //[Fact]
        //public void FindAll_ReturnsEmptyDb()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(0, out var users);

        //    var result = unitOfWork.Users.FindAll();
        //    result.Count.Should().Be(0);
        //}

        [Fact]
        public async Task FindOne_UnknownId_ReturnsNull()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out _);

            var result = await unitOfWork.Users.GetUserByIdAsync(9999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindOne_IdZero_ReturnsNull()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out _);

            var result = await unitOfWork.Users.GetUserByIdAsync(0);
            result.Should().BeNull();
        }

        //[Fact]
        //public void FindOne_Should_Return_Single_Entity_Matching_The_Predicate()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

        //    bool Where(UserEntity w) => w.IsDeleted;
        //    var expectedResults = users.Where(Where).ToArray();
        //    var index = new Random().Next(0, expectedResults.Length);
        //    var expectedResult = expectedResults[index] as UserEntity;

        //    // Act
        //    var result = unitOfWork.Users.FindOne(e => e.Name == expectedResult.Name);

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.Should().BeEquivalentTo(expectedResult);
        //}

        //[Fact]
        //public async Task FindOneAsync_Should_Return_Single_Entity_Matching_The_Predicate()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

        //    bool Where(UserEntity w) => w.IsDeleted;
        //    var expectedResults = users.Where(Where).Cast<UserEntity>().ToArray();
        //    var index = new Random().Next(0, expectedResults.Length);
        //    var expectedResult = expectedResults[index];

        //    // Act
        //    var result = await unitOfWork.Users.FindOneAsync(e => e.Name == expectedResult.Name);

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.Should().BeEquivalentTo(expectedResult);
        //}

        [Fact]
        public async Task FindOne_Should_Return_Single_Entity_With_Given_Id_And_Not_Deleted()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

            var expectedResult = users.FirstOrDefault(f => !f.IsDeleted)!;

            // Act
            var result = await unitOfWork.Users.GetUserByIdAsync(expectedResult.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedResult.Id);
        }

        [Fact]
        public async Task FindOneAsync_Should_Return_Single_Entity_With_Given_Id_And_Not_Deleted()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

            var expectedResult = users.FirstOrDefault(f => !f.IsDeleted)!;

            // Act
            var result = await unitOfWork.Users.GetUserByIdAsync(expectedResult.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedResult.Id);
        }

        [Theory, AutoData]
        public async Task CreateAsync_Should_Add_Entity(CancellationTokenSource cts)
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out _);

            var user = new UserEntity
            {
                Email = "test-create-user@test.com",
                Name = "Test",
                Surname = "Test",
                Birthdate = DateTime.Now,
                IsDeleted = false
            };

            await unitOfWork.Users.CreateUserAsync(user, cts.Token);
            await unitOfWork.SaveChangesAsync(cts.Token);
            var result = await unitOfWork.Users.GetUserByIdAsync(id: user.Id, cancellationToken: cts.Token);
            user.Id.Should().BeGreaterThan(0);
            user.Should().BeEquivalentTo(result);
        }

        [Fact]
        public void Create_Should_Add_Entity()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out _);

            var user = new UserEntity
            {
                Email = "test-create-user@test.com",
                Name = "Test",
                Surname = "Test",
                Birthdate = DateTime.Now,
                IsDeleted = false
            };

            unitOfWork.Users.CreateUserAsync(user);
            unitOfWork.SaveChanges();
            var result = unitOfWork.Users.GetUserByIdAsync(user.Id);
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        //[Fact]
        //public void Update_Should_Update_Entity()
        //{
        //    // Arrange
        //    using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out var users);

        //    var user = users.FirstOrDefault(f => !f.IsDeleted)!;
        //    var updatedUser = new UserEntity
        //    {
        //        Id = user.Id,
        //        Email = "",
        //        Name = "Test",
        //        Surname = "Test",
        //        Birthdate = DateTime.Now,
        //        IsDeleted = false
        //    };

        //    unitOfWork.Users.Update(user.Id, updatedUser);
        //    var r = unitOfWork.SaveChanges();
        //    var result = unitOfWork.Users.FindOne(user.Id);
        //    updatedUser.Should().BeEquivalentTo(result);
        //}

        [Fact]
        public async Task HardDelete_Should_Delete_Entity()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out _);

            var user = (await unitOfWork.Users.GetAllUsersAsync()).First();
            await unitOfWork.Users.HardDeleteUser(user.Id);
            unitOfWork.SaveChanges();
            var result = await unitOfWork.Users.GetUserByIdAsync(user.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDelete_Should_Delete_Entity()
        {
            // Arrange
            using var unitOfWork = fixture.GenerateUnitOfWorkMock(10, out _);

            var user = (await unitOfWork.Users.GetAllUsersAsync()).First();

            await unitOfWork.Users.SoftDeleteUser(user.Id);
            unitOfWork.SaveChanges();
            var result = await unitOfWork.Users.GetUserByIdAsync(user.Id);
            result.Should().BeNull();
        }
    }
}
