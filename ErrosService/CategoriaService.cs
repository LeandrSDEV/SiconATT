using Microsoft.EntityFrameworkCore;
using Servidor.Data;

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
                administrativosNormalizados.Any(a => a.Acoluna1 == c.Ccoluna2) && // Verificar se Ccoluna2 existe na Acoluna5
                !administrativosNormalizados.Any(a =>
                    a.Acoluna1 == c.Ccoluna2.TrimStart('0') && // Comparar Ccoluna2 com Acoluna1
                    a.Acoluna5 == c.Ccoluna16.Trim() // Comparar Ccoluna16 com Acoluna5
                )
            )
            .ToList();

        // Gerar o arquivo TXT com as discrepâncias
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "CATEGORIA.txt");

        // Usando StreamWriter assíncrono para escrever no arquivo
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var item in discrepancias)
            {
                // Salvando os valores de Ccoluna2, Ccoluna3 e Ccoluna16 das discrepâncias no arquivo
                await writer.WriteLineAsync($"{item.Ccoluna2};{item.Ccoluna3};{item.Ccoluna16}");
            }
        }

        Console.WriteLine($"Arquivo salvo em: {filePath}");
    }
}