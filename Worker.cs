using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMongoCollection<MyTask> _taskCollection;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Conectar ao MongoDB usando a string de conexão do appsettings.json
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("tasksdb");
            _taskCollection = database.GetCollection<MyTask>("tasks");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Log antes de buscar tarefas
                    _logger.LogInformation("Iniciando busca de tarefas no MongoDB...");

                    // Obter tarefas do MongoDB
                    var tasks = await _taskCollection.Find(_ => true).ToListAsync(stoppingToken);

                    // Log o número de tarefas encontradas
                    _logger.LogInformation($"Encontradas {tasks.Count} tarefas.");

                    foreach (var task in tasks)
                    {
                        _logger.LogInformation($"Task ID: {task.Id}, Description: {task.Description}");
                    }

                    // Exemplo de inserção de uma nova tarefa
                    var newTask = new MyTask
                    {
                        Description = "Nova tarefa a ser feita"
                    };

                    // Inserir a nova tarefa no MongoDB
                    await _taskCollection.InsertOneAsync(newTask, cancellationToken: stoppingToken);
                    _logger.LogInformation("Nova tarefa inserida: {Description}", newTask.Description);
                }
                catch (MongoException mongoEx)
                {
                    // Log de exceções específicas do MongoDB
                    _logger.LogError(mongoEx, "Erro ao acessar o MongoDB: {Message}", mongoEx.Message);
                }
                catch (Exception ex)
                {
                    // Log de exceções genéricas
                    _logger.LogError(ex, "An error occurred while processing tasks: {Message}", ex.Message);
                }

                // Esperar 10 segundos antes da próxima execução
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

public class MyTask
{
    [BsonId] // Indica que esta propriedade é o ID do documento
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); // Gera um novo ObjectId automaticamente

    public string Description { get; set; }
}

}
