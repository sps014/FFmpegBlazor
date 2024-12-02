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
        runFFmpeg: async (hash, params,errorHash, Dotnet) =>
        {
            let res = null;
            try {
                await ffmpegObjectInstances[hash].run(...params);
            }
            catch (e) {
                res = e.message;
            }
            finally {
                Dotnet.invokeMethodAsync("runCompleted", res, errorHash);
            }
        },
        readFileFFmpeg: async (hash,path) => {
            readFileBuffers[hash] = await ffmpegObjectInstances[hash].FS('readFile', path);
        },
        readFileLength: (obj) => {
            const h = obj.Hash;
            return readFileBuffers[h].byteLength;
        },
        readFileProcess: (h)=>
        {
            return readFileBuffers[h];
        },
        writeFileFFmpeg: async (hash, path, buffer) => {
            await ffmpegObjectInstances[hash].FS("writeFile", path, buffer);

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

            const file = new File([data], name, { type: type });
            return URL.createObjectURL(file);
        },
        revokeObjectURLCleanUp: (name) => {
            URL.revokeObjectURL(name);
        },
        exitfs: (hash) => {
            ffmpegObjectInstances[hash].exit();
        },
        downloadFile: (data, name, type) => {

            const file = new File([data], name, { type: type });
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
