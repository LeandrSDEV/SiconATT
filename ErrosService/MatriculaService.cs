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
        // Carregar os dados das tabelas
        var TabelaTxt = await _context.Contracheque.ToListAsync();
        var TabelaExcel = await _context.Administrativo.ToListAsync();

        // Normalizar as tabelas para facilitar as comparações
        var administrativosNormalizados = TabelaExcel.Select(a => new
        {
            Acoluna1 = a.Acoluna1.TrimStart('0'), // Remover zeros à esquerda
            Acoluna2 = a.Acoluna2.TrimStart('0'),
            Entidade = a // Referência à entidade para atualizações
        }).ToList();

        // Preparar um dicionário de correspondência entre os itens processados
        var correspondencias = new List<(string Ccoluna2, string Acoluna2, string Ccoluna3)>();

        // Percorrer Contracheque para encontrar correspondências únicas com Administrativo
        foreach (var c in TabelaTxt)
        {
            var administrativoParaAtualizar = administrativosNormalizados
                .FirstOrDefault(a => a.Acoluna1 == c.Ccoluna2.TrimStart('0') && // CPF igual
                                      a.Acoluna2 != c.Ccoluna3.TrimStart('0') && // Matrícula diferente
                                      !correspondencias.Any(x => x.Acoluna2 == a.Acoluna2)); // Garantir que ainda não foi usado

            if (administrativoParaAtualizar != null)
            {
                correspondencias.Add((
                    Ccoluna2: c.Ccoluna2.TrimStart('0'),
                    Acoluna2: administrativoParaAtualizar.Acoluna2,
                    Ccoluna3: c.Ccoluna3.TrimStart('0')
                ));
            }
        }

        // Se não houver discrepâncias, encerrar a execução
        if (!correspondencias.Any())
        {
            Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não foi gerado.");
            return;
        }

        // Gerar o arquivo MATRICULA.txt
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "MATRICULA.txt");

        await using (var writer = new StreamWriter(filePath))
        {
            foreach (var discrepancia in correspondencias)
            {
                await writer.WriteLineAsync($"{discrepancia.Ccoluna2};{discrepancia.Acoluna2};{discrepancia.Ccoluna3}");
            }
        }

        // Verificar se o arquivo foi gerado corretamente
        if (new FileInfo(filePath).Length == 0)
        {
            File.Delete(filePath); // Excluir o arquivo vazio
            Console.WriteLine("Arquivo vazio detectado e excluído.");
            return;
        }

        Console.WriteLine($"Arquivo salvo em: {filePath}");

        // Atualizar os valores no banco de dados com as correspondências
        foreach (var correspondencia in correspondencias)
        {
            var administrativoParaAtualizar = TabelaExcel.FirstOrDefault(a =>
                a.Acoluna1.TrimStart('0') == correspondencia.Ccoluna2 &&
                a.Acoluna2.TrimStart('0') == correspondencia.Acoluna2);

            if (administrativoParaAtualizar != null)
            {
                administrativoParaAtualizar.Acoluna2 = correspondencia.Ccoluna3; // Atualiza a matrícula
            }
        }

        // Salvar alterações no banco
        await _context.SaveChangesAsync();
        Console.WriteLine("Tabela Administrativo atualizada com sucesso.");
    }
}
