using PostmanCloneLibrary;

namespace PostmanCloneUI;

public partial class Dashboard : Form
{
    private readonly IApiAccess api = new ApiAccess();
    public Dashboard()
    {
        InitializeComponent();
    }

    private async void callApi_Click(object sender, EventArgs e)
    {
        systemStatus.Text = "Calling API...";
        responseTextBox.Text = "";

        //Validate the api url
        if (api.IsValidUrl(urlTextBox.Text) == false)
        {
            systemStatus.Text = "Invalid Url...";
            return;

        }

        try
        {

            //Sample code  - replave the actual API call
            responseTextBox.Text = await api.CallApiAsync(urlTextBox.Text);

            systemStatus.Text = "Ready";


        }
        catch (Exception ex)
        {
            responseTextBox.Text = $"Error: {ex.Message}";
            systemStatus.Text = "Error";
        }
    }
}
