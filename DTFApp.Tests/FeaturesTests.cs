using DTFApp.Models;
using DTFApp.Services;
using DTFApp.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace DTFApp.Tests
{
    [TestClass]
    public class FavoritesServiceTests
    {
        [TestMethod]
        public void Toggle_AddsThenRemoves()
        {
            var item = new NewsData { Id = 9001, Title = "fav" };
            Assert.IsTrue(FavoritesService.Toggle(item));
            Assert.IsTrue(FavoritesService.IsFavorite(9001));
            Assert.IsFalse(FavoritesService.Toggle(item));
            Assert.IsFalse(FavoritesService.IsFavorite(9001));
        }
    }

    [TestClass]
    public class CommentsComposerTests
    {
        [TestMethod]
        public async Task SendComment_WithoutLogin_SetsError()
        {
            SettingsService.AuthToken = null;
            var api = new FakeDtfApiService
            {
                CommentsResponse = new CommentsResponse { Result = new CommentsResult { Items = new Comment[0] } }
            };
            var vm = new CommentsViewModel(api) { NewCommentText = "hello" };

            await vm.SendCommentAsync(5);

            Assert.AreEqual("Login required to comment", vm.ErrorMessage);
            Assert.IsNull(api.LastCommentText);
        }

        [TestMethod]
        public async Task SendComment_LoggedIn_SendsAndClearsText()
        {
            SettingsService.AuthToken = "token-123";
            var api = new FakeDtfApiService
            {
                CommentsResponse = new CommentsResponse { Result = new CommentsResult { Items = new Comment[0] } }
            };
            var vm = new CommentsViewModel(api) { NewCommentText = "  hello  " };

            await vm.SendCommentAsync(5);

            Assert.AreEqual(5, api.LastCommentContentId);
            Assert.AreEqual("hello", api.LastCommentText);
            Assert.IsNull(vm.NewCommentText);
            SettingsService.AuthToken = null;
        }

        [TestMethod]
        public async Task SendComment_ApiFailure_SetsError()
        {
            SettingsService.AuthToken = "token-123";
            var api = new FakeDtfApiService
            {
                AddCommentException = new InvalidOperationException("cannot post"),
                CommentsResponse = new CommentsResponse { Result = new CommentsResult { Items = new Comment[0] } }
            };
            var vm = new CommentsViewModel(api) { NewCommentText = "hello" };

            await vm.SendCommentAsync(5);

            Assert.AreEqual("cannot post", vm.ErrorMessage);
            SettingsService.AuthToken = null;
        }
    }

    [TestClass]
    public class LoginTokenTests
    {
        [TestMethod]
        public async Task LoginAsync_StoresTokenFromResponse()
        {
            SettingsService.AuthToken = null;
            var api = new FakeDtfApiService { LoginResult = "{\"result\":{\"token\":\"abc-123\"}}" };
            var vm = new LoginViewModel(api) { Email = "a@b.c", Password = "pw" };

            await vm.LoginAsync();

            Assert.AreEqual("abc-123", SettingsService.AuthToken);
            Assert.AreEqual("Logged in", vm.StatusText);
            SettingsService.AuthToken = null;
        }

        [TestMethod]
        public async Task RegisterAsync_WithoutName_DoesNotCallApi()
        {
            var api = new FakeDtfApiService { LoginResult = "{\"result\":{}}" };
            var vm = new LoginViewModel(api) { Email = "a@b.c", Password = "pw", Name = " " };

            await vm.RegisterAsync();

            Assert.AreEqual("Enter name", vm.StatusText);
        }
    }
}
