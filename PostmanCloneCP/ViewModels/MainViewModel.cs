using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostmanCloneLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanCloneCP.ViewModels;


public partial class MainViewModel : ObservableObject
{

    private readonly IApiAccess api=new ApiAccess();

    int count = 0;

    [ObservableProperty]
    private string? _response;

    [ObservableProperty]
    private string? _systemStatus;

    [ObservableProperty]
    private string _apiUrl;

    [RelayCommand]
    async Task CallApi()
    {

        SystemStatus = "Calling API...";
        Response = "";

        //Validate the api url
        if (api.IsValidUrl(ApiUrl) == false)
        {
            SystemStatus = "Invalid Url...";
            return;

        }
        try
        {

            //Sample code  - replave the actual API call
            Response = await api.CallApiAsync(ApiUrl);

            SystemStatus  = "Ready";


        }
        catch (Exception ex)
        {
            Response = $"Error: {ex.Message}";
            SystemStatus = "Error";
        }
    }
}
