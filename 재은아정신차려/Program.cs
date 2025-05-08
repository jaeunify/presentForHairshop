using System.Text.RegularExpressions;
using Radzen;
using 재은아정신차려;
using 재은아정신차려.Components;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Telegram.Bot;
using MimeKit;


class Program
{
    private static string gmailUser = "chahongnw1207@gmail.com";
    private static string gmailPassword = "tudv jlff qnul asqv";
    private static string telegramToken = "7829681305:AAEnjOQp8mSXldy9UyLhRsP3sfz5ktfCRdM";
    private static string chatId = "7415089515";

    static TelegramBotClient bot = new TelegramBotClient(telegramToken);
 
    static async Task Main(string[] args)
    {
        // 메일 작업 돌리기
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await CheckMailAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"오류 발생: {ex.Message}");
                }

                await Task.Delay(10000); // 10초 대기
            }
        });
        
        // 프론트 구축
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        
        app.UseAntiforgery();

        // app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }

    static async Task CheckMailAsync()
    {
        using var client = new ImapClient();
        await client.ConnectAsync("imap.gmail.com", 993, true);
        await client.AuthenticateAsync(gmailUser, gmailPassword);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite);

        // 아직읽지 않은 메일을 읽어 옴
        var messageIds = await inbox.SearchAsync(SearchQuery.NotSeen.And(SearchQuery.FromContains("naverbooking_noreply@navercorp.com")));

        foreach (var uid in messageIds)
        {
            var message = await inbox.GetMessageAsync(uid);
            var subject = message.Subject ?? "";

            // 제목에 네이버 예약이 있는 메일만
            if (subject.Contains("[네이버 예약]"))
            {
                var body = message.HtmlBody ?? message.TextBody;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(body);
                    var text = doc.DocumentNode.InnerText;
                    var lines = text.Split(Environment.NewLine).ToList();

                    // 예약 상태
                    string status = null;
                    if (text.Contains("예약을 취소")) status = "취소";
                    else if (text.Contains("새로운 예약이 확정")) status = "추가";
                    else if (text.Contains("입금대기")) status = "입금대기";

                    // 디자이너 이름 추출
                    string designer = Regex.Match(text, @"(\S+)\s*(실장|디자이너)").Groups[1].Value;

                    // 이용일시 추출
                    string time = Regex.Match(text, @"\d{4}\.\d{2}\.\d{2}\.\(.*?\)\s*(오전|오후)\s*\d{1,2}:\d{2}").Value;

                    // 예약메뉴 추출 + 변환
                    var menuLine = lines.First(x=>x.Contains("예약금"));
                    var menuMatches = Regex.Matches(menuLine, @"(\S+)\s*예약금");
                    var menuList = new List<string>();
                    var menuMap = new Dictionary<string, string>
                    {
                        { "컷", "C" }, { "펌", "P" }, { "컬러", "CL" }, { "클리닉", "TM" }, { "이미지 헤어 컨설팅", "컨설팅" }
                    };
                    foreach (Match match in menuMatches)
                        menuList.Add(menuMap.TryGetValue(match.Groups[1].Value, out var m) ? m : match.Groups[1].Value);

                    string finalMsg = $"{designer} / {time} {string.Join(" ", menuList)} {status} 되었습니다.";
                    await bot.SendMessage(chatId, finalMsg);
                }

                await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
            }
        }

        await client.DisconnectAsync(true);
    }
}