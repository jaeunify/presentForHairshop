using 재은아정신차려;
using 재은아정신차려.Components;
using Telegram.Bot;

class Program
{
    static async Task Main(string[] args)
    {
        var mailManager = new MailManager();
        
        // 메일 작업 돌리기
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await mailManager.Trigger();
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

}