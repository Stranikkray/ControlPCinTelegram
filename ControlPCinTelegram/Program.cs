using ControlPCinTelegram;
using Microsoft.Win32;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;

#region App_Parameters
Settings settings = new Settings();
string versApp = "0.2";

bool codeSend = false;
int code = -1;
string ApiBot;
#endregion

#region TelegramBot_Settings
TelegramBotClient botClient = null;

if (settings.tokenBot==null || settings.chatIdClient==-1)
{
RestartConnectToApi:
    Console.Clear();
    Console.Write("ControlPCinTelegram vers:" + versApp);
    Console.Write("\nThe telegram bot API is not specified.\nSpecify it now:");

    Console.BackgroundColor = ConsoleColor.White;
    Console.ForegroundColor = ConsoleColor.Black;        
    ApiBot = Console.ReadLine();
    Console.ResetColor();

    if (ApiBot != null)
    {
        botClient = new TelegramBotClient(ApiBot);
        botClient.StartReceiving(СonfirmationOfTheUserChatId, Error);
        
        Random rnd = new Random();
        code = rnd.Next(100000, 999999);        

        Console.WriteLine("Confirmation code: " + code);

        if (!await TimerForConfirmationChatId())
        {
            Console.Clear();
            goto RestartConnectToApi;
        }
    }
    else
    {
        Console.Write("\n");
        goto RestartConnectToApi;
    }
    botClient.StartReceiving(Updating, Error);
    Console.ReadLine();
}

async Task<bool> TimerForConfirmationChatId()
{
    int seconds = 60 * 5;
    while (seconds>0)
    {
        await Task.Delay(1000);
        seconds--;
        Console.Write("\r{0}:{1:00} ", seconds/60, seconds%60);
        if (settings.chatIdClient!=-1 && !string.IsNullOrEmpty(settings.tokenBot))
            return true;        
    }    
    return false;
}
#endregion

#region Main
if (!settings.isFirstSettings)
{
    if (settings.isAutoStart)
        AddAutoStart();
    if (settings.isBlockTaskManager)
        BlockTaskManager();    
}
var handle = NativeMethods.GetConsoleWindow();
NativeMethods.ShowWindow(handle, NativeMethods.SW_HIDE);

await botClient.SendTextMessageAsync(settings.chatIdClient, $"I am back\nVersApp: {versApp}"); // send message about start system
Console.ReadLine();
#endregion

#region MessageProcessing
async Task Updating(ITelegramBotClient client, Update update, CancellationToken token)
{
    var message = update.Message;
    if (settings.chatIdClient == message.Chat.Id)
    {
        if (message.Text != null)
        {
            switch (message.Text)
            {
                case @"\vers App":
                    await client.SendTextMessageAsync(message.Chat.Id, $"VersApp: {versApp}");
                    break;
                case @"\stop work PC":
                    await client.SendTextMessageAsync(message.Chat.Id, "I’ll be back");
                    Process.Start("shutdown", "/s /t 0");
                    break;
                case @"\check autostart":
                    await client.SendTextMessageAsync(message.Chat.Id, "Автозапуск:" + settings.isAutoStart);
                    break;
                case @"\block Task Manager":
                    if (!settings.isBlockTaskManager)
                    {
                        settings.isBlockTaskManager = true;
                        BlockTaskManager();
                    }
                    break;
                case @"\unblock Task Manager":
                    settings.isBlockTaskManager = false;
                    break;
                case @"\list processes":
                    string s = "";
                    foreach (var item in ProcessesOfRunningApplications())
                    {
                        s = s + "\n" + item.ProcessName;
                    }
                    await client.SendTextMessageAsync(message.Chat.Id, "Запущенные процессы:" + s);
                    break;                    
                default:
                    break;
            }
        }
    }
    else await client.SendTextMessageAsync(message.Chat.Id, $"You chat Id: {message.Chat.Id}");    
}
async Task СonfirmationOfTheUserChatId(ITelegramBotClient client, Update update, CancellationToken token)
{
    var message = update.Message;

    if (Convert.ToInt32(message.Text) == code)
    {
        settings.chatIdClient = (int)message.Chat.Id;
        settings.tokenBot = ApiBot;
        await client.SendTextMessageAsync(message.Chat.Id, "The code is correct!");
    }
    else
        await client.SendTextMessageAsync(message.Chat.Id, "The code is not correct");
}
Task Error(ITelegramBotClient client, Exception exception, CancellationToken token) => client.SendTextMessageAsync(settings.chatIdClient, exception.ToString());
#endregion

#region WorkWithProcesses
List<Process> ProcessesOfRunningApplications() => Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero).ToList();
#endregion

#region BlockingProgramShutdown
async void BlockTaskManager()
{
    while (settings.isBlockTaskManager)
    {
        Process[] taskManagers = Process.GetProcessesByName("Taskmgr");

        if (Process.GetProcessesByName("Taskmgr").Length != 0)
        {            
            foreach (var item in taskManagers)
            {
                item.Kill();
            }
            await Task.Delay(200);
        }
    }    
}
#endregion

#region AutoStart
async void AddAutoStart() // Добавление программы в автозапуск 
{
    // Получаем имя исполняемого файла без пути
    string appName = Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName);

    // Получаем ключ реестра для автозапуска текущего пользователя
    RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

    try
    {
        // Добавляем запись в автозапуск
        registryKey.SetValue(appName, System.Reflection.Assembly.GetExecutingAssembly().Location);
        settings.isAutoStart = true;
    }
    catch (Exception ex)
    {
        settings.isAutoStart = false;
        await botClient.SendTextMessageAsync(settings.chatIdClient, "Error add autostart");
    }
    finally
    {
        registryKey.Close();
    }
}
#endregion

#region HidingWorkProgram
class NativeMethods
{
    public static int SW_HIDE = 0;
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();
}
#endregion