using CommunityToolkit.Mvvm.ComponentModel;

using PostmanCloneLibrary.Models;

namespace PostmanCloneWPFUI.ViewModels;

public partial class SavedRequestViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _method = "GET";
    [ObservableProperty] private string _url = string.Empty;

    public RequestTab RequestData { get; set; } = new();

    public SavedRequest ToModel()
    {
        RequestData.Name = Name;
        RequestData.Method = Method;
        RequestData.Url = Url;
        RequestData.Response = null;
        return new SavedRequest
        {
            Id = Id,
            Name = Name,
            Method = Method,
            Url = Url,
            RequestData = RequestData.Clone()
        };
    }

    public static SavedRequestViewModel FromModel(SavedRequest model)
    {
        var data = model.RequestData?.Clone() ?? new RequestTab
        {
            Name = model.Name,
            Method = model.Method,
            Url = model.Url
        };
        data.Name = model.Name;
        data.Method = string.IsNullOrWhiteSpace(data.Method) ? model.Method : data.Method;
        data.Url = string.IsNullOrWhiteSpace(data.Url) ? model.Url : data.Url;
        return new SavedRequestViewModel
        {
            Id = model.Id,
            Name = model.Name,
            Method = data.Method,
            Url = data.Url,
            RequestData = data
        };
    }

    public static SavedRequestViewModel FromTab(RequestTabViewModel tab)
    {
        var data = tab.ToModel();
        data.Response = null;
        return new SavedRequestViewModel
        {
            Name = string.IsNullOrWhiteSpace(tab.Name) ? tab.Method + " " + tab.Url : tab.Name,
            Method = tab.Method,
            Url = tab.Url,
            RequestData = data
        };
    }
}
