using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;

namespace OrchestrationPerformance
{
  public class OrchestrationsBadPerf
  {
    [FunctionName(nameof(TriggerBadPerf))]
    public async Task<HttpManagementPayload> TriggerBadPerf(
      [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "bad/{length:int}")] HttpRequest req,
      int length,
      [DurableClient] IDurableOrchestrationClient starter,
      ILogger log)
    {
      var payload = Payloadifier.BOINK(length);

      var instanceId = await starter.StartNewAsync(nameof(BadPerfOrchestration), payload);

      return starter.CreateHttpManagementPayload(instanceId);
    }

    [FunctionName(nameof(BadPerfOrchestration))]
    public async Task BadPerfOrchestration(
      [OrchestrationTrigger] IDurableOrchestrationContext context,
      ILogger log)
    {
      var start = await context.CallActivityAsync<DateTimeOffset>(nameof(OrchestrationsBetterPerf.RecordTime), context.CurrentUtcDateTime);

      var payloads = context.GetInput<List<Payload>>();

      var greetings = await context.CallSubOrchestratorAsync<List<string>>(nameof(BadPerfGreetingDispatcher), payloads);

      await context.CallActivityAsync(nameof(BadPerfThatLikeBrown), payloads);

      await context.CallActivityAsync(nameof(BadPerfILikeTurtles), payloads);
      
      var end = context.CurrentUtcDateTime;
      var elapsed = end - start;

      log.LogInformation("-----------------------------------------");
      log.LogInformation($"Finished running orchestration: {nameof(BadPerfOrchestration)}");
      log.LogInformation($"Total time: {elapsed.TotalSeconds} seconds");
      log.LogInformation("-----------------------------------------");

    }

    [FunctionName(nameof(BadPerfGreetingDispatcher))]
    public async Task<List<string>> BadPerfGreetingDispatcher([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
    {
      log.LogInformation($"Dispatching to {nameof(BadPerfGreeting)}");

      var payloads = context.GetInput<List<Payload>>();
      return await context.CallActivityAsync<List<string>>(nameof(BadPerfGreeting), payloads);
    }

    [FunctionName(nameof(BadPerfGreeting))]
    public List<string> BadPerfGreeting([ActivityTrigger] List<Payload> payloads, ILogger log)
    {
      var greetings = new[] { "Hello", "Howdy", "Salutations", "Ayy" };

      return payloads.Select(payload => $"{greetings[new Random().Next(0, greetings.Length)]}, {payload.Name}").ToList();
    }

    [FunctionName(nameof(BadPerfThatLikeBrown))]
    public void BadPerfThatLikeBrown([ActivityTrigger] List<Payload> payloads, ILogger log)
    {
      var brownLikers = payloads.Where(p => p.Color == Color.Brown);
      log.LogWarning($"Uh oh, turns out {brownLikers.Count()} people like brown");
    }

    [FunctionName(nameof(BadPerfILikeTurtles))]
    public void BadPerfILikeTurtles([ActivityTrigger] List<Payload> payloads, ILogger log)
    {
      var fansOfTurles = payloads.Where(p => p.FavoriteAnimals.Contains(Animal.Turtle));
      log.LogWarning($"Turtle fans: {fansOfTurles.Count()}");
    }
  }
}
