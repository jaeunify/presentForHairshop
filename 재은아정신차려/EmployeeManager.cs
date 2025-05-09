using Newtonsoft.Json;

namespace 재은아정신차려;

public class Employee
{
    public string Name { get; set; }
    public string Team { get; set; }
    public string TelegramToken { get; set; }
    public string ChatId { get; set; }
    public AlramState AlramState { get; set; }
}

public class AlramState
{
    public bool 취소알림 { get; set; }
    public bool 추가알림 { get; set; }
    public bool 입금대기알림 { get; set; }
    public bool 전체알림 { get; set; } // false : 우리팀에게 오는 알림만 받음
    public int 알림범위 { get; set; } // -1: 전체, 0: 당일것만, 1: 1일 후것까지
}

public class EmployeeManager
{
    private Dictionary<string, Employee> employeeDic = new();

    public EmployeeManager()
    {
        var json = File.ReadAllText("명부.txt");
        var employeeList = JsonConvert.DeserializeObject<List<Employee>>(json);

        if (employeeList == null)
            throw new Exception("오빠가 직원명단 작성 잘못함 -_-");

        foreach (var employee in employeeList)
        {
            employeeDic.Add(employee.Name, employee);
        }
    }

    public List<Employee> GetAll()
        => employeeDic.Values.ToList();
}