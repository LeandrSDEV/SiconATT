using Servidor.Models.Enums;
using Servidor.Models;
//======================================    ALCINOPÓLIS    ============================================\\

public class AlcinopolisService
{
    private static readonly Dictionary<string, string> Vinculo = new()
    {
        { "Comissionado", "7" },
        { "Concursado", "2" },
        { "Tempo determinado/Processo seletivo simplificado", "5" },
        { "Prefeito", "13" },
        { "Vice-Prefeito", "13" },
        { "Conselho tutelar", "17" },
        { "Membro de Conselho", "17" }
    };

    public Task<List<ContrachequeModel>> ProcessarArquivoAsync(string[] colunas, Status status)
    {
        var contracheque = ContrachequeHelper.CriarContracheque(colunas, "ALCINOPOLIS");

        if (contracheque.Ccoluna1 == "PREFEITURA MUNICIPAL DE ALCINOPOLIS")
        {
            contracheque.Ccoluna21 = "1";
        }

        if (contracheque.Ccoluna1 == "FUNDO MUN M.D.ED.V.P.ED-FUNDEB")
        {
            contracheque.Ccoluna21 = "2";
        }

        if (Vinculo.ContainsKey(colunas[16].Trim()))
        {
            contracheque.Ccoluna16 = Vinculo[colunas[16].Trim()];
        }

        switch (contracheque.Ccoluna16)
        {
            case "7":
            case "2":
            case "17":
            case "13":
            case "5":
                contracheque.Ccoluna18 = "794";
                break;
            default:
                contracheque.Ccoluna18 = "ERRO";
                break;
        }


        return Task.FromResult(new List<ContrachequeModel> { contracheque });
    }
}
