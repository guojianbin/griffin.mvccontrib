using System.Globalization;
using Griffin.MvcContrib.Localization.Views;
using Griffin.MvcContrib.SqlServer.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Griffin.MvcContrib.SqlServer.Tests
{

    [TestClass]
    public class LocalizedViewsRepositoryTests
    {
        public const string Schema =
            @"CREATE TABLE LocalizedTypes(
	Id int IDENTITY(1,1) NOT NULL,
	LocaleId int NOT NULL,
	[Key] nvarchar(250) NOT NULL,
	TypeName nvarchar(255) NOT NULL,
	TextName nvarchar(250) NOT NULL,
	UpdatedAt datetime NOT NULL,
	UpdatedBy nvarchar(50) NOT NULL,
	Value nvarchar(2000) NOT NULL
);


CREATE TABLE LocalizedViews(
	Id int IDENTITY(1,1) NOT NULL,
	LocaleId int NOT NULL,
	[Key] nvarchar(50) NOT NULL,
	ViewPath nvarchar(255) NOT NULL,
	TextName nvarchar(2000) NOT NULL,
	Value nvarchar(2000) NOT NULL,
	UpdatedAt datetime NOT NULL,
	UpdatedBy nvarchar(50) NOT NULL
);";
        private const string ViewPath = "/myarea/controller/index";

        private const string TextName =
            "This is a text that should be translated since it contains a lot of things and so.";

        private readonly ViewPromptKey _key = new ViewPromptKey(ViewPath, TextName);
        private readonly SqlLocalizedViewsRepository _repository;
        private readonly SqlExpressConnectionFactory _factory = new SqlExpressConnectionFactory(Schema);

        public LocalizedViewsRepositoryTests()
        {
            _repository = new SqlLocalizedViewsRepository(_factory);
        }

        [TestMethod]
        public void GetNonExistant()
        {
            var p = _repository.GetPrompt(new CultureInfo(1053), new ViewPromptKey("/some/action/", "forme"));
            Assert.IsNull(p);
        }

        [TestMethod]
        public void TestCreateLang()
        {
            using (var cmd = _factory.Connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM LocalizedViews WHERE LocaleId = 1044";
                cmd.ExecuteNonQuery();
            }

            _repository.CreateLanguage(new CultureInfo(1044), new CultureInfo(1033));
        }


        [TestMethod]
        public void GetExisting()
        {
            _repository.Save(new CultureInfo(1053), ViewPath, TextName, "F�rnamn");

            var prompt = _repository.GetPrompt(new CultureInfo(1053), _key);
            Assert.IsNotNull(prompt);
            Assert.AreEqual("F�rnamn", prompt.TranslatedText);
        }

        [TestMethod]
        public void Update()
        {
            _repository.Save(new CultureInfo(1053), ViewPath, TextName, "F�rrenammne");
            var prompt = _repository.GetPrompt(new CultureInfo(1053), _key);
            Assert.IsNotNull(prompt);
            Assert.AreEqual("F�rrenammne", prompt.TranslatedText);
        }

        [TestMethod]
        public void TwoLanguages()
        {
            _repository.Save(new CultureInfo(1033), ViewPath, TextName, "FirstName");
            _repository.Save(new CultureInfo(1053), ViewPath, TextName, "F�rnamn");


            var enprompt = _repository.GetPrompt(new CultureInfo(1033), _key);
            var seprompt = _repository.GetPrompt(new CultureInfo(1053), _key);
            Assert.IsNotNull(enprompt);
            Assert.IsNotNull(seprompt);
            Assert.AreNotEqual(enprompt.TranslatedText, seprompt.TranslatedText);
        }
    }
}