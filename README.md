# FFmpegBlazor

[![NuGet Package](https://img.shields.io/badge/nuget-v1.0.0.3%20Preview%204-orange.svg)](https://www.nuget.org/packages/FFmpegBlazor/)
[![NuGet Badge](https://buildstats.info/nuget/FFmpegBlazor)](https://www.nuget.org/packages/FFmpegBlazor/)
![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

 
FFmpegBlazor provides ability to utilize ffmpeg.wasm from Blazor Wasm C#.\
[ffmpeg.wasm](https://github.com/ffmpegwasm/ffmpeg.wasm) is a pure Webassembly / Javascript  port of FFmpeg. It enables video & audio record, convert and stream right inside browsers.\
FFmpegBlazor integrates nicely with Blazor `InputFile` Component. Supports Lazy loading of ffmpeg binary. It is self hosted version one time download of core ffmpeg wasm lib will be 25Mb.

#### Roadmap for .NET 7
* Exploring to reduce extra configuration steps aka no longer need to add Wasm headers. (.NET 7 RC1)
* Robust integration with .NET 7 wasm threads

### Installation

Download package via  [Nuget](https://www.nuget.org/packages/FFmpegBlazor/)  or DotNet CLI and you are good to go , no extra configuration required.
```cli
dotnet add package FFmpegBlazor 
```
[API Documentation](https://github.com/sps014/FFmpegBlazor/wiki)

### Running WASM App

**Currently we need to use a workaround to run FFmpegApps on web assembly, this will be removed in .NET 7 (Early September 2022) once Multi threading support is available on WASM.**

We need to add 2 headers in Blazor WASM-local-server and in actual deployment static server also

```
Cross-Origin-Embedder-Policy: require-corp
Cross-Origin-Opener-Policy: same-origin
```
To do so we can create a `web.config` file in root of our project with content 
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<system.webServer>
		<httpProtocol>
			<customHeaders>
				<add name="Cross-Origin-Embedder-Policy" value="require-corp"/>
				<add name="Cross-Origin-Opener-Policy" value="same-origin"/>
			</customHeaders>
		</httpProtocol>
	</system.webServer>

</configuration>
```
Use [IIS Express](https://github.com/sps014/FFmpegBlazor/issues/9#issuecomment-1059950578) to run apps locally.
Also In actual deployment we need to add these 2 headers in server config to avoid `SharedArrayBuffer not defined` error.
You can check Netlify deployment [sample here](https://github.com/sps014/ffmpegBlazor-Deployed).
<br/>Thanks to [@aokocax](https://github.com/aokocax) for helping with it.

### Sample 
Here is a sample page to convert mp4 to mp3 and play it in browser.

```razor

@page "/"
@using FFmpegBlazor
@inject IJSRuntime Runtime
@using Microsoft.AspNetCore.Components.Forms


<InputFile OnChange="fileLoad" /><br /> <br />
<video width="300" height="200" autoplay controls src="@url" /><br /><br />
<button class="btn btn-primary" @onclick="Process">Convert Mp3</button><br /><br />
<audio controls src="@url2" />

@code
{
    string url; string url2;
    FFMPEG ff;
    byte[] buffer;

    protected override async Task OnInitializedAsync()
    {
        if (FFmpegFactory.Runtime == null)
        {
            FFmpegFactory.Logger += WriteLogs;
            FFmpegFactory.Progress += ProgressChange;
        }

        //initialize Library
        await FFmpegFactory.Init(Runtime);
    }

    async void fileLoad(InputFileChangeEventArgs v)
    {
        //get fist file from input selection
        var file = v.GetMultipleFiles()[0];

        //read all bytes
        using var stream = file.OpenReadStream(100000000); //Max size for file that can be read
        buffer = new byte[file.Size];

        //read all bytes
        await stream.ReadAsync(buffer);

        //create a video link from buffer so that video can be played
        url = FFmpegFactory.CreateURLFromBuffer(buffer, "myFile.mp4", file.ContentType);

        //reRender DOM
        StateHasChanged();
    }

    async void Process()
    {
        //create an instance
        ff = FFmpegFactory.CreateFFmpeg(new FFmpegConfig() { Log = true });

        //download all dependencies from cdn
        await ff.Load(); 

        if (!ff.IsLoaded) return;

        //write buffer to in-memory files (special emscripten files, Ffmpeg only interact with this file)
        ff.WriteFile("myFile.mp4", buffer);

        //Pass CLI argument here equivalent to ffmpeg -i myFile.mp4 output.mp3
        await ff.Run("-i", "myFile.mp4", "output.mp3");

        //delete in-memory file
        //ff.UnlinkFile("myFile.mp4");
    }

    async void ProgressChange(Progress m)
    {
         // display progress % (0-1)
        Console.WriteLine($"Progress {m.Ratio}");

        //if ffmpeg processing is complete (generate a media URL so that it can be played or alternatively download that file)
        if (m.Ratio == 1)
        {
            //get bytepointer from c wasm to c#
            var res = await ff.ReadFile("output.mp3");


            //generate a url from file bufferPointer
            url2 = FFmpegFactory.CreateURLFromBuffer(res, "output.mp3", "audio/mp3");

            //Download the file instantly

            //FFmpegFactory.DownloadBufferAsFile(res, "output.mp3", "audio/mp3");

            StateHasChanged();
        }
    }

    void WriteLogs(Logs m)
    {
        Console.WriteLine(m.Type + " " + m.Message);
    }
}
```
