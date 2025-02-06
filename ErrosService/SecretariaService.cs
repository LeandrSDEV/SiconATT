using Microsoft.EntityFrameworkCore;
using Servidor.Data;
using Servidor.Models;
using Servidor.Models.Enums;
using System;
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

    public async Task GerarSecretariasAsync(Status statusSelecionado)
    {
        if (statusSelecionado == default)
        {
            Console.WriteLine("StatusSelecionado não está definido.");
            return;
        }

        // Carregar os dados das tabelas sem rastreamento para melhor performance
        var TabelaTxt = await _context.Contracheque.AsNoTracking().ToListAsync();
        var TabelaExcel = await _context.Administrativo.AsNoTracking().ToListAsync();

        // Normalizando os dados da tabela Administrativo
        var administrativosNormalizados = TabelaExcel
            .Select(a => new
            {
                Acoluna1 = a.Acoluna1.TrimStart('0'), // Remover zeros à esquerda de Acoluna1
                Acoluna4 = a.Acoluna4.Trim()          // Normalizar Acoluna4
            })
            .ToList();

        // Filtrar as discrepâncias considerando as regras de comparação
        var discrepancias = TabelaTxt
            .Where(c =>
                administrativosNormalizados.Any(a =>
                    a.Acoluna1 == c.Ccoluna2.TrimStart('0') &&
                    VerificarDivergencia(c, a.Acoluna4, statusSelecionado)
                )
            )
            .ToList();

        // Verificação para não gerar o arquivo se não houver discrepâncias
        if (!discrepancias.Any())
        {
            Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não foi gerado.");
            return;
        }

        // Gerar o arquivo TXT com as discrepâncias
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "SECRETARIAS.txt");

        try
        {
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
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar o arquivo: {ex.Message}");
        }
    }

    private bool VerificarDivergencia(ContrachequeModel c, string acoluna4, Status statusSelecionado)
    {
        return statusSelecionado switch
        {
            Status.PREF_Cupira_PE =>
                (c.Ccoluna21.TrimStart('0') == "1" && acoluna4 != "PREFEITURA") ||
                (c.Ccoluna21.TrimStart('0') == "3" && acoluna4 != "EDUCAÇÃO"),

            Status.PREF_Aalcinópolis_BA =>
                (c.Ccoluna21.TrimStart('0') == "1" && acoluna4 != "MUNICÍPIO DE ALCINÓPOLIS/MS") ||
                (c.Ccoluna21.TrimStart('0') == "2" && acoluna4 != "FUNDO MUNICIPAL DE EDUCAÇÃO"),

            Status.PREF_Cansanção_BA =>
                (c.Ccoluna21.TrimStart('0') == "1" && acoluna4 != "PREFEITURA MUNICIPAL DE CANSANCAO"),

            Status.PREF_Abare_BA =>
                (c.Ccoluna21.TrimStart('0') == "1" && acoluna4 != "PREFEITURA"),

            Status.PREF_CAFARNAUM_BA =>
                (c.Ccoluna21.TrimStart('0') == "1" && acoluna4 != "PREFEITURA"),

            _ => false
        };
    }
}
