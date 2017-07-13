using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Imageboard10PerformanceTests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ThreadSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var tester = new ThreadSaveTest();
            int pt;
            if (!int.TryParse(UpdateThreadsEdit.Text?.Trim(), out pt))
            {
                pt = 5;
            }
            if (pt < 2)
            {
                pt = 1;
            }
            await tester.Initilize(pt);
            try
            {
                await tester.SaveThreadToStoreBenhcmark(msg =>
                {
                    var unawaitedTask = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Results.Text = msg;
                    });
                }, 10);
            }
            finally
            {
                await tester.Cleanup();
            }
        }
    }
}
