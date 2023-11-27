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
       

        // D�finissez les param�tres de votre requ�te pour r�cup�rer les �v�nements du calendrier
        EventsResource.ListRequest request = _calendarService.Events.List(calendarId);
        request.TimeMin = DateTime.Now; // Sp�cifiez la date de d�but pour r�cup�rer les �v�nements � partir de maintenant

        // R�cup�rez les �v�nements � partir de Google Calendar
        Events events = await request.ExecuteAsync();
        return events.Items;
    }


    async Task<bool> EventExistsByName(string eventName)
    {

        // R�cup�rer la liste des �v�nements
        await _calendarService.Events.List(calendarId).ExecuteAsync();

        IList<Event> events = await ListEvents();

        // V�rifier si un �v�nement avec le nom sp�cifi� existe dans la liste
        foreach (var evt in events)
        {
            if (evt.Summary == eventName)
            {
                return true; // L'�v�nement existe
            }
        }

        return false; // Aucun �v�nement avec ce nom trouv�
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

                // Supprimer l'�v�nement s'il existe
                await DeleteEvent(eventNameToDelete);


                await DisplayAlert("Succ�s", "�v�nement supprim� avec succ�s!", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", "Aucun �v�nement avec ce nom n'a �t� trouv�.", "OK");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            await DisplayAlert("Erreur", $"Une erreur s'est produite lors de la suppression de l'�v�nement : {ex.Message}", "OK");
        }
    }
}