// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

window.FfmpegBlazorReference = () => {
    return {
        dispose: function () {
            DotNet.disposeJSObjectReference(this);
        },
        alert: function () {
            console.log("ok");
        }, unmarshalledFunction: function (fields) {
            const name = Blazor.platform.readStringField(fields, 0);
            const year = Blazor.platform.readInt32Field(fields, 8);

            //return name === "Brigadier Alistair Gordon Lethbridge-Stewart" &&
            //    year === 1968;
        },
        greatWar: function () {
        },
    };
}
