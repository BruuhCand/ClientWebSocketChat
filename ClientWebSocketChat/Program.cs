using System.Net.WebSockets;
using System.Text;


Console.WriteLine("Digite seu nome");
string nome = Console.ReadLine();




using var ws = new ClientWebSocket(); // Cria uma instância do cliente WebSocket

// Conecta ao servidor WebSocket no endereço e porta especificados
await ws.ConnectAsync(new Uri("ws://192.168.15.13:5124"), CancellationToken.None);

Console.WriteLine("Bem vindo ao chat. Escreva suas mensagens e aperte ENTER para enviar. Escreva 'exit' para sair do chat."); //printa

// Tarefa para receber mensagens do servidor
var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024 * 4]; // Buffer para receber mensagens
    while (ws.State == WebSocketState.Open) // Mantém a conexão enquanto estiver aberta
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None); // Recebe uma mensagem
        if (result.MessageType == WebSocketMessageType.Close) //compara se o estado do Socket esta fechado
        {
            // Fecha a conexão se a mensagem for de fechamento
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
        else
        {
            // Converte o buffer recebido em uma string e exibe no console
            var message = Encoding.ASCII.GetString(buffer, 0, result.Count);
            Console.WriteLine($"{message}"); //printa
        }
    }
});

// Loop para ler mensagens do console e enviá-las ao servidor
while (ws.State == WebSocketState.Open) // continua rodando enquanto socket esta aberto
{
    var message = Console.ReadLine(); // Lê uma mensagem do console
    if (message.ToLower() == "exit") // compara mensagem com sair
    {
        // Fecha a conexão se a mensagem for "exit"
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Usuario fechou o chat", CancellationToken.None); // fecha o chat
        break; //quebra o while
    }
    message = $"{nome}: {message}"; // concatena a mensagem e nome
    // Converte a mensagem em bytes e envia ao servidor
    var data = Encoding.ASCII.GetBytes(message); //transforma mensagem array de byte
    await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None); //manda a mensagem 
}

// Aguarda a conclusão da tarefa de recebimento
await receiveTask;