using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Servidor.Data;
using Microsoft.EntityFrameworkCore;

public class MatriculaService
{
    private readonly BancoContext _context;

    public MatriculaService(BancoContext context)
    {
        _context = context;
    }

    public async Task GerarMatriculasAsync()
    {
        var TabelaTxt = await _context.Contracheque.ToListAsync(); // Usando ToListAsync para operações assíncronas
        var TabelaExcel = await _context.Administrativo.ToListAsync(); // Usando ToListAsync para operações assíncronas

        // Comparar dados ignorando zeros à esquerda
        var administrativosNormalizados = TabelaExcel.Select(a => new
        {
            Acoluna1 = a.Acoluna1.TrimStart('0'),
            Acoluna2 = a.Acoluna2.TrimStart('0')
        }).ToList();

        // Comparar as tabelas e encontrar discrepâncias
        var discrepancias = TabelaTxt
            .Where(c => !administrativosNormalizados.Any(a =>
                c.Ccoluna2.TrimStart('0') == a.Acoluna1 && // Comparando Ccoluna2 com Acoluna1
                c.Ccoluna3.TrimStart('0') == a.Acoluna2)) // Comparando Ccoluna3 com Acoluna2
            .ToList();

        // Gerar arquivo TXT com as discrepâncias
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "MATRICULA.txt");

        // Usando StreamWriter assíncrono
        await using (var writer = new StreamWriter(filePath))
        {
            foreach (var item in discrepancias)
            {
                // Encontrar o valor correspondente de Acoluna2 para a discrepância
                var aColuna2Discrepancia = administrativosNormalizados
                    .FirstOrDefault(a => a.Acoluna1.TrimStart('0') == item.Ccoluna2.TrimStart('0'))
                    ?.Acoluna2; // Usando o primeiro valor correspondente ou null se não encontrado

                // Salvando Ccoluna2, Acoluna2 e Ccoluna3 no arquivo
                if (aColuna2Discrepancia != null)
                {
                    await writer.WriteLineAsync($"{item.Ccoluna2};{aColuna2Discrepancia};{item.Ccoluna3}");
                }
            }
        }

        Console.WriteLine($"Arquivo salvo em: {filePath}");
    }
}
