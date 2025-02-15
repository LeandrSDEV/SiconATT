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
        var contracheques = await _context.Contracheque.ToListAsync();
        var administrativos = await _context.Administrativo.ToListAsync();

        // Normalizar Acoluna1 removendo zeros à esquerda
        var administrativosNormalizados = administrativos
            .Select(a => a.Acoluna1.TrimStart('0'))
            .ToList();

        // Criar listas para facilitar contagem de ocorrências
        var cColuna2List = contracheques
            .Select(c => c.Ccoluna2.TrimStart('0').Trim())
            .ToList();

        var discrepancias = new List<dynamic>();

        // Identificar valores duplicados em Ccoluna2
        var duplicatasCcoluna2 = cColuna2List
            .GroupBy(c => c)
            .Select(g => new { Valor = g.Key, QuantidadeC = g.Count() })
            .ToList();

        foreach (var duplicata in duplicatasCcoluna2)
        {
            int ocorrenciasEmAdministrativo = administrativosNormalizados.Count(a => a == duplicata.Valor);

            if (duplicata.QuantidadeC > ocorrenciasEmAdministrativo)
            {
                // Quantidade extra que não tem correspondência em Acoluna1
                int quantidadeExtra = duplicata.QuantidadeC - ocorrenciasEmAdministrativo;

                // Adiciona exatamente a quantidade extra ao arquivo e ao banco
                var linhasExtras = contracheques
                    .Where(c => c.Ccoluna2.TrimStart('0').Trim() == duplicata.Valor)
                    .Take(quantidadeExtra);

                discrepancias.AddRange(linhasExtras);

                // Adicionar as linhas extras diretamente no banco de dados
                foreach (var linha in linhasExtras)
                {
                    var novaLinha = new Servidor.Models.AdministrativoModel
                    {
                        Acoluna1 = linha.Ccoluna2,
                        Acoluna2 = linha.Ccoluna3,
                        Acoluna3 = linha.Ccoluna4,
                        Acoluna4 = linha.Ccoluna21,
                        Acoluna5 = linha.Ccoluna16,
                        Acoluna6 = linha.Ccoluna18
                    };

                    Console.WriteLine($"Adicionando ao banco: {novaLinha.Acoluna1}, {novaLinha.Acoluna2}, {novaLinha.Acoluna3}");
                    _context.Administrativo.Add(novaLinha);
                }
            }
        }

        // Salvar as discrepâncias no banco de dados
        if (discrepancias.Any())
        {
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Discrepâncias salvas no banco de dados com sucesso.");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Erro ao salvar no banco de dados: {dbEx.Message}");
                Console.WriteLine($"Detalhes: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado: {ex.Message}");
            }

            // Gerar o arquivo SERVIDOR.txt
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, "SERVIDOR.txt");

            await using (var writer = new StreamWriter(filePath))
            {
                foreach (var linha in discrepancias)
                {
                    await writer.WriteLineAsync(FormatarLinha(linha));
                }
            }

            Console.WriteLine($"Arquivo salvo em: {filePath}");
        }
        else
        {
            Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não gerado.");
        }
    }

    private string FormatarLinha(dynamic c)
    {
        return string.Join(';', new[]
        {
            c.Ccoluna1, c.Ccoluna2, c.Ccoluna3, c.Ccoluna4, c.Ccoluna5,
            c.Ccoluna6, c.Ccoluna7, c.Ccoluna8, c.Ccoluna9, c.Ccoluna10,
            c.Ccoluna11, c.Ccoluna12, c.Ccoluna13, c.Ccoluna14, c.Ccoluna15,
            c.Ccoluna16, c.Ccoluna17, c.Ccoluna18, c.Ccoluna19, c.Ccoluna20,
            c.Ccoluna21, c.Ccoluna22, c.Ccoluna23, c.Ccoluna24, c.Ccoluna25
        });
    }
}
