using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.ObjectModel;

namespace CalManager.Pages;

public partial class ListPage : ContentPage
{
    static string[] Scopes = { CalendarService.Scope.Calendar };
    static string ApplicationName = "EventManager";
    private CalendarService _calendarService;
    string calendarId = "domifrag.quent@gmail.com";
    public ListPage()
	{
		InitializeComponent();
        InitializeCalendar();
	}

    private async Task InitializeCalendar()
    {

        UserCredential credential;

        await using (var stream =
            new FileStream("C:\\Users\\Quentin\\Desktop\\2eme_ES\\Prog_Avancee\\CalManager\\bin\\Debug\\client_secret.json", FileMode.Open, FileAccess.Read))
        {
            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
            string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials");


            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);
        }

        // Create Google Calendar API service.
        _calendarService = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });



    }

    async Task<IList<Event>> ListEvents()
    {
       

        // Définissez les paramètres de votre requête pour récupérer les événements du calendrier
        EventsResource.ListRequest request = _calendarService.Events.List(calendarId);
        request.TimeMin = DateTime.Now; // Spécifiez la date de début pour récupérer les événements à partir de maintenant

        // Récupérez les événements à partir de Google Calendar
        Events events = await request.ExecuteAsync();
        return events.Items;
    }

    async void OnListEventsClicked(object sender, EventArgs e)
    {

        try
        {
            await InitializeCalendar();

            IList<Event> events = await ListEvents();


            ObservableCollection<EventViewModel> eventList = new ObservableCollection<EventViewModel>();


            foreach (var evt in events)
            {
                eventList.Add(new EventViewModel
                {
                    Id = evt.Id,
                    Summary = evt.Summary,
                    Location = evt.Location,
                    StartDateTime = evt.Start?.DateTime,
                    EndDateTime = evt.End?.DateTime
                });
            }

            eventsListView.ItemsSource = eventList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            await DisplayAlert("Erreur", $"Une erreur s'est produite lors de la récupération des événements : {ex.Message}", "OK");
        }
    }

}