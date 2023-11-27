using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CalManager.Pages;

public partial class DeletePage : ContentPage
{
    static string[] Scopes = { CalendarService.Scope.Calendar };
    static string ApplicationName = "EventManager";
    private CalendarService _calendarService;
    string calendarId = "domifrag.quent@gmail.com";
    public DeletePage()
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


    async Task<bool> EventExistsByName(string eventName)
    {

        // Récupérer la liste des événements
        await _calendarService.Events.List(calendarId).ExecuteAsync();

        IList<Event> events = await ListEvents();

        // Vérifier si un événement avec le nom spécifié existe dans la liste
        foreach (var evt in events)
        {
            if (evt.Summary == eventName)
            {
                return true; // L'événement existe
            }
        }

        return false; // Aucun événement avec ce nom trouvé
    }

    private async Task<string> FindEventIdByName(string eventName)
    {

        EventsResource.ListRequest request = _calendarService.Events.List(calendarId);
        Events events = await request.ExecuteAsync();

        foreach (Event evt in events.Items)
        {
            if (evt.Summary == eventName)
            {
                return evt.Id;
            }
        }

        return null;
    }

    private async Task DeleteEvent(string eventName)
    {

        string eventIdToDelete = await FindEventIdByName(eventName);

        try
        {
            if (eventIdToDelete != null)
            {
                await _calendarService.Events.Delete(calendarId, eventIdToDelete).ExecuteAsync();
            }
        }
        catch
        {
            await DisplayAlert("Erreur", $"Erreur : {eventName} n'existe pas", "OK");
        }

    }


    async void OnDeleteEventClicked(object sender, EventArgs e)
    {
        try
        {
            await InitializeCalendar();

            string eventNameToDelete = eventNameEntry.Text;


            bool eventExists = await EventExistsByName(eventNameToDelete);

            if (eventExists)
            {

                string eventIdToDelete = await FindEventIdByName(eventNameToDelete);

                // Supprimer l'événement s'il existe
                await DeleteEvent(eventNameToDelete);


                await DisplayAlert("Succès", "Événement supprimé avec succès!", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", "Aucun événement avec ce nom n'a été trouvé.", "OK");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            await DisplayAlert("Erreur", $"Une erreur s'est produite lors de la suppression de l'événement : {ex.Message}", "OK");
        }
    }
}