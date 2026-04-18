namespace M1ndLink.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    public void SetStatus(string status)
    {
        StatusLabel.Text = status;
    }
}
