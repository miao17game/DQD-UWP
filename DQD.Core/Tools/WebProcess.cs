using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DQD.Core.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace DQD.Core. Tools {
    public static class WebProcess {
        static CoreDispatcher Dispatcher = Window.Current.Dispatcher;
        /// <summary>
        /// Get the html in string by string-Uri
        /// </summary>
        /// <param name="urlString">Target web uri-string</param>
        /// <returns></returns>
        public  static async Task<StringBuilder> GetHtmlResources ( string urlString ) {
            var LrcStringBuider = new StringBuilder ( );
            try {
                var request = WebRequest.Create(urlString) as HttpWebRequest;
                request.Method = "GET";
                try {
                    using (var response = await request.GetResponseAsync() as HttpWebResponse) {
                        var stream = response.GetResponseStream();
                        var streamReader = new StreamReader(stream, Encoding.UTF8);
                        LrcStringBuider.Append(await streamReader.ReadToEndAsync());
                    }
                } catch (WebException ex) {
                    Debug.WriteLine("\nTimeOut：\n" + ex.Message);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { new ToastSmooth("网络超时，请重试").Show(); });
                    return null;
                } catch (Exception e) {
                    Debug.WriteLine("\nTimeOut：\n" + e.Message);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { new ToastSmooth("网络异常，请重试").Show(); });
                    return null;
                } request.Abort();
            } catch {
                Debug.WriteLine("\nTimeOut：\n" );
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { new ToastSmooth("网络异常，请检查网络").Show(); });
                return null;
            }
            return LrcStringBuider;
        }

    }
}
