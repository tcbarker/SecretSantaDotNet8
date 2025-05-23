using Microsoft.AspNetCore.Components;

namespace SecretSanta.Client.Pages.Components;

public class ConfirmationBoxBase<T> : ComponentBase {

    public class ButtonOption {
        public string ButtonText { get; set; } = "This thing?";
        public string CssClass { get; set; } = "btn-primary"; //btn-warning
        public T? Value { get; set; }
    }

    protected bool Displayed { get; set; } = false;
    [Parameter] public string MainText { get; set; } = "Do the thing?";
    [Parameter] public string MainClass { get; set; } = "btn-primary";
    [Parameter] public ButtonOption[] ButtonOptions { get; set; } = [];
    [Parameter] public EventCallback<T> CallbackFunc { get; set; }


    public delegate void Voidddelegate(EventArgs args);
    public static event Voidddelegate? closeevent;

    public void Closer(EventArgs args) {
        Displayed = false;
    }

    protected override async Task OnInitializedAsync() {
        await base.OnInitializedAsync();
        closeevent+=Closer;
    }

    public void Dispose() {//@implements IDisposable
        closeevent-=Closer;
    }


    public async Task ButtonPressed(T value) {
        Displayed = false;
        if(value!=null){
            await CallbackFunc.InvokeAsync(value);
            closeevent?.Invoke(EventArgs.Empty);
        }
    }

    public void OpenConfirmation() {
        Displayed = true;
    }

}

