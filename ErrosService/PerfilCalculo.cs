using Microsoft.EntityFrameworkCore;
using Servidor.Data;
using Servidor.Models.Enums;

namespace Servidor.ErrosService
{
    public class PerfilCalculo
    {
        private readonly BancoContext _bancoContext;

        public PerfilCalculo(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        //==================================  CALCULO =================================//

        public async Task GeradorPerfilCalculo(Status statusSelecionado)
        {
            // Obter os dados da tabela Administrativo
            var TabelaExcel = await _bancoContext.Administrativo.ToListAsync();

            var resultado = new List<string>();

            // Lógica para determinar o valor esperado baseado no statusSelecionado e Acoluna5
            foreach (var linha in TabelaExcel)
            {
                // Normalizar Acoluna6 para comparação, ignorando texto adicional
                var valorAcoluna6 = linha.Acoluna6?.Split('-').LastOrDefault()?.Trim();

                // Lógica para determinar o valor esperado com base no statusSelecionado
                var valorResultado = statusSelecionado switch
                {
                    Status.PREF_Anadia_AL => linha.Acoluna5.Trim() switch
                    {
                        "2" => "171",
                        _ => "329"
                    },
                    Status.PREF_Cafarnaum_BA => linha.Acoluna5.Trim() switch
                    {
                        "1" => "936",
                        "10" => "936",
                        _ => "937"
                    },
                    // Adicione mais status e suas respectivas lógicas conforme necessário
                    _ => throw new ArgumentException("Status inválido.")
                };

                // Verificar se a linha já possui o valor correto na Acoluna6 (ignorar se já está correto)
                if (valorAcoluna6 == valorResultado)
                {
                    continue; // Ignorar a linha se já está com o valor esperado em Acoluna6
                }

                // Adicionar a linha no formato esperado ao resultado
                resultado.Add($"{linha.Acoluna1};{linha.Acoluna2};{valorResultado}");
            }

            // Se não houver resultado, não criar o arquivo
            if (!resultado.Any())
            {
                Console.WriteLine("Nenhuma discrepância encontrada. Arquivo não foi gerado.");
                return;
            }

            // Gerar o arquivo PERFIL_DE_CALCULO.txt
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, "PERFIL DE CALCULO.txt");

            await File.WriteAllLinesAsync(filePath, resultado);

            Console.WriteLine($"Arquivo salvo em: {filePath}");
        }
    }
}
