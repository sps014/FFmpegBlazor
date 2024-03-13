using BlazorBindGen;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegBlazor
{
    public static class Util
    {
        internal static IJSRuntime Runtime { get; set; }

        public static async ValueTask<string> ToBlobURL(string url,string mimeType)
        {
            return (await BindGen.Window.CallAwaitedAsync<string>("toBlobURL", url, mimeType))!;
        }
    }
}
