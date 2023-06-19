using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

public interface IJSInteropAuthorization : System.IDisposable
{
    public Task AlertFunctionInvoke(System.String message);
}

public class JSInteropAuthorization : System.Object, IJSInteropAuthorization
{
    protected ILogger<JSInteropAuthorization> Logger { get; private set; } = default!;
    protected IJSRuntime JSHandling { get; private set; } = default!;

    public JSInteropAuthorization(ILogger<JSInteropAuthorization> logger, IJSRuntime js_link)
        : this(logger) { this.JSHandling = js_link; }

    protected JSInteropAuthorization(ILogger<JSInteropAuthorization> logger) { this.Logger = logger; }

    void System.IDisposable.Dispose() { }

    public async Task AlertFunctionInvoke(string message)
    {
        await JSHandling.InvokeVoidAsync("invoke_exception", message);
    }
}
