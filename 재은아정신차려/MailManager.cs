using Telegram.Bot;
using Telegram.Bot.Types;

namespace 재은아정신차려;

using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

public class MailManager
{
    private static string gmailUser = "chahongnw1207@gmail.com";
    private static string gmailPassword = "tudv jlff qnul asqv";
    private static string telegramToken = "7829681305:AAEnjOQp8mSXldy9UyLhRsP3sfz5ktfCRdM";
    private static string chatId = "7415089515";
    static TelegramBotClient bot = new TelegramBotClient(telegramToken);

    public static Dictionary<UniqueId, MailInfo> mails = new();

    public async Task Trigger()
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

            // 제목에 네이버 예약이 있는 메일만
            var subject = message.Subject ?? "";
            if (subject.Contains("[네이버 예약]"))
            {
                var body = message.HtmlBody ?? message.TextBody;
                if (body.Any())
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(body);
                    var text = doc.DocumentNode.InnerText;
                    var mailInfo = new MailInfo(text);
                    mails.Add(uid, mailInfo);
                }
            }
            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
        }
        await client.DisconnectAsync(true);

        // 전체 메일을 돌며 알림 전송
        foreach (var (_, mailInfo) in mails)
        {
            await bot.SendMessage(chatId, mailInfo.FinalMessage);
        }

        // 시간 지난 메일 dic에서 삭제
        
    }
}