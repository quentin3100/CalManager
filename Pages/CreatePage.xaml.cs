using CalManager;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
namespace CalManager.Pages;

public partial class CreatePage : ContentPage 
{
    static string[] Scopes = { CalendarService.Scope.Calendar };
    static string ApplicationName = "EventManager";
    private CalendarService _calendarService;
    public CreatePage()
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


    async void OnAddEventClicked(object sender, EventArgs e)
    {
        try
        {
            await InitializeCalendar();

            // Cr�ation d'un nouvel �v�nement
            var newEvent = new Event()
            {
                Summary = eventNameEntry.Text,
                Location = eventLocationEntry.Text,
                Description = eventDescriptionEntry.Text,
                Start = new EventDateTime()
                {
                    DateTime = startDatePicker.Date + startTimePicker.Time,
                    //TimeZone = TimeZoneInfo.Local.Id
                },
                End = new EventDateTime()
                {
                    DateTime = endDatePicker.Date + endTimePicker.Time,
                    //TimeZone = TimeZoneInfo.Local.Id
                }
            };

            string calendarId = "domifrag.quent@gmail.com";

            // Insertion de l'�v�nement dans le calendrier
            EventsResource.InsertRequest request = _calendarService.Events.Insert(newEvent, calendarId);
            Event createdEvent = await request.ExecuteAsync();

            Console.WriteLine("�v�nement cr�� : " + createdEvent.HtmlLink);
            await DisplayAlert("Succ�s", "�v�nement ajout� avec succ�s!", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            await DisplayAlert("Erreur", $"Une erreur s'est produite lors de l'ajout de l'�v�nement : {ex.Message}", "OK");
        }
    }






}