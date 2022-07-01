// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

window.ffmpegObjectInstances = new Object();
window.readFileBuffers = new Object();

window.FfmpegBlazorReference = () => {
    return {
        createFFmpeg: (hash, config, Dotnet) => {
            if (config.corePath == null)
                ffmpegObjectInstances[hash] = FFmpeg.createFFmpeg({
                    log: config.log,
                    logger: (message) => Dotnet.invokeMethodAsync("logger", message),
                    progress: (p) => Dotnet.invokeMethodAsync("progress", p)
            });
            else
                ffmpegObjectInstances[hash] = FFmpeg.createFFmpeg({
                    corePath: config.corePath,
                    log: config.log,
                    logger: ({ message }) => Dotnet.invokeMethodAsync("logger", message),
                    progress: (p) => Dotnet.invokeMethodAsync("progress", p)
                });

        },
        loadFFmpeg: async (hash) =>
        {
            await ffmpegObjectInstances[hash].load();
        },
        runFFmpeg: async (hash, params,Dotnet) =>
        {
            try {
                await ffmpegObjectInstances[hash].run(...params);
            }
            catch (e) {
                Dotnet.invokeMethodAsync("OnErr", e.message);
            }

        },
        readFileFFmpeg: async (obj) => {
            const h = Blazor.platform.readInt32Field(obj, 8);
            const p = Blazor.platform.readStringField(obj, 0);
            readFileBuffers[h] = await ffmpegObjectInstances[h].FS('readFile', p);
            return true; //Uint8Array array
        },
        readFileLength: (obj) => {
            const h = Blazor.platform.readInt32Field(obj, 8);
            return readFileBuffers[h].byteLength;
        },
        readFileProcess: (h)=>
        {
            return readFileBuffers[h];
        },
        writeFileFFmpeg: async (obj, buffer)=>{
            const contentArray = Blazor.platform.toUint8Array(buffer);
            const h = Blazor.platform.readInt32Field(obj, 8);
            const p = Blazor.platform.readStringField(obj, 0);
            await ffmpegObjectInstances[h].FS("writeFile", p, contentArray);

        },
        unlinkFileFFmpeg: async (h, p) => {
            await ffmpegObjectInstances[h].FS('unlink', p);
        },
        fsFFmpeg: async (h, method, args) => {
            return await ffmpegObjectInstances[h].FS(method, ...args);
        },
        dispose: function () {
            DotNet.disposeJSObjectReference(this);
        },
        isLoadedFFmpeg: (h) => {
            return ffmpegObjectInstances[h].isLoaded();
        },
        setFFmpegEvent: (h, dotnet) => {
            ffmpegObjectInstances[h].setProgress((p) => { dotnet.invokeMethodAsync("progress", p); });
            ffmpegObjectInstances[h].setLogger((p) => { dotnet.invokeMethodAsync("logger", p); });
        },
        disposeFFmpeg: (hash) => {
            delete ffmpegObjectInstances[hash];
        },
        createObjectURL: (data,name,type) => {
            const contentArray = Blazor.platform.toUint8Array(data);
            const nameStr = BINDING.conv_string(name);
            const contentTypeStr = BINDING.conv_string(type);

            const file = new File([contentArray], nameStr, { type: contentTypeStr });
            return BINDING.js_to_mono_obj(URL.createObjectURL(file));
        },
        revokeObjectURLCleanUp: (name) => {
            URL.revokeObjectURL(name);
        },
        exitfs: (hash) => {
            ffmpegObjectInstances[hash].exit();
        },
        downloadFile: (data, name, type) => {
            const contentArray = Blazor.platform.toUint8Array(data);
            const nameStr = BINDING.conv_string(name);
            const contentTypeStr = BINDING.conv_string(type);

            const file = new File([contentArray], nameStr, { type: contentTypeStr });
            const exportUrl = URL.createObjectURL(file);

            const a = document.createElement("a");
            document.body.appendChild(a);
            a.href = exportUrl;
            a.download = nameStr;
            a.target = "_self";
            a.click();

            URL.revokeObjectURL(exportUrl);
        }
    };
}
