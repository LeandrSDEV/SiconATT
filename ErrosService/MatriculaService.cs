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
            Acoluna1 = a.Acoluna1,
            Acoluna2 = a.Acoluna2,
            Entidade = a
        }).ToList();

        // Filtrar as discrepâncias, agora garantindo que não gere se Ccoluna3 == Acoluna2
        var discrepancias = TabelaTxt
            .Where(c => !administrativosNormalizados.Any(a =>
                c.Ccoluna2 == a.Acoluna1 &&
                c.Ccoluna3 == a.Acoluna2) &&
                c.Ccoluna3 != c.Ccoluna2) // Garantir que Ccoluna3 e Ccoluna2 não sejam iguais
            .ToList();

        // Verificar se existe alguma discrepância antes de continuar
        if (!discrepancias.Any())
        {
            Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não foi gerado.");
            return;
        }

        // Atualizar os valores da tabela Administrativo com base nas discrepâncias
        foreach (var item in discrepancias)
        {
            var administrativoParaAtualizar = administrativosNormalizados
                .FirstOrDefault(a => a.Acoluna1 == item.Ccoluna2);

            if (administrativoParaAtualizar != null)
            {
                // Atualiza os valores da entidade
                administrativoParaAtualizar.Entidade.Acoluna2 = item.Ccoluna3; // Atualiza a matrícula
            }
        }

        // Salvar alterações no banco de dados
        await _context.SaveChangesAsync();
        Console.WriteLine("Tabela Administrativo atualizada com sucesso.");

        // Verificar se existem discrepâncias válidas para gerar o arquivo
        var discrepanciasValidas = discrepancias
            .Where(item =>
                administrativosNormalizados
                    .Any(a => a.Acoluna1 == item.Ccoluna2) &&
                item.Ccoluna3 != item.Ccoluna2) // Garantir que Ccoluna3 não seja igual a Ccoluna2
            .ToList();

        // Se não houver discrepâncias válidas, não gera o arquivo
        if (!discrepanciasValidas.Any())
        {
            Console.WriteLine("Nenhuma discrepância válida encontrada. Arquivo não foi gerado.");
            return;
        }

        // Gerar o arquivo MATRICULA.txt
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "MATRICULA.txt");

        // Usando StreamWriter assíncrono para escrever no arquivo
        await using (var writer = new StreamWriter(filePath))
        {
            foreach (var item in discrepanciasValidas)
            {
                var aColuna2Discrepancia = administrativosNormalizados
                    .FirstOrDefault(a => a.Acoluna1 == item.Ccoluna2)
                    ?.Entidade.Acoluna2;

                // Verificar se a discrepância não é gerada quando Ccoluna3 == Acoluna2
                if (aColuna2Discrepancia != null && item.Ccoluna3 != aColuna2Discrepancia)
                {
                    await writer.WriteLineAsync($"{item.Ccoluna2};{aColuna2Discrepancia};{item.Ccoluna3}");
                }
            }
        }

        // Verificar se o arquivo foi criado e tem conteúdo
        if (new FileInfo(filePath).Length == 0)
        {
            // Se o arquivo estiver vazio, excluir o arquivo
            File.Delete(filePath);
            Console.WriteLine("Arquivo gerado vazio, e foi excluído.");
        }
        else
        {
            Console.WriteLine($"Arquivo salvo em: {filePath}");
        }
    }
}