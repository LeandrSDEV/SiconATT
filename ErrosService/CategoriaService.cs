using Microsoft.EntityFrameworkCore;
using Servidor.Data;
using System.Linq;

public class CategoriaService
{
    private readonly BancoContext _context;

    public CategoriaService(BancoContext context)
    {
        _context = context;
    }

    public async Task GerarVinculoAsync()
    {
        // Carregar os dados das tabelas
        var TabelaTxt = await _context.Contracheque.ToListAsync();
        var TabelaExcel = await _context.Administrativo.ToListAsync();

        // Normalizando os dados da tabela Administrativo
        var administrativosNormalizados = TabelaExcel
            .Select(a => new
            {
                Acoluna1 = a.Acoluna1.TrimStart('0'), // Remover zeros à esquerda de Acoluna1
                Acoluna5 = a.Acoluna5.Trim() // Normalizar Acoluna5
            })
            .ToList();

        // Filtrar as discrepâncias considerando que o Ccoluna2 deve existir na Acoluna5
        var discrepancias = TabelaTxt
    .Where(c =>
        administrativosNormalizados.Any(a => a.Acoluna1 == c.Ccoluna2) && // Verifica se Ccoluna2 existe na Acoluna1
        !administrativosNormalizados.Any(a =>
            a.Acoluna1 == c.Ccoluna2.TrimStart('0') && // Comparação Acoluna1 <-> Ccoluna2
            a.Acoluna5.Equals(c.Ccoluna16.Trim(), StringComparison.OrdinalIgnoreCase) // Comparação segura Acoluna5 <-> Ccoluna16
        )
    )
    .ToList();


        // Verificar se há discrepâncias antes de gerar o arquivo
        if (discrepancias.Any())
        {
            // Agrupar discrepâncias por Ccoluna2
            var discrepanciasAgrupadas = discrepancias
                .GroupBy(d => d.Ccoluna2)
                .ToList();

            // Gerar o arquivo TXT com as discrepâncias
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, "CATEGORIA.txt");

            // Usando StreamWriter assíncrono para escrever no arquivo
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var grupo in discrepanciasAgrupadas)
                {
                    foreach (var item in grupo)
                    {
                        // Escrever cada linha com valores únicos de Ccoluna3
                        await writer.WriteLineAsync($"{item.Ccoluna2};{item.Ccoluna3};{item.Ccoluna16}");
                    }
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
