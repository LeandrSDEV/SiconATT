using Microsoft.EntityFrameworkCore;
using Servidor.Data;

public class ServidorService
{
    private readonly BancoContext _context;

    public ServidorService(BancoContext context)
    {
        _context = context;
    }

    public async Task GerarEncontradoAsync()
    {
        // Carregar os dados das tabelas
        var TabelaTxt = await _context.Contracheque.ToListAsync();
        var TabelaExcel = await _context.Administrativo.ToListAsync();

        // Normalizar os dados da tabela Administrativo (remover os zeros à esquerda de Acoluna1 e Acoluna2)
        var administrativosNormalizados = TabelaExcel.Select(a => new
        {
            Acoluna1 = a.Acoluna1.TrimStart('0'), // Normalizando Acoluna1
            Acoluna2 = a.Acoluna2.TrimStart('0')  // Normalizando Acoluna2
        }).ToList();

        // Comparar as tabelas e encontrar os itens de Contracheque que não existem na tabela Administrativo
        var discrepancias = TabelaTxt
            .Where(c =>
                // Verifica se a combinação de Ccoluna2 e Ccoluna3 não está na tabela Administrativo
                !administrativosNormalizados.Any(a =>
                    c.Ccoluna2.TrimStart('0') == a.Acoluna1 &&
                    c.Ccoluna3.TrimStart('0') == a.Acoluna2)) // Desconsiderando os zeros à esquerda
            .ToList();

        // Verificar se há discrepâncias antes de gerar o arquivo
        if (discrepancias.Any())
        {
            // Gerar o arquivo TXT com as discrepâncias
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, "SERVIDOR.txt");

            // Agrupar discrepâncias por Ccoluna2 para tratar valores repetidos
            var discrepanciasAgrupadas = discrepancias
                .GroupBy(d => d.Ccoluna2)
                .SelectMany(g => g);  // Agrupa todas as linhas de discrepância

            // Usando StreamWriter assíncrono
            await using (var writer = new StreamWriter(filePath))
            {
                foreach (var item in discrepanciasAgrupadas)
                {
                    // Salvando as discrepâncias no arquivo
                    await writer.WriteLineAsync($"{item.Ccoluna1};{item.Ccoluna2};{item.Ccoluna3};{item.Ccoluna4};{item.Ccoluna5};{item.Ccoluna6};{item.Ccoluna7};{item.Ccoluna8};{item.Ccoluna9};{item.Ccoluna10};{item.Ccoluna11};{item.Ccoluna12};{item.Ccoluna13};{item.Ccoluna14};{item.Ccoluna15};{item.Ccoluna16};{item.Ccoluna17};{item.Ccoluna18};{item.Ccoluna19};{item.Ccoluna20};{item.Ccoluna21};{item.Ccoluna22};{item.Ccoluna23};{item.Ccoluna24};{item.Ccoluna25}");
                }
            }

            Console.WriteLine($"Arquivo salvo em: {filePath}");
        }
        else
        {
            Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não gerado.");
        }
    }
}
