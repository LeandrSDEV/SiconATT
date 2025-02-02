using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Servidor.Data;
using Microsoft.EntityFrameworkCore;

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

        // Normalizar os dados da tabela Administrativo (remover os zeros à esquerda de Acoluna1)
        var administrativosNormalizados = TabelaExcel.Select(a => new
        {
            Acoluna1 = a.Acoluna1.TrimStart('0') // Normalizando Acoluna1 para comparar com Ccoluna2
        }).ToList();

        // Comparar as tabelas e encontrar os itens de Contracheque que não existem na tabela Administrativo
        var discrepancias = TabelaTxt
            .Where(c => !administrativosNormalizados.Any(a =>
                c.Ccoluna2.TrimStart('0') == a.Acoluna1)) // Comparando Ccoluna2 com Acoluna1
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
                .SelectMany(g =>
                {
                    // Verificar se há mais de uma linha duplicada para o valor de Ccoluna2
                    var administrativoExistente = administrativosNormalizados.FirstOrDefault(a => a.Acoluna1 == g.Key.TrimStart('0'));
                    if (administrativoExistente != null && g.Count() > 1)
                    {
                        // Se houver duplicatas, adicione todas as linhas duplicadas ao arquivo
                        return g; // Retorna todas as linhas do grupo
                    }
                    return g; // Retorna o grupo mesmo que não tenha duplicata (não precisa de filtro adicional aqui)
                });

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
