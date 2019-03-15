using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessingEngineDemo.Common;
using ProcessingEngineDemo.ProcessingEngineExtensions;
using ProcessingEngineDemo.XConnectExtensions;
using Sitecore.Framework.Messaging;
using Sitecore.Framework.Messaging.Rebus.Configuration;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Abstractions.Messages;
using Sitecore.Processing.Tasks.Messaging;
using Sitecore.Processing.Tasks.Messaging.Buses;
using Sitecore.Processing.Tasks.Messaging.Handlers;
using Sitecore.Processing.Tasks.Options.DataSources.Search;
using Sitecore.Processing.Tasks.Options.Workers.ML;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Schema;
using Sitecore.XConnect.Serialization;
using Sitecore.Xdb.Common.Web;

namespace ProcessingEngineDemo.Console
{
    class Application
    {
        private const int TimeoutIntervalMinutes = 15;

        private readonly IConfiguration _config;
        private readonly XdbModel _xDbModel = null;
        private readonly Guid _channelId = new Guid("{ED10766E-C012-47F0-9DA4-7DD223F9EC73}");
        private readonly IServiceProvider _services = null;
        private XConnectClientConfiguration _xConnectClientConfiguration = null;
        private readonly List<Guid> _taskIds = null;

        public Application(IConfiguration config)
        {
            _config = config;
            _xDbModel = MusicModel.Model;
            _services = CreateServices();
            _taskIds = new List<Guid>();
        }

        public async Task RunAsync()
        {
            System.Console.WriteLine("Processing Engine Demo");

            char option = (char)0;
            while (option != 'q')
            {
                PrintOptions();
                option = System.Console.ReadKey().KeyChar;
                System.Console.WriteLine();

                try
                {
                    switch (option)
                    {
                        case '1':
                            await ImportInteractionsAsync();
                            break;

                        case '2':
                            await SearchInteractionsAsync();
                            break;

                        case '3':
                            await RegisterRecommendationTaskAsync();
                            break;

                        case '4':
                            await GetRecommendationFacet();
                            break;

                        case '5':
                            await CheckTaskStatus();
                            break;

                        case 'a':
                            SerializeXConnectModel();
                            break;

                        case 'b':
                            await TestRecommendationAsync();
                            break;

                        case 'q':
                            return;

                        default:
                            System.Console.WriteLine("Unknown option");
                            break;
                    }
                }
                catch (Exception e)
                {
                    var color = System.Console.ForegroundColor;
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine(e);
                    System.Console.ForegroundColor = color;
                }
            }
        }

        private void PrintOptions()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            System.Console.WriteLine("1.    Import POS vinyl sales");
            System.Console.WriteLine("2.    Search for interactions");
            System.Console.WriteLine("3.    Register recommendation task");
            System.Console.WriteLine("4.    Get recommendation facet");
            System.Console.WriteLine("5.    Check task status");
            System.Console.WriteLine();
            System.Console.WriteLine("a.    Serialize xConnect model");
            System.Console.WriteLine("b.    Test recommendation");
            System.Console.WriteLine();
            System.Console.WriteLine("q.    Quit");
            System.Console.WriteLine();
        }

        private void SerializeXConnectModel()
        {
            var json = XdbModelWriter.Serialize(_xDbModel);

            var filename = _xDbModel + ".json";
            File.WriteAllText(filename, json);

            System.Console.WriteLine($"Model written to {filename}");
        }

        private async Task ImportInteractionsAsync()
        {
            var interactionCount = 3;
            System.Console.WriteLine($"Importing {interactionCount} sales as interactions...");

            var albums = new []
            {
                new AlbumDescriptor{ AlbumId = "445uybhAr9ey555kQ6gahx", AlbumName = "Supermasive Black Hole", ArtistId = "12Chz98pHFMPJEknJQMWvI", ArtistName= "Muse"},
                new AlbumDescriptor{ AlbumId = "2wart5Qjnvx1fd7LPdQxgJ", AlbumName = "Drones", ArtistId = "12Chz98pHFMPJEknJQMWvI", ArtistName= "Muse"},
                new AlbumDescriptor{ AlbumId = "3KuXEGcqLcnEYWnn3OEGy0", AlbumName = "The 2nd Law", ArtistId = "12Chz98pHFMPJEknJQMWvI", ArtistName= "Muse"},
                new AlbumDescriptor{ AlbumId = "0eFHYz8NmK75zSplL5qlfM", AlbumName = "The Resistance", ArtistId = "12Chz98pHFMPJEknJQMWvI", ArtistName= "Muse"},
                new AlbumDescriptor{ AlbumId = "0lw68yx3MhKflWFqCsGkIs", AlbumName = "Black Holes And Revelations", ArtistId = "12Chz98pHFMPJEknJQMWvI", ArtistName= "Muse"},
                new AlbumDescriptor{ AlbumId = "0HcHPBu9aaF1MxOiZmUQTl", AlbumName = "Absolution", ArtistId = "12Chz98pHFMPJEknJQMWvI", ArtistName= "Muse"},

                new AlbumDescriptor{ AlbumId = "0pGYWRvsRw7SOFMJEyl8Qu", AlbumName = "The Pale Emporer", ArtistId = "2VYQTNDsvvKN9wmU5W7xpj", ArtistName = "Marilyn Manson"},

                new AlbumDescriptor{ AlbumId = "5LEXck3kfixFaA3CqVE7bC", AlbumName = "White Pony", ArtistId = "6Ghvu1VvMGScGpOUJBAHNH", ArtistName = "Deftones"},
                new AlbumDescriptor{ AlbumId = "4o1KnoVpzXZceJxyjELEQB", AlbumName = "Saturday Night Wrist", ArtistId = "6Ghvu1VvMGScGpOUJBAHNH", ArtistName = "Deftones"},
                new AlbumDescriptor{ AlbumId = "3tsXyEbUQehXPaRFCS8K1n", AlbumName = "Gore", ArtistId = "6Ghvu1VvMGScGpOUJBAHNH", ArtistName = "Deftones"},
                new AlbumDescriptor{ AlbumId = "4PIVdqvL1Rc7T7Vfsr8n8Q", AlbumName = "Koi No Yokan", ArtistId = "6Ghvu1VvMGScGpOUJBAHNH", ArtistName = "Deftones"},
                new AlbumDescriptor{ AlbumId = "1GjjBpY2iDwSQs5bykQI5e", AlbumName = "Diamond Eyes", ArtistId = "6Ghvu1VvMGScGpOUJBAHNH", ArtistName = "Deftones"}
            };

            var random = new Random();

            var xConnectClient = await CreateXConnectClient();

            var contacts = new List<Contact>();

            for (var i = 0; i < interactionCount; i++)
            {
                var contactId = new ContactIdentifier("music", Guid.NewGuid().ToString(), ContactIdentifierType.Known);
                var contact = new Contact(contactId);
                contacts.Add(contact);

                var album = albums[random.Next(0, 3)];
                var outcome = new MusicPurchaseOutcome(DateTime.UtcNow, "AU", 20)
                {
                    Album = album
                };

                var interaction = new Interaction(contact, InteractionInitiator.Contact, _channelId, "music kiosk 1.0");
                interaction.Events.Add(outcome);

                xConnectClient.AddContact(contact);
                xConnectClient.AddInteraction(interaction);
            }

            await xConnectClient.SubmitAsync();

            foreach (var contact in contacts)
            {
                System.Console.WriteLine($"Created contact {contact.Id}");
            }
        }

        private async Task SearchInteractionsAsync()
        {
            var xConnectClient = await CreateXConnectClient();

            var query = xConnectClient.Contacts.Where(contact =>
                contact.Interactions.Any(interaction =>
                    interaction.Events.OfType<MusicPurchaseOutcome>().Any() &&
                    interaction.EndDateTime > DateTime.UtcNow.AddMinutes(-TimeoutIntervalMinutes)
                )
            );

            var expandOptions = new ContactExpandOptions
            {
                Interactions = new RelatedInteractionsExpandOptions()
            };

            query = query.WithExpandOptions(expandOptions);

            var batchEnumerator = await query.GetBatchEnumerator();
            System.Console.WriteLine($"Found {batchEnumerator.TotalCount} interactions");
            
            while (await batchEnumerator.MoveNext())
            {
                foreach (var contact in batchEnumerator.Current)
                {
                    System.Console.WriteLine("====================================");
                    System.Console.WriteLine($"Contact ID {contact.Id}");

                    var outcomes = contact.Interactions.SelectMany(i => i.Events.OfType<MusicPurchaseOutcome>());
                    foreach (var outcome in outcomes)
                    {
                        var album = outcome.Album;
                        System.Console.WriteLine("------------------------------------");
                        System.Console.WriteLine($"Album Id: {album.AlbumId}");
                        System.Console.WriteLine($"Album Name: {album.AlbumName}");
                        System.Console.WriteLine($"Artist Id: {album.ArtistId}");
                        System.Console.WriteLine($"Artist Name: {album.ArtistName}");
                    }
                }
            }
        }

        private async Task TestRecommendationAsync()
        {
            var recommender = _services.GetRequiredService<IMusicRecommender>();
            var albums = await recommender.GetRecommendationsAsync(new[] {"6Ghvu1VvMGScGpOUJBAHNH", "2VYQTNDsvvKN9wmU5W7xpj"}, 5);

            foreach (var album in albums)
            {
                System.Console.WriteLine("------------------------------------");
                System.Console.WriteLine($"Album Id: {album.AlbumId}");
                System.Console.WriteLine($"Album Name: {album.AlbumName}");
                System.Console.WriteLine($"Artist Id: {album.ArtistId}");
                System.Console.WriteLine($"Artist Name: {album.ArtistName}");
            }
        }

        private async Task RegisterRecommendationTaskAsync()
        {
            _taskIds.Clear();

            var taskManager = GetTaskManager();
            var xConnectClient = await CreateXConnectClient();
            var taskTimeout = TimeSpan.FromMinutes(10);
            var storageTimeout = TimeSpan.FromMinutes(30);

            // Prepare data source query
            var query = xConnectClient.Contacts.Where(contact => 
                contact.Interactions.Any(interaction =>
                    interaction.Events.OfType<MusicPurchaseOutcome>().Any() &&
                    interaction.EndDateTime > DateTime.UtcNow.AddMinutes(-TimeoutIntervalMinutes)
                )
            );
                
            var expandOptions = new ContactExpandOptions
            {
                Interactions = new RelatedInteractionsExpandOptions()
            };

            query = query.WithExpandOptions(expandOptions);

            var searchRequest = query.GetSearchRequest();

            // Task for projection
            var dataSourceOptions = new ContactSearchDataSourceOptionsDictionary(
                searchRequest, // searchRequest
                30, // maxBatchSize
                50 // defaultSplitItemCount
            );

            var projectionOptions = new ContactProjectionWorkerOptionsDictionary(
                typeof(AlbumRecommendationModel).AssemblyQualifiedName, // modelTypeString
                storageTimeout, // timeToLive
                "recommendation", // schemaName
                new Dictionary<string, string> // modelOptions
                {
                    { AlbumRecommendationModel.OptionTableName, "contactArtists" }
                }
            );

            var projectionTaskId = await taskManager.RegisterDistributedTaskAsync(
                dataSourceOptions, // datasourceOptions
                projectionOptions, // workerOptions
                null, // prerequisiteTaskIds
                taskTimeout // expiresAfter
            );

            _taskIds.Add(projectionTaskId);

            // Task for merge
            var mergeOptions = new MergeWorkerOptionsDictionary(
                "contactArtistsFinal", // tableName
                "contactArtists", // prefix
                storageTimeout, // timeToLive
                "recommendation" // schemaName
            );

            var mergeTaskId = await taskManager.RegisterDeferredTaskAsync(
                mergeOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    projectionTaskId
                },
                taskTimeout // expiresAfter
            );

            _taskIds.Add(mergeTaskId);

            // Task for recommendation
            var recommendationOptions = new DeferredWorkerOptionsDictionary(
                typeof(AlbumRecommendationWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { AlbumRecommendationWorker.OptionSourceTableName, "contactArtistsFinal" },
                    { AlbumRecommendationWorker.OptionTargetTableName, "contactRecommendations" },
                    { AlbumRecommendationWorker.OptionSchemaName, "recommendation" },
                    { AlbumRecommendationWorker.OptionLimit, "5" }
                }
            );

            var recommendationTaskId = await taskManager.RegisterDeferredTaskAsync(
                recommendationOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    mergeTaskId
                },
                taskTimeout // expiresAfter
            );

            _taskIds.Add(recommendationTaskId);

            // Task to store facet
            var storeFacetOptions = new DeferredWorkerOptionsDictionary(
                typeof(RecommendationFacetStorageWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { RecommendationFacetStorageWorker.OptionTableName, "contactRecommendations" },
                    { RecommendationFacetStorageWorker.OptionSchemaName, "recommendation" }
                }
            );
            
            var storeFacetTaskId = await taskManager.RegisterDeferredTaskAsync(
                storeFacetOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    recommendationTaskId
                },
                taskTimeout // expiresAfter
            );

            _taskIds.Add(storeFacetTaskId);

            foreach (var taskId in _taskIds)
            {
                System.Console.WriteLine($"Registered task {taskId}");
            }
        }

        private async Task GetRecommendationFacet()
        {
            System.Console.WriteLine("Enter ID of contact:");
            var idInput = System.Console.ReadLine();
            var contactId = new Guid(idInput);

            var client = await CreateXConnectClient();
            var contact = await client.GetContactAsync(contactId, new ContactExpandOptions(AlbumRecommendationFacet.DefaultFacetName));
            var facet = contact.GetFacet<AlbumRecommendationFacet>(AlbumRecommendationFacet.DefaultFacetName);

            if (facet == null)
            {
                System.Console.WriteLine("Facet not found");
            }
            else
            {
                System.Console.WriteLine($"AlbumId: {facet.AlbumRecommendations.First().AlbumId}");
                System.Console.WriteLine($"AlbumName: {facet.AlbumRecommendations.First().AlbumName}");
                System.Console.WriteLine($"ArtistId: {facet.AlbumRecommendations.First().ArtistId}");
                System.Console.WriteLine($"ArtistName: {facet.AlbumRecommendations.First().ArtistName}");
            }
        }

        private async Task CheckTaskStatus()
        {
            if (!_taskIds.Any())
            {
                System.Console.WriteLine("No tasks registered...");
                return;
            }

            var taskManager = GetTaskManager();

            foreach (var taskId in _taskIds)
            {
                var progress = await taskManager.GetTaskProgressAsync(taskId);
                if (progress == null)
                    System.Console.WriteLine("Task not found");
                else
                {
                    System.Console.WriteLine("---------------------------------------------------");
                    System.Console.WriteLine($"Task ID: {progress.Id}");
                    System.Console.WriteLine($"Task TaskType: {progress.TaskType}");
                    System.Console.WriteLine($"Task Status: {progress.Status}");
                    System.Console.WriteLine($"Task Progress: {progress.Progress}");
                    System.Console.WriteLine($"Task Total: {progress.Total}");
                }
            }
        }

        private async Task<XConnectClient> CreateXConnectClient()
        {
            if (_xConnectClientConfiguration == null)
            {
                _xConnectClientConfiguration = await CreateXConnectClientConfiguration();
            }

            return new XConnectClient(_xConnectClientConfiguration);
        }

        private async Task<XConnectClientConfiguration> CreateXConnectClientConfiguration()
        {
            System.Console.WriteLine("Initializing xConnect configuration...");

            var uri = new Uri(_config.GetValue<string>("xconnect:uri"));
            var certificateSection = _config.GetSection("xconnect:certificate");
            var handlerModifiers = new List<IHttpClientHandlerModifier>();

            if (certificateSection.GetChildren().Any())
            {
                var certificateModifier = new CertificateHttpClientHandlerModifier(certificateSection);
                handlerModifiers.Add(certificateModifier);
            }

            var xConnectConfigurationClient = new ConfigurationWebApiClient(new Uri(uri + "configuration"), null, handlerModifiers);
            var xConnectCollectionClient = new CollectionWebApiClient(new Uri(uri + "odata"), null, handlerModifiers);
            var xConnectSearchClient = new SearchWebApiClient(new Uri(uri + "odata"), null, handlerModifiers);

            var xConnectClientConfig = new XConnectClientConfiguration(_xDbModel, xConnectCollectionClient, xConnectSearchClient, xConnectConfigurationClient);
            await xConnectClientConfig.InitializeAsync();

            System.Console.WriteLine("xConnect configuration has been initialized");
            return xConnectClientConfig;
        }

        private TaskManager GetTaskManager()
        {
            // Task registration bus
            var taskRegistrationSyncBus =
                _services.GetRequiredService<ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>>();

            // Task progress bus
            var taskProgressSyncBus =
                _services.GetRequiredService<ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>>();

            var options = new TaskManagerOptions(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            var taskManager = new TaskManager(options, taskRegistrationSyncBus, taskProgressSyncBus);
            return taskManager;
        }

        private IServiceProvider CreateServices()
        {
            var serviceCollection = new ServiceCollection();

            // options
            serviceCollection.AddOptions();

            // logging
            serviceCollection.AddLogging();

            // bus
            var rebusConfigSection = _config.GetSection("rebus");
            serviceCollection.AddMessaging(config => config.AddBuses(rebusConfigSection, _ => { }));
            new RebusConfigurationServices(serviceCollection).AddSqlServerConfigurators();

            serviceCollection
                .AddSingleton<ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>,
                    SynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>>();

            serviceCollection
                .AddSingleton<ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>,
                    SynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>>();

            // message handlers
            serviceCollection.AddTransient(typeof(IMessageHandler<TaskRegistrationStatus>), typeof(TaskRegistrationStatusHandler));
            serviceCollection.AddTransient(typeof(IMessageHandler<TaskProgressResponse>), typeof(TaskProgressResponseHandler));

            // music recommender
            var spotifySection = _config.GetSection("spotify");
            serviceCollection.Configure<SpotifyMusicRecommenderOptions>(x => spotifySection.Bind(x));
            serviceCollection.AddSingleton<IMusicRecommender, SpotifyMusicRecommender>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
