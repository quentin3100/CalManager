
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.ObjectModel;


namespace CalManager
{
    public partial class MainPage : ContentPage
    {

        static string[] Scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "EventManager";
        private CalendarService _calendarService;

        public MainPage()
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

                // Création d'un nouvel événement
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

                // Insertion de l'événement dans le calendrier
                EventsResource.InsertRequest request = _calendarService.Events.Insert(newEvent, calendarId);
                Event createdEvent = await request.ExecuteAsync();

                Console.WriteLine("Événement créé : " + createdEvent.HtmlLink);
                await DisplayAlert("Succès", "Événement ajouté avec succès!", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                await DisplayAlert("Erreur", $"Une erreur s'est produite lors de l'ajout de l'événement : {ex.Message}", "OK");
            }
        }

        async Task<IList<Event>> ListEvents()
        {
            // Spécifiez le calendrier à partir duquel vous souhaitez récupérer les événements
            string calendarId = "domifrag.quent@gmail.com";

            // Définissez les paramètres de votre requête pour récupérer les événements du calendrier
            EventsResource.ListRequest request = _calendarService.Events.List(calendarId);
            request.TimeMin = DateTime.Now; // Spécifiez la date de début pour récupérer les événements à partir de maintenant

            // Récupérez les événements à partir de Google Calendar
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

        private async Task DeleteEvent(string eventName)
        {
            string calendarId = "domifrag.quent@gmail.com";

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
                await DisplayAlert("Erreur",$"Erreur : {eventName} n'existe pas","OK");
            }

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

        async Task<bool> EventExistsByName(string eventName)
        {
            string calendarId = "domifrag.quent@gmail.com";

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

        private async Task UpdateEvent(string eventId, string updatedEventName, string updatedLocation, string updatedDescription, DateTime updatedStartDate, DateTime updatedEndDate)
        {
            string calendarId = "domifrag.quent@gmail.com";

            // Récupérer l'événement à mettre à jour
            Event existingEvent = await _calendarService.Events.Get(calendarId, eventId).ExecuteAsync();

            // Mettre à jour les propriétés de l'événement existant
            existingEvent.Summary = updatedEventName;
            existingEvent.Location = updatedLocation;
            existingEvent.Description = updatedDescription;
            existingEvent.Start = new EventDateTime { DateTime = updatedStartDate };
            existingEvent.End = new EventDateTime { DateTime = updatedEndDate };

            // Envoyer la requête pour mettre à jour l'événement
            await _calendarService.Events.Update(existingEvent, calendarId, eventId).ExecuteAsync();
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
                

                // Vérifiez si l'événement à mettre à jour existe
                bool eventExists = await EventExistsByName(eventNameEntry.Text);

                if (eventExists)
                {
                    await UpdateEvent(eventIdToUpdate,updatedEventName, updatedLocation, updatedDescription, updatedStartDate, updatedEndDate);

                    await DisplayAlert("Succès", "Événement mis à jour avec succès!", "OK");
                }
                else
                {
                    await DisplayAlert("Erreur", "L'événement à mettre à jour n'existe pas.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                await DisplayAlert("Erreur", $"Une erreur s'est produite lors de la mise à jour de l'événement : {ex.Message}", "OK");
            }

        }




    }

}
