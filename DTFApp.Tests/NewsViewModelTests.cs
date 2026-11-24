using DTFApp.Models;
using DTFApp.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DTFApp.Tests
{
    [TestClass]
    public class NewsViewModelTests
    {
        private static NewsResult NewsResultWith(params long[] ids)
        {
            var items = new NewsItem[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                items[i] = new NewsItem { Data = new NewsData { Id = ids[i], Title = "t" + ids[i] } };
            }
            return new NewsResult { News = items, LastId = ids.Length > 0 ? ids[ids.Length - 1] : 0 };
        }

        [TestMethod]
        public async Task LoadNewsAsync_AddsItemsAndTracksLastId()
        {
            var api = new FakeDtfApiService { NewsResult = NewsResultWith(10, 9) };
            var vm = new NewsViewModel(api);

            await vm.LoadNewsAsync();

            Assert.AreEqual(2, vm.NewsItems.Count);
            Assert.AreEqual(0, api.LastRequestedNewsId);
            Assert.IsTrue(vm.HasMoreItems);
            Assert.IsNull(vm.ErrorMessage);
        }

        [TestMethod]
        public async Task LoadNewsAsync_UsesLastIdForPaging()
        {
            var api = new FakeDtfApiService { NewsResult = NewsResultWith(8, 7) };
            var vm = new NewsViewModel(api);
            await vm.LoadNewsAsync();

            api.NewsResult = NewsResultWith(6);
            await vm.LoadNewsAsync();

            Assert.AreEqual(7, api.LastRequestedNewsId);
            Assert.AreEqual(3, vm.NewsItems.Count);
        }

        [TestMethod]
        public async Task LoadNewsAsync_EmptyResult_StopsPaging()
        {
            var api = new FakeDtfApiService { NewsResult = new NewsResult { News = new NewsItem[0], LastId = 0 } };
            var vm = new NewsViewModel(api);

            await vm.LoadNewsAsync();

            Assert.IsFalse(vm.HasMoreItems);
            Assert.AreEqual(0, vm.NewsItems.Count);
            Assert.AreEqual(Visibility.Visible, vm.EmptyVisibility);
        }

        [TestMethod]
        public async Task LoadNewsAsync_OnException_SetsErrorMessage()
        {
            var api = new FakeDtfApiService { NewsException = new InvalidOperationException("boom") };
            var vm = new NewsViewModel(api);

            await vm.LoadNewsAsync();

            Assert.AreEqual("boom", vm.ErrorMessage);
            Assert.AreEqual(Visibility.Visible, vm.ErrorVisibility);
            Assert.IsFalse(vm.IsLoading);
        }

        [TestMethod]
        public async Task RefreshAsync_ClearsItemsAndReloads()
        {
            var api = new FakeDtfApiService { NewsResult = NewsResultWith(5, 4) };
            var vm = new NewsViewModel(api);
            await vm.LoadNewsAsync();
            Assert.AreEqual(2, vm.NewsItems.Count);

            api.NewsResult = NewsResultWith(3);
            await vm.RefreshAsync();

            Assert.AreEqual(1, vm.NewsItems.Count);
            Assert.AreEqual(0, api.LastRequestedNewsId);
        }
    }
}
