using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MailKit;
using MimeKit;

namespace 재은아정신차려;

public class MailInfo
{
    public UniqueId Uid { get; set; }
    public MimeMessage Message { get; set; }
    public ReservationState ReservationState { get; set; }
    public string FinalMessage { get; set; }

    public MailInfo()
    {
    }

    public MailInfo(string text)
    {
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
        var menuLine = lines.First(x => x.Contains("예약금"));
        var menuMatches = Regex.Matches(menuLine, @"(\S+)\s*예약금");
        var menuList = new List<string>();
        var menuMap = new Dictionary<string, string>
        {
            { "컷", "C" }, { "펌", "P" }, { "컬러", "CL" }, { "클리닉", "TM" }, { "이미지 헤어 컨설팅", "컨설팅" }
        };
        foreach (Match match in menuMatches)
            menuList.Add(menuMap.TryGetValue(match.Groups[1].Value, out var m) ? m : match.Groups[1].Value);

        FinalMessage = $"{designer} / {time} {string.Join(" ", menuList)} {status} 되었습니다.";
    }
}
public enum ReservationState
{

    취소,
    추가,
    입금대기
}