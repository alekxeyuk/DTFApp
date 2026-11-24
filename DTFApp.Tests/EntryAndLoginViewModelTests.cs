using DTFApp.Models;
using DTFApp.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DTFApp.Tests
{
    [TestClass]
    public class EntryViewModelTests
    {
        [TestMethod]
        public async Task LoadEntryAsync_SetsTitleAndBlocks()
        {
            var api = new FakeDtfApiService
            {
                EntryResponse = new EntryResponse
                {
                    Result = new EntryResult
                    {
                        Title = "Hello",
                        Blocks = new[] { new Block { Type = "text" } },
                        Counters = new Counters { Comments = 7 }
                    }
                }
            };
            var vm = new EntryViewModel(api);

            await vm.LoadEntryAsync(123);

            Assert.AreEqual(123, api.LastRequestedEntryId);
            Assert.AreEqual("Hello", vm.Title);
            Assert.AreEqual(1, vm.Blocks.Length);
            Assert.AreEqual(7, vm.CommentsCount);
            Assert.IsNull(vm.ErrorMessage);
        }

        [TestMethod]
        public async Task LoadEntryAsync_OnException_SetsErrorMessage()
        {
            var api = new FakeDtfApiService { EntryException = new InvalidOperationException("fail") };
            var vm = new EntryViewModel(api);

            await vm.LoadEntryAsync(123);

            Assert.AreEqual("fail", vm.ErrorMessage);
            Assert.AreEqual(Visibility.Visible, vm.ErrorVisibility);
        }
    }

    [TestClass]
    public class LoginViewModelTests
    {
        [TestMethod]
        public async Task LoginAsync_OnSuccess_ShowsFriendlyMessage_WithoutToken()
        {
            var api = new FakeDtfApiService { LoginResult = "{\"result\":{\"token\":\"secret-token\"}}" };
            var vm = new LoginViewModel(api) { Email = "a@b.c", Password = "pw" };

            await vm.LoginAsync();

            Assert.AreEqual("Logged in", vm.StatusText);
            Assert.IsFalse(vm.StatusText.Contains("secret-token"));
        }

        [TestMethod]
        public async Task LoginAsync_MessageResponse_ShowsServerMessage()
        {
            var api = new FakeDtfApiService { LoginResult = "{\"message\":\"email not confirmed\"}" };
            var vm = new LoginViewModel(api) { Email = "a@b.c", Password = "pw" };

            await vm.LoginAsync();

            Assert.AreEqual("email not confirmed", vm.StatusText);
        }

        [TestMethod]
        public async Task RegisterAsync_OnSuccess_ShowsFriendlyMessage()
        {
            var api = new FakeDtfApiService { LoginResult = "{\"result\":{\"token\":\"x\"}}" };
            var vm = new LoginViewModel(api) { Email = "a@b.c", Password = "pw" };

            await vm.RegisterAsync();

            Assert.AreEqual("Registered", vm.StatusText);
        }

        [TestMethod]
        public async Task LoginAsync_OnException_SetsErrorStatus_AndDoesNotThrow()
        {
            var api = new FakeDtfApiService { LoginException = new InvalidOperationException("bad credentials") };
            var vm = new LoginViewModel(api) { Email = "a@b.c", Password = "pw" };

            await vm.LoginAsync();

            Assert.IsTrue(vm.StatusText.StartsWith("Error:", StringComparison.Ordinal));
            StringAssert.Contains(vm.StatusText, "bad credentials");
        }
    }
}
