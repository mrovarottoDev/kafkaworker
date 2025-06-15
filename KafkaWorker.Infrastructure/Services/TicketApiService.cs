using KafkaWorker.Domain.Dtos;
using System.Net.Http.Json;
 
public class TicketApiService : ITicketApiService
{
    private readonly HttpClient _http;

    public TicketApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<TicketApiResultado> CriarTicketAsync(TicketDados dados)
    {
        //Aqui vai usar o apiworker
        var response = await _http.PostAsJsonAsync("/api/tickets", dados);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TicketApiSuccessResponse>();
            return new TicketApiResultado { Sucesso = true, TicketId = result.TicketId };
        }

        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
        {
            var erro = await response.Content.ReadAsStringAsync();
            return new TicketApiResultado { Sucesso = false, ErroDeNegocio = true, Mensagem = erro };
        }

        var erroTecnico = await response.Content.ReadAsStringAsync();
        return new TicketApiResultado { Sucesso = false, Mensagem = erroTecnico };
    }
}

public class TicketApiResultado
{
    public bool Sucesso { get; set; }
    public string TicketId { get; set; }
    public string Mensagem { get; set; }
    public bool ErroDeNegocio { get; set; }
}

public class TicketApiSuccessResponse
{
    public string TicketId { get; set; }
}
