/*namespace BlazorCleanRelease.Components.Classes
{
    public class Notes
    {
    }
}
*/
/*

Исключение System.IO.InvalidDataException в Microsoft.AspNetCore.SignalR.Core.dll указывает на проблемы с SignalR соединением. Это может происходить, когда передается слишком много данных за один раз или происходит перегрузка памяти. Вот несколько шагов, которые помогут справиться с этим:

1. Пакетная обработка файлов
Как мы уже сделали, обрабатываем файлы партиями.

2. Ограничение размера файла
Убедимся, что файлы не превышают установленный лимит. Вы уже используете maxFileSize, но стоит убедиться, что это правильно обрабатывается.

3. Настройки SignalR
Настройки для SignalR можно изменить, чтобы увеличить ограничения на размер сообщения и таймауты.

4. Обработка больших файлов на стороне сервера
Возможно, стоит пересмотреть подход, при котором большие файлы обрабатываются на стороне сервера с использованием потоков или фоновых задач.

Изменение настроек SignalR
В Startup.cs или где у вас настроен SignalR, можно добавить настройки для увеличения лимитов:

csharp
Копировать код
public void ConfigureServices(IServiceCollection services)
{
    services.AddSignalR().AddHubOptions<YourHub>(options =>
    {
        options.MaximumReceiveMessageSize = 1024 * 1024 * 100; // 100MB
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
        options.HandshakeTimeout = TimeSpan.FromMinutes(5);
    });
    // Другие настройки...
}
Пример кода с учетом всех изменений
Давайте немного изменим ваш код, добавив обработку ошибок, пакетную загрузку файлов и обновление настроек SignalR:

razor
Копировать код
@page "/replicator-ibs"
@using BlazorCleanRelease.Components.Classes;
@using MudBlazor;
@inject IJSRuntime JSRuntime;

<PageTitle>10162 - Replicator IBS</PageTitle>
<h2 style="font-family: 'Arial', sans-serif; color: #333; text-align: center; margin-top: 20px;">Аудио в Interbase</h2>
<br />

<InterbaseConnectionSettings nameConfigFile="@nameConfigFile" Title="&#8594;" />

<LoadFilesFromFolder pathToSaveTempAudio="@pathToSaveTempAudio" onlyAudioType="@onlyAudioType" />
<br />

<div id="startibs" style="visibility: hidden">
    <InterbaseAudioReplication nameConfigFile="@nameConfigFile" pathToSaveTempAudio="@pathToSaveTempAudio" />
</div>

@code {
    private string nameConfigFile = "conf-ibs.ini";
    private string pathToSaveTempAudio = @"C:\temp\2\";
    private bool onlyAudioType = true;
}

@code {
    [Parameter]
    public string pathToSaveTempAudio { get; set; } = "";
    [Parameter]
    public bool onlyAudioType { get; set; } = true;
    [Parameter]
    public MudBlazor.Color colorButton { get; set; } = Color.Primary;

    private List<IBrowserFile> loadedFiles = new();
    private long maxFileSize = 1024 * 1024 * 512; // 512 MB
    private int maxAllowedFiles = 10000;
    private int filesPerBatch = 10; // Количество файлов на одну партию
    private bool isLoading;
    private bool isVisible = false;
    private decimal progressPercent;

    private async void BeforeLoadFiles()
    {
        isVisible = false;
        await JSRuntime.InvokeVoidAsync("hideElement", "startibs");
    }

    private async Task LoadFiles(InputFileChangeEventArgs e)
    {
        isLoading = true;
        isVisible = false;
        loadedFiles.Clear();

        // Предварительная очистка директории
        Operations operationWithFiles = new Operations();
        operationWithFiles.clearFolder(pathToSaveTempAudio);

        progressPercent = 0;

        var files = e.GetMultipleFiles(maxAllowedFiles).Where(file => file.ContentType.Contains("audio")).ToList();
        int totalFiles = files.Count;
        int processedFiles = 0;

        for (int i = 0; i < totalFiles; i += filesPerBatch)
        {
            var batch = files.Skip(i).Take(filesPerBatch).ToList();
            var tasks = batch.Select(async file =>
            {
                try
                {
                    var trustedFileName = file.Name;
                    string filePath = Path.Combine(pathToSaveTempAudio, trustedFileName);
                    Console.WriteLine(filePath);

                    await using FileStream writeStream = new(filePath, FileMode.Create);
                    using var readStream = file.OpenReadStream(maxFileSize);
                    var bytesRead = 0;
                    var totalRead = 0;
                    var buffer = new byte[1024 * 10];

                    while ((bytesRead = await readStream.ReadAsync(buffer)) != 0)
                    {
                        totalRead += bytesRead;
                        await writeStream.WriteAsync(buffer, 0, bytesRead);
                    }

                    lock (loadedFiles)
                    {
                        loadedFiles.Add(file);
                    }

                    Console.WriteLine($"Unsafe Filename: {file.Name} File saved: {trustedFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File: {file.Name} Error: {ex.Message}");
                }
                finally
                {
                    Interlocked.Increment(ref processedFiles);
                    progressPercent = (decimal)processedFiles / totalFiles;
                    await InvokeAsync(StateHasChanged);
                }
            });

            await Task.WhenAll(tasks);
        }

        isLoading = false;
        isVisible = true;

        await JSRuntime.InvokeVoidAsync("toggleElement", "startibs"); // js видимость кнопки пуск
    }
}
Настройка SignalR в Startup.cs
csharp
Копировать код
public void ConfigureServices(IServiceCollection services)
{
    services.AddSignalR().AddHubOptions<YourHub>(options =>
    {
        options.MaximumReceiveMessageSize = 1024 * 1024 * 100; // 100MB
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
        options.HandshakeTimeout = TimeSpan.FromMinutes(5);
    });
    // Другие настройки...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
        endpoints.MapHub<YourHub>("/yourhub");
    });
}
Использование BackgroundService для обработки файлов на сервере
Если проблема продолжает существовать, рассмотрите использование фонового сервиса для обработки больших файлов. Это позволит вам обрабатывать файлы вне основного потока выполнения Blazor Server.

csharp
Копировать код
public class FileProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Логика обработки файлов
        }
    }
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddHostedService<FileProcessingService>();
    // Другие настройки...
}
Эти изменения должны помочь вам избежать перегрузки SignalR и улучшить производительность вашего приложения при обработке большого количества файлов.









 
 
 
*/