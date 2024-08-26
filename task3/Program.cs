using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace meetingsApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args) {
            const string C_DEFAULT_PATH = "C:/meetings.txt";
            string path, command, text, meetName, meetStr;
            FileInfo fileInfo;
            ConsoleKeyInfo key;
            DateTime dateTimeBegin, dateTimeEnd, dateTimeAlarm;
            int id;
            int idCnt = 1;
            Dictionary<int, List<string>> meetList = new Dictionary<int, List<string>>();
            List<string> argList = null;
            Dictionary<int, string> upd = null;
            CancellationTokenSource cts = null;
            Dictionary<int, CancellationTokenSource> cancelTokens = new Dictionary<int, CancellationTokenSource>(); ;
            //Task task;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========== Приветствуем Вас в приложении для планирования встреч meetingsApp! ==========\n");
            Thread.Sleep(500);
            Console.WriteLine("Для сохранения информации о встречах, Вам необходимо ввести путь к текстовому файлу, после чего нажать ENTER.");
            Console.WriteLine($"Вы можете оставить поле пустым и нажать ENTER для выбора пути к файлу по умолчанию ({C_DEFAULT_PATH}).\n");
            Console.Write("Путь к файлу:\t");
            path = Console.ReadLine();
            if (path == "")
                Console.WriteLine($"\nВы выбрали путь к файлу по умолчанию ({C_DEFAULT_PATH}).");
            path = path == "" ? C_DEFAULT_PATH : path;

            // Цикл для непрерывного ввода
            while (true) {
                try {
                    // Проверка информации о файле
                    fileInfo = new FileInfo(path);
                    if (!fileInfo.Exists)
                        throw new Exception($"\nУказанный Вами файл ({path}) не существует!");
                    if (!FileIO.IsFileInTextFormat(path))
                        throw new Exception($"\nУказанный Вами файл ({path}) не соответсвует формату текстового файла!");
                    //if (!IsTextFileInUTF8(path))
                    //    throw new Exception($"\nУказанный Вами файл ({path}) не соответсвует кодировке UTF-8!");


                    if (idCnt == 1) File.WriteAllText(path, "");
                    Console.WriteLine("Введите '/help' для просмотра списка доступных команд.");
                    command = Console.ReadLine();
                    switch (command) {
                        // Вызов списка доступных комманд
                        case "/help":
                            ConsoleIO.ShowCommands();
                            break;
                        // Просмотр всех существующих встреч
                        case "/all":
                            text = File.ReadAllText(path, Encoding.UTF8);
                            if (text.Length == 0) Console.WriteLine("Встреч еще нет!");
                            else Console.WriteLine(text);
                            break;
                        case "/allbydate":
                            ConsoleIO.ShowMeetingsSortedByValue(meetList, 1);
                            break;
                        case "/allbyname":
                            ConsoleIO.ShowMeetingsSortedByValue(meetList, 0);
                            break;
                        case "/com":
                            ConsoleIO.ShowMeetingsSortedByValue(meetList, 1, false);
                            break;
                        // Создание новой встречи
                        case "/new":
                            dateTimeBegin = ConsoleIO.InputDate("начала");
                            dateTimeEnd = ConsoleIO.InputDate("примерного окончания");
                            while (!meetingsAPI.CheckPeriod(meetList, dateTimeBegin, dateTimeEnd)) {
                                dateTimeBegin = ConsoleIO.InputDate("начала");
                                dateTimeEnd = ConsoleIO.InputDate("примерного окончания");
                            }
                            Console.Write("Введите название встречи:\t");
                            meetName = Console.ReadLine();
                            while (meetName.Trim() == "") {
                                Console.WriteLine("\nОшибка: Вы ввели пустое значение! Пожалуйста, повторите ввод названия встречи.");
                                meetName = Console.ReadLine();
                            }   
                            dateTimeAlarm = ConsoleIO.InputTimeBeforeAlarm(dateTimeBegin);
                            argList = new List<string>();
                            argList.Add(meetName);
                            argList.Add(dateTimeBegin.ToString());
                            argList.Add(dateTimeEnd.ToString());
                            argList.Add(dateTimeAlarm.ToString());
                            meetList.Add(idCnt, argList);
                            meetStr = $"ID = {idCnt} | Название встречи: {meetName} | Начало встречи: {dateTimeBegin} | Окончание встречи: {dateTimeEnd} | Время оповещения о встрече: {dateTimeAlarm}";
                            FileIO.UpdateFile(path, meetList);
                            cts = new CancellationTokenSource();
                            cancelTokens.Add(idCnt, cts);
                            // Запускаем задачу с отложенным сообщением
                            meetingsAPI.TimerAsync(meetStr, (dateTimeAlarm - DateTime.Now).TotalMilliseconds, cts.Token);
                            Console.WriteLine($"\nСоздана новая встреча: {meetStr}");
                            idCnt++;
                           // await task;
                            break;
                        case "/day":
                            dateTimeBegin = ConsoleIO.InputDate();
                            ConsoleIO.GetMeetingsByDate(meetList, dateTimeBegin);
                            break;
                        case "/del":
                            id = ConsoleIO.InputMeetingId(meetList);
                            if (id != -1) {
                                meetingsAPI.DeleteMeetingById(ref meetList, id);
                                FileIO.UpdateFile(path, meetList);
                                cancelTokens[id].Cancel();
                                Console.WriteLine($"\nВстреча ({id}) успешно удалена.");
                            }
                                break;
                        case "/upd":
                            id = ConsoleIO.InputMeetingId(meetList);
                            if (id != -1) {
                                upd = ConsoleIO.InputMeetingParameter(meetList, id);
                                if (upd != null) {
                                    meetingsAPI.UpdateMeetingById(ref meetList, id, upd);
                                    FileIO.UpdateFile(path, meetList);
                                    meetStr = $"ID = {id} | Название встречи: {meetList[id][0]} | Начало встречи: {meetList[id][1]} | Окончание встречи: {meetList[id][2]} | Время оповещения о встрече: {meetList[id][3]}";
                                    cts = new CancellationTokenSource();
                                    cancelTokens[id].Cancel();
                                    cancelTokens.Remove(id);
                                    cancelTokens.Add(id, cts);
                                    meetingsAPI.TimerAsync(meetStr, (DateTime.Parse(meetList[id][3]) - DateTime.Now).Milliseconds, cts.Token);
                                    Console.WriteLine($"\nДанные о встрече ({id}) успешно изменены.");
                                }
                            }   
                            break;
                        case "/exp":
                            dateTimeBegin = ConsoleIO.InputDate();
                            ConsoleIO.GetMeetingsByDate(meetList, dateTimeBegin, path);
                            Console.WriteLine($"\nДанные о встречах за {dateTimeBegin.ToString().Substring(0, 10)} успешно записаны в файл ({path}).");
                            break;
                        case "/path":
                            Console.Write("Введите путь к новому текстовому файлу:\t");
                            path = Console.ReadLine();
                            idCnt = 1;
                            cancelTokens.Clear();
                            break;
                        default:
                            Console.WriteLine("\nОшибка: Введенная Вами команда не распознана! Введите '/help' для просмотра списка доступных команд.");
                            break;
                    }

                }
                // Обработка исключений
                catch (Exception ex) {
                    Console.WriteLine($"\nПроизошла ошибка: {ex.Message}");
                }

                Console.WriteLine("\nНажмите ENTER чтобы продолжить, BACKSPACE для смены активного файла, или ESCAPE для завершения работы программы.");
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter) continue;
                else if (key.Key == ConsoleKey.Escape) break;
                else if (key.Key == ConsoleKey.Backspace) {
                    Console.Write("Введите путь к новому текстовому файлу:\t");
                    path = Console.ReadLine();
                    while (path.Trim() == "") {
                        Console.WriteLine("\nОшибка: Вы ввели пустое значение! Пожалуйста, повторите ввод.");
                        path = Console.ReadLine();
                    }
                    idCnt = 1;
                    cancelTokens.Clear();
                    continue;
                }
            }

            Thread.Sleep(1000);
            Console.WriteLine("\n\n\n========== Выполнение программы завершено. ==========");
            if (argList != null) argList.Clear();
            if (upd != null) upd.Clear();
            if (cts != null) cts.Dispose();
            meetList.Clear();
            cancelTokens.Clear();
            Environment.Exit(0);
        }
    }
}