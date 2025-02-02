using Microsoft.EntityFrameworkCore;
using Servidor.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class SecretariaService
{
    private readonly BancoContext _context;

    public SecretariaService(BancoContext context)
    {
        _context = context;
    }

    public async Task GerarSecretariasAsync()
    {
        // Carregar os dados das tabelas
        var TabelaTxt = await _context.Contracheque.ToListAsync();
        var TabelaExcel = await _context.Administrativo.ToListAsync();

        // Normalizando os dados da tabela Administrativo
        var administrativosNormalizados = TabelaExcel
            .Select(a => new
            {
                Acoluna1 = a.Acoluna1.TrimStart('0'), // Remover zeros à esquerda de Acoluna1
                Acoluna2 = a.Acoluna2.TrimStart('0'), // Remover zeros à esquerda de Acoluna2
                Acoluna4 = a.Acoluna4.Trim() // Normalizar Acoluna4
            })
            .ToList();

        // Filtrar as discrepâncias e comparar os dados
        var discrepancias = TabelaTxt
            .Where(c =>
                // Se Ccoluna21 é "1", comparar com "PREFEITURA"
                (c.Ccoluna21.TrimStart('0') == "1" &&
                 !administrativosNormalizados.Any(a =>
                     a.Acoluna1 == c.Ccoluna2.TrimStart('0') && // Comparando Ccoluna2 com Acoluna1
                     a.Acoluna2 == c.Ccoluna3.TrimStart('0') && // Comparando Ccoluna3 com Acoluna2
                     a.Acoluna4 == "PREFEITURA")) ||

                // Se Ccoluna21 é "3", comparar com "EDUCAÇÃO"
                (c.Ccoluna21.TrimStart('0') == "3" &&
                 !administrativosNormalizados.Any(a =>
                     a.Acoluna1 == c.Ccoluna2.TrimStart('0') && // Comparando Ccoluna2 com Acoluna1
                     a.Acoluna2 == c.Ccoluna3.TrimStart('0') && // Comparando Ccoluna3 com Acoluna2
                     a.Acoluna4 == "EDUCAÇÃO"))
            )
            .ToList();

        // Gerar o arquivo TXT com as discrepâncias
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "SECRETARIAS.txt");

        // Usando StreamWriter assíncrono para escrever no arquivo
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var item in discrepancias)
            {
                // Salvando os valores de Ccoluna2, Ccoluna3 e Ccoluna21 das discrepâncias no arquivo
                await writer.WriteLineAsync($"{item.Ccoluna2};{item.Ccoluna3};{item.Ccoluna21}");
            }
        }

        Console.WriteLine($"Arquivo salvo em: {filePath}");
    }
}
