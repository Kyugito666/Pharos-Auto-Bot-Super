// Author: Kyugito666
// Original Source Code from GitHub user: linuxdil

using Grpc.Net.Client;
using Grpc.Core;
using Pharos;
using static Pharos.PharosService;

Console.Title = "Pharos Automation Suite by Kyugito666";
var channel = GrpcChannel.ForAddress("http://localhost:50051");
var client = new PharosServiceClient(channel);

while (true)
{
    DisplayMenu();
    var choice = Console.ReadKey(true).Key;

    if (choice == ConsoleKey.Q) break;

    string selectedBot = GetBotFromChoice(choice);

    if (!string.IsNullOrEmpty(selectedBot))
    {
        Console.Clear();
        Console.WriteLine($"Memulai bot: {selectedBot}...");
        await RunBotAsync(client, selectedBot);
        Console.WriteLine("\nEksekusi selesai. Tekan tombol apa saja untuk kembali ke menu.");
        Console.ReadKey();
    }
}

Console.WriteLine("Menutup aplikasi...");

// --- FUNGSI-FUNGSI ---

async Task RunBotAsync(PharosServiceClient client, string botName)
{
    var request = new BotRequest { BotName = botName };

    try
    {
        request.PrivateKeys.AddRange(File.ReadAllLines("../accounts.txt"));
        request.Proxies.AddRange(File.ReadAllLines("../proxy.txt"));
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] Gagal membaca file accounts.txt atau proxy.txt: {ex.Message}");
        Console.ResetColor();
        return;
    }

    // Di sini Anda akan meminta input parameter dari user
    // Contoh:
    if (botName == "AutoStaking") {
         Console.Write("Masukkan jumlah stake: ");
         string amount = Console.ReadLine() ?? "0";
         request.Parameters.Add("amount", amount);
    }

    try
    {
        using var call = client.RunBot(request);

        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            var color = response.Level switch
            {
                BotResponse.Types.LogLevel.Info => ConsoleColor.Cyan,
                BotResponse.Types.LogLevel.Success => ConsoleColor.Green,
                BotResponse.Types.LogLevel.Warn => ConsoleColor.Yellow,
                BotResponse.Types.LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
            Console.ForegroundColor = color;
            Console.WriteLine($"[{response.Timestamp}] [{response.Level}] {response.Message}");
            Console.ResetColor();
        }
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"FATAL: Tidak dapat terhubung ke backend Rust. Pastikan server sudah berjalan.");
        Console.ResetColor();
    }
}

void DisplayMenu()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"
██████╗ ██╗  ██╗ █████╗ ██████╗  ██████╗ ███████╗
██╔══██╗██║  ██║██╔══██╗██╔══██╗██╔═══██╗██╔════╝
██████╔╝███████║███████║██████╔╝██║   ██║███████╗
██╔═══╝ ██╔══██║██╔══██║██╔══██╗██║   ██║╚════██║
██║     ██║  ██║██║  ██║██████╔╝╚██████╔╝███████║
╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═════╝  ╚═════╝ ╚══════╝
");
Console.WriteLine("    -- Automation Suite by Kyugito666 --\n");
Console.ResetColor();

// Menu diurutkan berdasarkan tanggal rilis (paling lama di atas)
// dan faucet di nomor 1
Console.WriteLine(" [1] Claim Faucet (Contoh, belum diimplementasi)");
Console.WriteLine(" [2] Auto");
Console.WriteLine(" [3] R2");
Console.WriteLine(" [4] Faro-Swap");
Console.WriteLine(" [5] Domain");
Console.WriteLine(" [6] AutoStaking");
Console.WriteLine(" [7] Grandline");
Console.WriteLine(" [8] Brokex");
Console.WriteLine(" [9] OpenFi");
Console.WriteLine(" [A] Bitverse");
Console.WriteLine("\n [F] Jalankan Semua Fitur (Full Run)");
Console.WriteLine(" [Q] Keluar");
Console.Write("\n Pilih Opsi: ");
}

string GetBotFromChoice(ConsoleKey key)
{
    return key switch
    {
        ConsoleKey.D1 => "ClaimFaucet",
        ConsoleKey.D2 => "Auto",
        ConsoleKey.D3 => "R2",
        ConsoleKey.D4 => "Faro-Swap",
        ConsoleKey.D5 => "Domain",
        ConsoleKey.D6 => "AutoStaking",
        ConsoleKey.D7 => "Grandline",
        ConsoleKey.D8 => "Brokex",
        ConsoleKey.D9 => "OpenFi",
        ConsoleKey.A => "Bitverse",
        ConsoleKey.F => "FullRun",
        _ => string.Empty
    };
}
