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
        var TabelaTxt = await _context.Contracheque.ToListAsync();
        var TabelaExcel = await _context.Administrativo.ToListAsync();

        var administrativosNormalizados = TabelaExcel.Select(a => new
        {
            Acoluna1 = a.Acoluna1.TrimStart('0'),
            Acoluna2 = a.Acoluna2.TrimStart('0')
        }).ToList();

        var discrepancias = TabelaTxt
            .Where(c => !administrativosNormalizados.Any(a =>
                c.Ccoluna2.TrimStart('0') == a.Acoluna1 &&
                c.Ccoluna3.TrimStart('0') == a.Acoluna2))
            .ToList();

        // Verificação para não gerar o arquivo se não houver discrepâncias
        if (!discrepancias.Any())
        {
            Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não foi gerado.");
            return;
        }

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "MATRICULA.txt");

        await using (var writer = new StreamWriter(filePath))
        {
            foreach (var item in discrepancias)
            {
                var aColuna2Discrepancia = administrativosNormalizados
                    .FirstOrDefault(a => a.Acoluna1.TrimStart('0') == item.Ccoluna2.TrimStart('0'))
                    ?.Acoluna2;

                if (aColuna2Discrepancia != null)
                {
                    await writer.WriteLineAsync($"{item.Ccoluna2};{aColuna2Discrepancia};{item.Ccoluna3}");
                }
            }
        }

        Console.WriteLine($"Arquivo salvo em: {filePath}");
    }
}
