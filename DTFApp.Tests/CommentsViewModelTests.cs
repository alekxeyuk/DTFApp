using DTFApp.Models;
using DTFApp.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DTFApp.Tests
{
    [TestClass]
    public class CommentsViewModelTests
    {
        private static CommentsResponse TwoLevelComments()
        {
            return new CommentsResponse
            {
                Result = new CommentsResult
                {
                    Items = new[]
                    {
                        new Comment { Id = 1, Text = "root", Level = 0 },
                        new Comment { Id = 2, Text = "child", Level = 1, ReplyTo = 1 },
                        new Comment { Id = 3, Text = "grandchild", Level = 2, ReplyTo = 2 },
                        new Comment { Id = 4, Text = "orphan", Level = 0, ReplyTo = 999 }
                    }
                }
            };
        }

        [TestMethod]
        public async Task LoadCommentsAsync_BuildsTree()
        {
            var api = new FakeDtfApiService { CommentsResponse = TwoLevelComments() };
            var vm = new CommentsViewModel(api);

            await vm.LoadCommentsAsync(42);

            Assert.AreEqual(42, api.LastRequestedContentId);
            Assert.AreEqual(4, vm.Comments.Count);
            var root = vm.Comments[0];
            Assert.AreEqual(1, root.Children.Count);
            Assert.AreEqual(2, root.Children[0].Id);
            Assert.AreEqual(1, root.Children[0].Children.Count);
            Assert.AreEqual(3, root.Children[0].Children[0].Id);
            Assert.IsTrue(root.HasReplies);
        }

        [TestMethod]
        public async Task ToggleCollapse_HidesDescendants()
        {
            var api = new FakeDtfApiService { CommentsResponse = TwoLevelComments() };
            var vm = new CommentsViewModel(api);
            await vm.LoadCommentsAsync(42);

            var root = vm.Comments[0];
            vm.ToggleCollapse(root);

            Assert.IsTrue(root.IsHiddenByCollapse == false);
            Assert.IsTrue(vm.Comments[0].Children[0].IsHiddenByCollapse);
            Assert.IsTrue(vm.Comments[0].Children[0].Children[0].IsHiddenByCollapse);
            Assert.AreEqual(Visibility.Collapsed, vm.Comments[0].Children[0].RowVisibility);

            vm.ToggleCollapse(root);
            Assert.IsFalse(vm.Comments[0].Children[0].IsHiddenByCollapse);
        }

        [TestMethod]
        public async Task LoadCommentsAsync_OnException_SetsErrorMessage()
        {
            var api = new FakeDtfApiService { CommentsException = new InvalidOperationException("nope") };
            var vm = new CommentsViewModel(api);

            await vm.LoadCommentsAsync(42);

            Assert.AreEqual("nope", vm.ErrorMessage);
            Assert.AreEqual(Visibility.Visible, vm.ErrorVisibility);
            Assert.IsFalse(vm.IsLoading);
        }
    }
}
