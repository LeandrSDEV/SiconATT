using Microsoft.EntityFrameworkCore;
using System.Text;
using Servidor.Models.Enums;
using Servidor.Data;

public class GeradorDePerfil
{
    private readonly BancoContext _context;

    public GeradorDePerfil(BancoContext context)
    {
        _context = context;
    }

    public async Task GerarPerfilAcessoAsync(Status statusSelecionado)
    {
        var nomeArquivo = $"PERFIL DE ACESSO.txt";
        // Obtém o caminho da área de trabalho
        var caminhoDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var caminhoArquivo = Path.Combine(caminhoDesktop, nomeArquivo);

        // Código do município correspondente ao status selecionado
        string codigoMunicipio = statusSelecionado switch
        {
            Status.PREF_Abare_BA => "123456", // Código fixo para PREF_Abare_BA
            Status.PREF_Cupira_PE => "654321", // Código fixo para PREF_Cupira_PE
            Status.PREF_Alcinópolis_BA => "789012",
            Status.PREF_Anadia_AL => "2411",
            _ => "000000" // Código padrão para status desconhecidos
        };

        // Traz todos os registros relevantes do banco (somente os campos necessários)
        var registrosBanco = await _context.Administrativo
            .Select(a => new
            {
                a.Acoluna1,
                a.Acoluna5
            })
            .ToListAsync();

        // Aplica o filtro para retornar apenas registros válidos para o status e valor esperado
        var registrosFiltrados = registrosBanco
            .Where(a => DeveGerarLinha(statusSelecionado, a.Acoluna5))
            .ToList();

        if (!registrosFiltrados.Any())
        {
            return; // Se não houver registros válidos, não cria o arquivo
        }

        // Gera o conteúdo do arquivo
        var sb = new StringBuilder();
        foreach (var registro in registrosFiltrados)
        {
            // Sempre escreve o código do município no segundo campo
            sb.AppendLine($"{registro.Acoluna1};{codigoMunicipio}");
        }

        // Escreve o arquivo na área de trabalho
        await File.WriteAllTextAsync(caminhoArquivo, sb.ToString(), Encoding.UTF8);
    }

    public static bool DeveGerarLinha(Status statusSelecionado, string acoluna5)
    {
        return statusSelecionado switch
        {
            Status.PREF_Cupira_PE => acoluna5 == "2", // Só inclui se acoluna5 for "2"
            Status.PREF_Alcinópolis_BA => acoluna5 == "2", 
            Status.PREF_Cansanção_BA => acoluna5 == "2", 
            Status.PREF_Abare_BA => acoluna5 == "2", 
            Status.PREF_Anadia_AL => acoluna5 == "2", 
            _ => false // Ignora para qualquer outro status ou valor
        };
    }

}
