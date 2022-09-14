using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace OrchestrationPerformance
{
  public class OrchestrationsBetterPerf
  {
    [FunctionName(nameof(TriggerBetterPerf))]
    public async Task<HttpManagementPayload> TriggerBetterPerf(
      [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "better/{length:int}")] HttpRequest req,
      int length,
      [DurableClient] IDurableOrchestrationClient starter,
      ILogger log)
    {
      var payload = Payloadifier.BOINK(length);

      var instanceId = await starter.StartNewAsync(nameof(BetterPerfOrchestration), payload);

      return starter.CreateHttpManagementPayload(instanceId);
    }

    [FunctionName(nameof(BetterPerfOrchestration))]
    public async Task BetterPerfOrchestration(
      [OrchestrationTrigger] IDurableOrchestrationContext context,
      ILogger log)
    {
      var start = await context.CallActivityAsync<DateTimeOffset>(nameof(RecordTime), context.CurrentUtcDateTime);
      
      var payloads = context.GetInput<List<Payload>>();

      var greetings = await context.CallActivityAsync<List<string>>(nameof(BetterPerfGreeting), payloads.Select(p => p.Name));
      
      await context.CallActivityAsync(nameof(BetterPerfThatLikeBrown), payloads.Where(p => p.Color == Color.Brown));

      await context.CallActivityAsync(nameof(BetterPerfILikeTurtles), payloads.Where(p => p.FavoriteAnimals.Contains(Animal.Turtle)));
      
      var end = context.CurrentUtcDateTime;
      var elapsed = end - start;

      log.LogInformation("-----------------------------------------");
      log.LogInformation($"Finished running orchestration: {nameof(BetterPerfOrchestration)}");
      log.LogInformation($"Total time: {elapsed.TotalSeconds} seconds");
      log.LogInformation("-----------------------------------------");
    }

    [FunctionName(nameof(RecordTime))]
    public DateTimeOffset RecordTime([ActivityTrigger] DateTime current) => current;
    
    [FunctionName(nameof(BetterPerfGreeting))]
    public List<string> BetterPerfGreeting([ActivityTrigger] List<string> names, ILogger log)
    {
      var greetings = new[] { "Hello", "Howdy", "Salutations", "Ayy" };

      return names.Select(name => $"{greetings[new Random().Next(0, greetings.Length)]}, {name}").ToList();
    }

    [FunctionName(nameof(BetterPerfThatLikeBrown))]
    public void BetterPerfThatLikeBrown([ActivityTrigger] List<Payload> payloads, ILogger log)
    {
      var brownLikers = payloads.Where(p => p.Color == Color.Brown);
      log.LogWarning($"Uh oh, turns out {brownLikers.Count()} people like brown");
    }

    [FunctionName(nameof(BetterPerfILikeTurtles))]
    public void BetterPerfILikeTurtles([ActivityTrigger] List<Payload> payloads, ILogger log)
    {
      var fansOfTurles = payloads.Where(p => p.FavoriteAnimals.Contains(Animal.Turtle));
      log.LogWarning($"Turtle fans: {fansOfTurles.Count()}");
    }
  }
}