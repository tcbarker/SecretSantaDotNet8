using SecretSanta.Interfaces;
using Microsoft.AspNetCore.Components;//NavigationManager

namespace SecretSanta.Services;

public class UrlService : IUrlService {

    private readonly NavigationManager? _navigationManager;

    public UrlService(NavigationManager? navigationManager){
        _navigationManager = navigationManager;
    }

    public string BaseUri(){
        return "test://test/";
        //return _navigationManager.BaseUri;//'RemoteNavigationManager' has not been initialized.
    }

}

