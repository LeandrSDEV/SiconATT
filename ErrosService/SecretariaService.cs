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
                Acoluna1 = a.Acoluna1, // Remover zeros à esquerda de Acoluna1
                Acoluna4 = a.Acoluna4,
                Entidade = a
            })
            .ToList();

        // Filtrar as discrepâncias considerando as regras de comparação
        var discrepancias = TabelaTxt
            .Where(c =>
                administrativosNormalizados.Any(a =>
                    a.Acoluna1 == c.Ccoluna2 &&
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

        // Gerar o arquivo TXT com as discrepâncias primeiro
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
            return; // Não continuar em caso de erro no salvamento do arquivo
        }

        // Atualizar os valores da tabela Administrativo com base nas discrepâncias após gerar o arquivo
        foreach (var item in discrepancias)
        {
            var administrativoParaAtualizar = administrativosNormalizados
                .FirstOrDefault(a => a.Acoluna1 == item.Ccoluna2);

            if (administrativoParaAtualizar != null)
            {
                // Atualiza Acoluna4 com o valor de Ccoluna21 da discrepância (ou outro valor)
                administrativoParaAtualizar.Entidade.Acoluna4 = item.Ccoluna21; // Atualiza Acoluna4
            }
        }

        // Salvar alterações no banco de dados
        await _context.SaveChangesAsync();
        Console.WriteLine("Tabela Administrativo atualizada com sucesso.");
    }

    private bool VerificarDivergencia(ContrachequeModel c, string acoluna4, Status statusSelecionado)
    {
        // Remover espaços extras ao redor de Acoluna4 para uma comparação precisa
        acoluna4 = acoluna4?.Trim();

        return statusSelecionado switch
        {
            Status.PREF_Cupira_PE =>
                (c.Ccoluna21 == "1" && acoluna4 != "PREFEITURA") ||
                (c.Ccoluna21 == "3" && acoluna4 != "EDUCAÇÃO"),

            Status.PREF_Alcinópolis_BA =>
                (c.Ccoluna21 == "1" && acoluna4 != "MUNICÍPIO DE ALCINÓPOLIS/MS") ||
                (c.Ccoluna21 == "2" && acoluna4 != "FUNDO MUNICIPAL DE EDUCAÇÃO"),

            Status.PREF_Cansanção_BA =>
                (c.Ccoluna21 == "1" && acoluna4 != "PREFEITURA MUNICIPAL DE CANSANCAO"),

            Status.PREF_Abare_BA =>
                // Verifica se Acoluna4 é "PREFEITURA" ou "1" com comparação precisa
                (c.Ccoluna21 == "1" && acoluna4 != "PREFEITURA" && acoluna4 != "1"),

            Status.PREF_Cafarnaum_BA =>
                (c.Ccoluna21 == "1" && acoluna4 != "PREFEITURA"),

            Status.PREF_Indiaporã_SP =>
                (c.Ccoluna21 == "1" && acoluna4 != "MUNICÍPIO DE INDIAPORÃ/SP "),

            Status.PREF_Anadia_AL =>
                (c.Ccoluna21 == "300" && acoluna4 != "PREFEITURA MUNICIPAL DE ANADIA" && acoluna4 != "300") ||
                (c.Ccoluna21 == "351" && acoluna4 != "FUNDO MUNICIPAL DE SAUDE DE ANADIA" && acoluna4 != "351"),

            _ => false
        };
    }
}
