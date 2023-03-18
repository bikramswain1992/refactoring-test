using LegacyApp;
using LegacyApp.Models;
using LegacyApp.Providers;
using LegacyApp.Repository;
using LegacyApp.Services;
using LegacyApp.Validators;
using Moq;
using NUnit.Framework;

namespace Refactoring.Test;

public class LegacyAppUserServiceTests
{
    private Mock<IDateTimeProvider> _dateTimeProviderMock = new Mock<IDateTimeProvider>();
    private Mock<IClientRepository> _clientRepositoryMock = new Mock<IClientRepository>();
    private Mock<IUserCreditService> _userCreditServiceMock = new Mock<IUserCreditService>();
    private Mock<IUserDataAccessProvider> _userDataAccessProviderMock = new Mock<IUserDataAccessProvider>();
    private IUserValidator _userValidator;
    private ICreditLimitCalculationService _creditLimitCalculationService;
    private UserService _userService;

    private int _clientId = 1;
    private string _firstName = "Bikram";
    private string _lastName = "Swain";
    private int _creditLimit = 1000;
    private Client _client;
    private User _user;
    private DateTime _dateOfBirth = new DateTime(1990, 2, 2);
    private string _email = "test@gmail.com";

    private static DateTime[] _dateOfBirthTestData = new[]{
        new DateTime(2000, 1, 1),
        new DateTime(1999, 3, 2),
        new DateTime(1999, 2, 3)
    };

    [SetUp]
    public void Setup()
    {
        _client = new Client
        {
            Id = _clientId,
            Name = $"{_firstName} {_lastName}",
            ClientStatus = ClientStatus.none
        };
        _user = new()
        {
            Client = _client,
            DateOfBirth = _dateOfBirth,
            EmailAddress = _email,
            Firstname = _firstName,
            Surname = _lastName,
            HasCreditLimit = true,
            CreditLimit = _creditLimit
        };

        _dateTimeProviderMock
            .Setup(x => x.Now)
            .Returns(new DateTime(2020, 02, 02));

        _clientRepositoryMock
            .Setup(x => x.GetById(It.IsAny<int>()))
            .Returns(_client);

        _userCreditServiceMock
            .Setup(x => x.GetCreditLimit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(_creditLimit);

        _userDataAccessProviderMock
            .Setup(x => x.AddUser(It.IsAny<User>()));

        _dateTimeProviderMock.Invocations.Clear();
        _clientRepositoryMock.Invocations.Clear();
        _userCreditServiceMock.Invocations.Clear();
        _userDataAccessProviderMock.Invocations.Clear();

        _userValidator = new UserValidator(_dateTimeProviderMock.Object);

        _creditLimitCalculationService = new CreditLimitCalculationService(_userCreditServiceMock.Object);

        _userService = new UserService(_clientRepositoryMock.Object, _userDataAccessProviderMock.Object, _userValidator, _creditLimitCalculationService);
    }

    [Test]
    public void AddUser_ShouldAddUser_WhenValidDataIsPassed()
    {
       var result = _userService.AddUser(_firstName, _lastName, _email, _dateOfBirth, _clientId);

        Assert.IsTrue(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Once);
        _clientRepositoryMock.Verify(x => x.GetById(It.Is<int>(y => y == _clientId)), Times.Once);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.Is<string>(y => y == _firstName),
            It.Is<string>(y => y == _lastName),
            It.Is<DateTime>(y => y == _dateOfBirth)), Times.Once);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.Is<User>(y => y.Firstname == _firstName
            && y.Surname == _lastName
            && y.EmailAddress == _email
            && y.CreditLimit == _creditLimit
            && y.HasCreditLimit == true)), Times.Once);
    }

    [TestCase("","Swain","test@gmail.com")]
    [TestCase("Bikram","","test@gmail.com")]
    [TestCase("Bikram","Swain", "test@gmailcom")]
    public void AddUser_ShouldNotAddUser_WhenFirstNameLastNameOrEmailAreInvalid(string firstName, string lastName, string email)
    {
        var result = _userService.AddUser(firstName, lastName, email, _dateOfBirth, _clientId);

        Assert.IsFalse(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Never);
        _clientRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void AddUser_ShouldNotAddUser_WhenAgeIsLessThan21Years([ValueSource(nameof(_dateOfBirthTestData))] DateTime testDateOfBirths)
    {
        var result = _userService.AddUser(_firstName, _lastName, _email, testDateOfBirths, _clientId);

        Assert.IsFalse(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Once);
        _clientRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void AddUser_ShouldAddUser_WhenUserNameIsVeryImportantClient()
    {
        _client.Name = "VeryImportantClient";
        _clientRepositoryMock
            .Setup(x => x.GetById(It.IsAny<int>()))
            .Returns(_client);

        var result = _userService.AddUser(_firstName, _lastName, _email, _dateOfBirth, _clientId);

        Assert.IsTrue(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Once);
        _clientRepositoryMock.Verify(x => x.GetById(It.Is<int>(y => y == _clientId)), Times.Once);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.Is<User>(y => y.Firstname == _firstName
            && y.Surname == _lastName
            && y.EmailAddress == _email
            && y.CreditLimit == default
            && y.HasCreditLimit == false)), Times.Once);
    }

    [Test]
    public void AddUser_ShouldAddUser_WhenUserNameImportantClientAndCreditLimitIsMoreThan500()
    {
        _client.Name = "ImportantClient";
        _clientRepositoryMock
            .Setup(x => x.GetById(It.IsAny<int>()))
            .Returns(_client);

        var result = _userService.AddUser(_firstName, _lastName, _email, _dateOfBirth, _clientId);

        Assert.IsTrue(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Once);
        _clientRepositoryMock.Verify(x => x.GetById(It.Is<int>(y => y == _clientId)), Times.Once);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.Is<string>(y => y == _firstName),
            It.Is<string>(y => y == _lastName),
            It.Is<DateTime>(y => y == _dateOfBirth)), Times.Once);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.Is<User>(y => y.Firstname == _firstName
            && y.Surname == _lastName
            && y.EmailAddress == _email
            && y.CreditLimit == _creditLimit * 2
            && y.HasCreditLimit == true)), Times.Once);
    }

    [Test]
    public void AddUser_ShouldNotAddUser_WhenUserNameImportantClientAndCreditLimitIsLessThan500()
    {
        _client.Name = "ImportantClient";
        _clientRepositoryMock
            .Setup(x => x.GetById(It.IsAny<int>()))
            .Returns(_client);
        _userCreditServiceMock
            .Setup(x => x.GetCreditLimit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(10);

        var result = _userService.AddUser(_firstName, _lastName, _email, _dateOfBirth, _clientId);

        Assert.IsFalse(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Once);
        _clientRepositoryMock.Verify(x => x.GetById(It.Is<int>(y => y == _clientId)), Times.Once);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.Is<string>(y => y == _firstName),
            It.Is<string>(y => y == _lastName),
            It.Is<DateTime>(y => y == _dateOfBirth)), Times.Once);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void AddUser_ShouldNotAddUser_WhenCreditLimitIsLessThan500()
    {
        _userCreditServiceMock
            .Setup(x => x.GetCreditLimit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(10);

        var result = _userService.AddUser(_firstName, _lastName, _email, _dateOfBirth, _clientId);

        Assert.IsFalse(result);
        _dateTimeProviderMock.Verify(x => x.Now, Times.Once);
        _clientRepositoryMock.Verify(x => x.GetById(It.Is<int>(y => y == _clientId)), Times.Once);
        _userCreditServiceMock.Verify(x => x.GetCreditLimit(
            It.Is<string>(y => y == _firstName),
            It.Is<string>(y => y == _lastName),
            It.Is<DateTime>(y => y == _dateOfBirth)), Times.Once);
        _userDataAccessProviderMock.Verify(x => x.AddUser(
            It.IsAny<User>()), Times.Never);
    }
}
