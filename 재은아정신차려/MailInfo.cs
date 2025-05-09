using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MailKit;
using MimeKit;

namespace 재은아정신차려;

public class MailInfo
{
    public UniqueId Uid { get; set; }
    public MimeMessage Message { get; set; }
    public DateTime ReservationTime { get; set; }
    public ReservationState ReservationState { get; set; }
    public string FinalMessage { get; set; }
    public HashSet<string> Receivers { get; set; } = new();

    public MailInfo()
    {
    }

    public MailInfo(string text)
    {
        var lines = text.Split(Environment.NewLine).ToList();

        // 예약 상태
        if (text.Contains("예약을 취소"))
            ReservationState = ReservationState.취소;
        else if (text.Contains("새로운 예약이 확정"))
            ReservationState = ReservationState.추가;
        else if (text.Contains("입금대기"))
            ReservationState = ReservationState.입금대기;
        else throw new Exception($"처음 보는 메일이 들어왔습니다! 내용 : {text}");

        // 디자이너 이름 추출
        string designer = Regex.Match(text, @"(\S+)\s*(실장|디자이너)").Groups[1].Value;

        // 이용일시 추출
        string timeText = Regex.Match(text, @"\d{4}\.\d{2}\.\d{2}\.\(.*?\)\s*(오전|오후)\s*\d{1,2}:\d{2}").Value;
        string pattern = @"\d{4}\.\d{2}\.\d{2}\.\(.*?\)\s*(오전|오후)\s*\d{1,2}:\d{2}";
        string cleaned = Regex.Replace(timeText, @"\([^)]*\)", "").Trim(); // → 2025.05.09. 오전 11:30
        ReservationTime = DateTime.ParseExact(cleaned, "yyyy.MM.dd. tt h:mm", CultureInfo.GetCultureInfo("ko-KR"));

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

        FinalMessage = $"{designer} / {timeText} {string.Join(" ", menuList)} {ReservationState.ToString()} 되었습니다.";
    }
        
    public bool CheckNeedAlarm(Employee employee)
    {
        switch (ReservationState)
        {
            case ReservationState.입금대기:
                if (employee.AlramState.입금대기알림 == false)
                    return false;
                break;
            case ReservationState.추가:
                if (employee.AlramState.추가알림 == false)
                    return false;
                break;
            case ReservationState.취소:
                if (employee.AlramState.취소알림 == false)
                    return false;
                break;
        }

        return true;
    }
}

public enum ReservationState
{
    취소,
    추가,
    입금대기
}