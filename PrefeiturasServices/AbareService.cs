using Servidor.Models.Enums;
using Servidor.Models;

public class AbareService
{
    private static readonly Dictionary<string, string> MapeamentoStatus = new()
    {
        { "Efetivo", "2" },
        { "Conselho Tutelar", "17" },
        { "Processo Seletivo", "11" },
        { "Trabalhador Temporário", "11" },
        { "Cargo em Comissão", "7" },
        { "Agente Político", "13" }
    };

    public Task<List<ContrachequeModel>> ProcessarArquivoAsync(string[] colunas, Status status)
    {
        var contracheque = new ContrachequeModel
        {
            Ccoluna1 = colunas[7],
            Ccoluna2 = colunas[3],
            Ccoluna3 = colunas[4],
            Ccoluna4 = colunas[5],
            Ccoluna5 = "Rua A",
            Ccoluna6 = "S/N",
            Ccoluna7 = "CASA",
            Ccoluna8 = "CENTRO",
            Ccoluna9 = "ABARE",
            Ccoluna10 = "BA",
            Ccoluna11 = "99999999",
            Ccoluna12 = "0",
            Ccoluna13 = "0",
            Ccoluna14 = "99999999999",
            Ccoluna15 = colunas[9],
            Ccoluna16 = colunas[16],
            Ccoluna17 = "0",
            Ccoluna18 = colunas[18],
            Ccoluna19 = "0",
            Ccoluna20 = "Teste@gmail.com",
            Ccoluna21 = colunas[19],
            Ccoluna22 = "0",
            Ccoluna23 = colunas[10],
            Ccoluna24 = "0",
            Ccoluna25 = "0"
        };

        if (contracheque.Ccoluna1 == "PREFEITURA MUNICIPAL DE ABARE")
        {
            contracheque.Ccoluna21 = "1";
        }

        // Verifica e atualiza Ccoluna16 com base no mapeamento
        if (MapeamentoStatus.ContainsKey(colunas[16].Trim()))
        {
            contracheque.Ccoluna16 = MapeamentoStatus[colunas[16].Trim()];
        }

        switch (contracheque.Ccoluna16)
        {
            case "7":
            case "13":
            case "5":
            case "17":
            case "11":
                contracheque.Ccoluna18 = "679";
                break;
            case "2":
                contracheque.Ccoluna18 = "678";
                break;
            default:
                contracheque.Ccoluna18 = "ERRO";
                break;
        }

        return Task.FromResult(new List<ContrachequeModel> { contracheque });
    }
}
