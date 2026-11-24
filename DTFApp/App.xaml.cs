using System;
using Windows.ApplicationModel;
using DTFApp.Services;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DTFApp
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        private static bool _backRequestedRegistered;

        public static event EventHandler<BackRequestedEventArgs> BackRequested;

        private static Type _lastPageType;
        private static object _lastNavigationParameter;
        private const string LastPageTypeKey = "LastPageType";
        private const string LastPageArgKey = "LastPageArg";

        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    var savedPageTypeName = settings.Values[LastPageTypeKey] as string;
                    if (!string.IsNullOrEmpty(savedPageTypeName))
                    {
                        var savedPageType = Type.GetType(savedPageTypeName);
                        if (savedPageType != null && savedPageType != typeof(Views.MainPage))
                        {
                            _lastPageType = savedPageType;
                            _lastNavigationParameter = settings.Values[LastPageArgKey];
                        }
                    }
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Если стек навигации не восстанавливается для перехода к первой странице,
                    // настройка новой страницы путем передачи необходимой информации в качестве параметра
                    // параметр
                    if (_lastPageType != null)
                    {
                        rootFrame.Navigate(_lastPageType, _lastNavigationParameter);
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(Views.MainPage), e.Arguments);
                    }
                }

                // Обеспечение активности текущего окна
                Window.Current.Activate();

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;

                rootFrame.Navigated += (s, args) =>
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                        rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                    _lastPageType = args.SourcePageType;
                    _lastNavigationParameter = args.Parameter;
                };

                if (!_backRequestedRegistered)
                {
                    SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
                    _backRequestedRegistered = true;
                }

                _ = BadgeCacheService.UpdateBadgesAsync();
            }
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            BackRequested?.Invoke(this, e);
            if (!e.Handled && Window.Current.Content is Frame rootFrame && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            if (_lastPageType != null)
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                settings.Values[LastPageTypeKey] = _lastPageType.FullName;
                settings.Values[LastPageArgKey] = _lastNavigationParameter;
            }
            deferral.Complete();
        }
    }
}
