using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Common {
    public abstract class Bot {
        private readonly BotType _botType;
        private readonly CancellationToken _cancellationToken;
        private readonly HttpClientHandler _httpClientHandler;

        private string _status;

        private readonly SortedSet<BotTask> _tasks =
             new SortedSet<BotTask>(Comparer<BotTask>.Create((a, b) => a.Priority() - b.Priority()));

        protected Bot(BotType botType, CancellationToken token) {
            _botType = botType;
            _cancellationToken = token;

            _httpClientHandler = new HttpClientHandler() {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseProxy = false
            };
        }

        public void Execute() {
            foreach (var task in _tasks.Where(task => task.Validate())) {
                task.Execute();

                _status = task.Description();
                System.Console.WriteLine(_status);
            }
        }

        /**
        * Add tasks to our current BotTask.
        */
        public void Append(params BotTask[] tasks) {
            Array.ForEach(tasks, task => _tasks.Add(task));
        }

        public BotType GetBotType() {
            return _botType;
        }

        public HttpClientHandler GetClientHandler() {
            return _httpClientHandler;
        }

        public CancellationToken GetCancellationToken() {
            return _cancellationToken;
        }
    }
}