using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class PedidoApprovalFunction
{
    [FunctionName("PedidoHttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "pedido")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        // Deserializa o corpo da requisi��o para obter o Pedido
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Pedido pedido = JsonConvert.DeserializeObject<Pedido>(requestBody);

        // Verifica se o Pedido foi fornecido
        if (pedido == null)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("O pedido n�o foi fornecido."),
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        // Inicia a inst�ncia da orquestra��o
        string instanceId = await starter.StartNewAsync("PedidoApprovalFunction", pedido);

        log.LogInformation($"Iniciada inst�ncia da orquestra��o com ID = '{instanceId}'.");

        // Retorna uma resposta com um link para verificar o status da orquestra��o
        return new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent($"Orquestra��o iniciada com ID = '{instanceId}'")
        };
    }

    [FunctionName("PedidoApprovalFunction")]
    public static async Task<string> RunOrchestrator(
       [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        // Recebe o pedido da entrada
        var pedido = context.GetInput<Pedido>();

        // Inicia as atividades em paralelo
        var tasks = new List<Task<string>>();

        tasks.Add(context.CallActivityAsync<string>("SubmeterPedido", pedido));
        tasks.Add(context.CallActivityAsync<string>("PrimeiraAprovacao", pedido));
        tasks.Add(context.CallActivityAsync<string>("SegundaAprovacao", pedido));

        // Aguarda todas as atividades completarem
        await Task.WhenAll(tasks);

        // Verifica os resultados das atividades
        string submissao = tasks[0].Result;
        string primeiraAprovacao = tasks[1].Result;
        string segundaAprovacao = tasks[2].Result;

        // L�gica de decis�o para determinar o resultado final
        if (submissao == "Rejeitado" || primeiraAprovacao == "Rejeitado" || segundaAprovacao == "Rejeitado")
        {
            return "Rejeitado";
        }
        else if (submissao == "Aprovado" && primeiraAprovacao == "Aprovado" && segundaAprovacao == "Aprovado")
        {
            return "Aprovado";
        }
        else
        {
            return "Aprovado com observa��o";
        }
    }

    [FunctionName("PrimeiraAprovacao")]
    public static string PrimeiraAprovacao([ActivityTrigger] Pedido pedido)
    {
        // Simula��o da primeira aprova��o
        if (pedido.ValorTotal <= 500)
        {
            return "Aprovado";
        }
        else if (pedido.ValorTotal <= 1000)
        {
            return "Aprovado com observa��o";
        }
        else
        {
            return "Rejeitado";
        }
    }

    [FunctionName("SegundaAprovacao")]
    public static string SegundaAprovacao([ActivityTrigger] Pedido pedido)
    {
        // Simula��o da segunda aprova��o
        if (pedido.Itens <= 10)
        {
            return "Aprovado";
        }
        else if (pedido.Itens <= 20)
        {
            return "Aprovado com observa��o";
        }
        else
        {
            return "Rejeitado";
        }
    }

    [FunctionName("SubmeterPedido")]
    public static string SubmeterPedido([ActivityTrigger] Pedido pedido)
    {
        // Simula��o de submiss�o de pedido
        if (pedido.ValorTotal <= 1000)
        {
            return "Aprovado";
        }
        else
        {
            return "Rejeitado";
        }
    }

    public class Pedido
    {
        public string Id { get; set; }
        public decimal ValorTotal { get; set; }
        public int Itens { get; set; }
    }
}
