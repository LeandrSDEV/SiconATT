﻿using Microsoft.EntityFrameworkCore;
using Servidor.Data;

public class SecretariaService
{
    private readonly BancoContext _context;

    public SecretariaService(BancoContext context)
    {
        _context = context;
    }

    public async Task GerarSecretariasAsync()
    {
        // Carregar os dados das tabelas
        var TabelaTxt = await _context.Contracheque.ToListAsync();
        var TabelaExcel = await _context.Administrativo.ToListAsync();

        // Normalizando os dados da tabela Administrativo
        var administrativosNormalizados = TabelaExcel
            .Select(a => new
            {
                Acoluna1 = a.Acoluna1.TrimStart('0'), // Remover zeros à esquerda de Acoluna1
                Acoluna4 = a.Acoluna4.Trim() // Normalizar Acoluna4
            })
            .ToList();

        // Filtrar as discrepâncias considerando as regras de comparação
        var discrepancias = TabelaTxt
            .Where(c =>
                administrativosNormalizados.Any(a =>
                    a.Acoluna1 == c.Ccoluna2.TrimStart('0') && // CPF existe nas duas tabelas
                    (
                        (c.Ccoluna21.TrimStart('0') == "1" && a.Acoluna4 != "PREFEITURA") || // Divergência para PREFEITURA
                        (c.Ccoluna21.TrimStart('0') == "3" && a.Acoluna4 != "EDUCAÇÃO")    // Divergência para EDUCAÇÃO
                    )
                )
            )
            .ToList();

        // Gerar o arquivo TXT com as discrepâncias
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, "SECRETARIAS.txt");

        // Usando StreamWriter assíncrono para escrever no arquivo
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
}
