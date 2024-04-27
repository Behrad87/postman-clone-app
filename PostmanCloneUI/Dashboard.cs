namespace PostmanCloneUI;

public partial class Dashboard : Form
{
    public Dashboard()
    {
        InitializeComponent();
    }

    private async void callApi_Click(object sender, EventArgs e)
    {
        //Validate the api url


        try
        {
            systemStatus.Text = "Calling API...";

            //Sample code  - replave the actual API call
            await Task.Delay(2000);

            systemStatus.Text = "Ready";


        }
        catch (Exception ex)
        {
            responseTextBox.Text = $"Error: {ex.Message}";
            systemStatus.Text = "Error";
        }
    }
}
