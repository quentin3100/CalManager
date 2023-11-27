using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CalManager.Pages;

public partial class UpdatePage : ContentPage
{
    static string[] Scopes = { CalendarService.Scope.Calendar };
    static string ApplicationName = "EventManager";
    private CalendarService _calendarService;
    string calendarId = "domifrag.quent@gmail.com";
    public UpdatePage()
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

    private async Task UpdateEvent(string eventId, string updatedEventName, string updatedLocation, string updatedDescription, DateTime updatedStartDate, DateTime updatedEndDate)
    {

        // R�cup�rer l'�v�nement � mettre � jour
        Event existingEvent = await _calendarService.Events.Get(calendarId, eventId).ExecuteAsync();

        // Mettre � jour les propri�t�s de l'�v�nement existant
        existingEvent.Summary = updatedEventName;
        existingEvent.Location = updatedLocation;
        existingEvent.Description = updatedDescription;
        existingEvent.Start = new EventDateTime { DateTime = updatedStartDate };
        existingEvent.End = new EventDateTime { DateTime = updatedEndDate };

        // Envoyer la requ�te pour mettre � jour l'�v�nement
        await _calendarService.Events.Update(existingEvent, calendarId, eventId).ExecuteAsync();
    }

    async Task<IList<Event>> ListEvents()
    {
        // Sp�cifiez le calendrier � partir duquel vous souhaitez r�cup�rer les �v�nements
        string calendarId = "domifrag.quent@gmail.com";

        // D�finissez les param�tres de votre requ�te pour r�cup�rer les �v�nements du calendrier
        EventsResource.ListRequest request = _calendarService.Events.List(calendarId);
        request.TimeMin = DateTime.Now; // Sp�cifiez la date de d�but pour r�cup�rer les �v�nements � partir de maintenant

        // R�cup�rez les �v�nements � partir de Google Calendar
        Events events = await request.ExecuteAsync();
        return events.Items;
    }

    private async Task<string> FindEventIdByName(string eventName)
    {
        string calendarId = "domifrag.quent@gmail.com";

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

    async Task<bool> EventExistsByName(string eventName)
    {
        string calendarId = "domifrag.quent@gmail.com";

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

    async void OnUpdateEventClicked(object sender, EventArgs e)
    {
        try
        {
            await InitializeCalendar();

            string eventNameToUpdate = eventNameEntry.Text;

            string eventIdToUpdate = await FindEventIdByName(eventNameToUpdate);


            string updatedEventName = eventNameEntry.Text;
            string updatedLocation = eventLocationEntry.Text;
            string updatedDescription = eventDescriptionEntry.Text;
            DateTime updatedStartDate = startDatePicker.Date + startTimePicker.Time;
            DateTime updatedEndDate = endDatePicker.Date + endTimePicker.Time;


            // V�rifiez si l'�v�nement � mettre � jour existe
            bool eventExists = await EventExistsByName(eventNameEntry.Text);

            if (eventExists)
            {
                await UpdateEvent(eventIdToUpdate, updatedEventName, updatedLocation, updatedDescription, updatedStartDate, updatedEndDate);

                await DisplayAlert("Succ�s", "�v�nement mis � jour avec succ�s!", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", "L'�v�nement � mettre � jour n'existe pas.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            await DisplayAlert("Erreur", $"Une erreur s'est produite lors de la mise � jour de l'�v�nement : {ex.Message}", "OK");
        }

    }



}