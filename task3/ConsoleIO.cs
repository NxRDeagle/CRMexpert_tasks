using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace meetingsApp
{
    public static class ConsoleIO
    {
        /// <summary>
        /// Функция для ввода даты и времени начала/окончания встречи с внутренней проверкой на корректность данных
        /// </summary>
        /// <param name="addStr">Добавочная строка для описания даты встречи</param>
        /// <returns>Возвращает корректно введенное значение даты и времени (DateTime) в формате 'ДД ММ ГГГГ ЧЧ:ММ:СС'</returns>
        public static DateTime InputDate(string addStr = "")
        {
            string date;
            bool check;
            DateTime dt = DateTime.Today;
            do
            {
                if (addStr != "")
                {
                    Console.WriteLine($"Пожалуйста, введите дату и время {addStr} встречи в формате 'ДД ММ ГГГГ ЧЧ:ММ:СС'");
                    Console.Write("(Ввод секунд не обязателен, ввод без времени будет автоматически заменен на 00:00:00)\t");
                }
                else Console.WriteLine($"Пожалуйста, введите дату встречи в формате 'ДД ММ ГГГГ'");
                date = Console.ReadLine();
                if (date.Trim() != "")
                {
                    try
                    {
                        dt = DateTime.Parse(date);
                        if (int.Parse(date.Split(' ')[1]) == 2 && int.Parse(date.Split(' ')[0]) == 29)
                        {
                            Console.WriteLine("\nОшибка: В нашем учете в феврале только 28 дней!");
                            check = false;
                        }
                        else return dt;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nОшибка: Дата введена некорректно!");
                        check = false;
                    }
                }
                else
                {
                    Console.WriteLine("\nОшибка: Вы ввели пустое значение!");
                    check = false;
                }
            } while (!check);
            return dt;
        }


        /// <summary>
        /// Функция для ввода времени, за которое необходимо оповестить о начале встречи с внутренней проверкой на корректность данных
        /// </summary>
        /// <param name="dateTimeBegin">Время начала встречи</param>
        /// <returns>Возвращает корректно введенное значение даты и времени (DateTime) в формате 'ДД ММ ГГГГ ЧЧ:ММ:СС', в которое необходимо оповестить пользователя о предстоящей встрече</returns>
        public static DateTime InputTimeBeforeAlarm(DateTime dateTimeBegin)
        {
            string time;
            string dateBegin = dateTimeBegin.ToString().Substring(0, 11);
            bool check;
            DateTime dt;
            DateTime res = dateTimeBegin;
            do
            {
                Console.WriteLine($"Пожалуйста, введите время, за которое необходимо оповестить Вас о начале встречи в формате 'ЧЧ:ММ:СС'");
                Console.Write("(Ввод секунд не обязателен)\t");
                time = Console.ReadLine();
                if (time.Trim() != "")
                {
                    try
                    {
                        time = dateBegin + time;
                        dt = Convert.ToDateTime(time);
                        res = res.Add(new TimeSpan(-dt.Hour, -dt.Minute, -dt.Second));
                        if (res < DateTime.Now)
                        {
                            Console.WriteLine($"\nОшибка: Введеное Вами время до оповещения ({res}) уже наступило!");
                            check = false;
                        }
                        else check = true;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nОшибка: Время встречи введено некорректно!");
                        check = false;
                    }
                }
                else
                {
                    Console.WriteLine("\nОшибка: Вы ввели пустое значение!");
                    check = false;
                }
            } while (!check);
            return res;
        }


        /// <summary>
        /// Функция для ввода ID встречи, с которой необходимо произвести действие
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <returns>Возвращает корректно введеный ID встречи, или -1 в случае отмены ввода</returns>
        public static int InputMeetingId(Dictionary<int, List<string>> meetList)
        {
            string input;
            bool check;
            int id = -1;
            do
            {
                Console.Write($"Пожалуйста, введите ID встречи:\t");
                input = Console.ReadLine();
                if (input == "cancel") return -1;
                if (input.Trim() != "")
                {
                    try
                    {
                        id = int.Parse(input);
                        if (!meetList.ContainsKey(id))
                        {
                            Console.WriteLine($"\nОшибка: Указанный Вами ID встречи '{id}' не найден в списке встреч!");
                            Console.WriteLine("Пожалуйста, повторите попытку, или введите 'cancel' для отмены действия.");
                            check = false;
                        }
                        else
                        {
                            if (!meetList.ContainsKey(id))
                            {
                                Console.WriteLine($"\nОшибка: Указанная Вами встреча({id}) не существует!");
                                check = false;
                            }
                            else return id;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nОшибка: ID встречи введен некорректно!");
                        check = false;
                    }
                }
                else
                {
                    Console.WriteLine("\nОшибка: Вы ввели пустое значение!");
                    check = false;
                }
            } while (!check);
            return id;
        }


        /// <summary>
        /// Функция для ввода параметра встречи, для получения данных о параметре, который необхожимо обновить
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <param name="id">ID встречи</param>
        /// <returns>Возвращает индекс значения параметра для изменения (0 - meetName, 1 - dateTimeBegin, 2 - dateTimeEnd, 3 - dateTimeAlarm)</returns>
        public static Dictionary<int, string> InputMeetingParameter(Dictionary<int, List<string>> meetList, int id)
        {
            string input, parameterValue;
            bool check;
            DateTime dt;
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<string> val;
            do
            {
                Console.WriteLine($"Пожалуйста, введите параметр, который необходимо изменить, или 'cancel' для отмены ввода.");
                Console.WriteLine("Введите 'name' для изменения наименования встречи;");
                Console.WriteLine("Введите 'start' для изменения даты и времени начала встречи;");
                Console.WriteLine("Введите 'end' для изменения даты и времени окончания встречи;");
                Console.WriteLine("Введите 'alarm' для изменения времени напоминания о встрече.");
                input = Console.ReadLine();
                if (input == "cancel") return null;
                if (input.Trim() != "")
                {
                    try
                    {
                        meetList.TryGetValue(id, out val);
                        switch (input)
                        {
                            case "name":
                                Console.Write("Введите название встречи:\t");
                                parameterValue = Console.ReadLine();
                                while (parameterValue.Trim() == "")
                                {
                                    Console.WriteLine("\nОшибка: Вы ввели пустое значение! Пожалуйста, повторите ввод названия встречи.");
                                    parameterValue = Console.ReadLine();
                                }
                                dict.Add(0, parameterValue);
                                return dict;
                            case "start":
                                dt = InputDate("начала");
                                while (!meetingsAPI.CheckPeriod(meetList, dt, DateTime.Parse(val[2]), id))
                                    dt = InputDate("начала");
                                dict.Add(1, dt.ToString());
                                return dict;
                            case "end":
                                dt = InputDate("примерного окончания");
                                while (!meetingsAPI.CheckPeriod(meetList, DateTime.Parse(val[1]), dt, id))
                                    dt = InputDate("примерного окончания");
                                dict.Add(2, dt.ToString());
                                return dict;
                            case "alarm":
                                dt = InputTimeBeforeAlarm(DateTime.Parse(val[1]));
                                dict.Add(3, dt.ToString());
                                return dict;
                            default:
                                Console.WriteLine("\nОшибка: Введенное Вами значение не было распознано!");
                                check = false;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nОшибка: параметр для изменения встречи введен некорректно!");
                        check = false;
                    }
                }
                else
                {
                    Console.WriteLine("\nОшибка: Вы ввели пустое значение!");
                    check = false;
                }
            } while (!check);
            return null;
        }


        /// <summary>
        /// Функция для вывода в консоль списка доступных команд
        /// </summary>
        public static void ShowCommands()
        {
            Console.WriteLine("Введите '/help' для просмотра списка доступных комманд;");
            Console.WriteLine("Введите /all, для просмотра информации о всех встречах;");
            Console.WriteLine("Введите /allbyname, для просмотра информации о всех встречах, отсортированных по имени;");
            Console.WriteLine("Введите /allbydate, для просмотра информации о всех встречах, отсортированных по датам;");
            Console.WriteLine("Введите /com, для просмотра информации о всех предстоящих встречах;");
            Console.WriteLine("Введите /day, чтобы посмотреть информацию о встречах за указанный день;");
            Console.WriteLine("Введите /new, чтобы запланировать новую встречу;");
            Console.WriteLine("Введите /upd, чтобы изменить информацию о существующей встрече;");
            Console.WriteLine("Введите /del, чтобы удалить существующую встречу;");
            Console.WriteLine("Введите /exp, для экспорта информации о встречах за указанный день в текстовый файл;");
            Console.WriteLine("Введите /path, для изменения пути к ативному текстовому файлу.");
        }


        /// <summary>
        /// Функция для вывода информации о встречах в консоль, с сортировкой по значению
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <param name="valueIdx">Индекс значения (0 - meetName, 1 - dateTimeBegin, 2 - dateTimeEnd, 3 - dateTimeAlarm)</param>
        /// <param name="showOnlyUpcoming">Отображать только предстоящие встречи (false по умолчанию)</param>
        public static void ShowMeetingsSortedByValue(Dictionary<int, List<string>> meetList, int valueIdx, bool showOnlyUpcoming = false)
        {
            var sortedDict = from entry in meetList orderby entry.Value[valueIdx] ascending select entry;
            foreach (var element in sortedDict)
                if ((!showOnlyUpcoming) || (showOnlyUpcoming && (DateTime.Parse(element.Value[valueIdx]) > DateTime.Now)))
                    Console.WriteLine($"ID = {element.Key} | Название встречи: {element.Value[0]} | Начало встречи: {element.Value[1]} | Окончание встречи: {element.Value[2]} | Время оповещения о встрече: {element.Value[3]}");
        }


        /// <summary>
        /// Функция для вывода информации о встречах за указанную дату в консоль
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <param name="date">Дата, за которую необходимо отобразить встречи</param>
        /// <param name="filePathToWrite">Путь к файлу, в который нужно записать (или "экспортировать") значения. По умолчанию - пустая строка, не записываем в файл</param>
        public static void GetMeetingsByDate(Dictionary<int, List<string>> meetList, DateTime date, string filePathToWrite = "")
        {
            int cnt = 0;
            var sortedDict = from entry in meetList orderby entry.Value[1] ascending select entry;
            List<string> lst = null;
            string str;
            if (filePathToWrite != "") lst = new List<string>();
            foreach (var element in sortedDict)
                if (element.Value[1].Substring(0, 10) == date.ToString().Substring(0, 10))
                {
                    str = $"ID = {element.Key} | Название встречи: {element.Value[0]} | Начало встречи: {element.Value[1]} | Окончание встречи: {element.Value[2]} | Время оповещения о встрече: {element.Value[3]}";
                    if (filePathToWrite != "") lst.Add(str);
                    Console.WriteLine(str);
                    cnt++;
                }
            if (cnt == 0) Console.WriteLine($"\nЗа указанную Вами дату ({date}) не найдено ни одной встречи!");
            if (filePathToWrite != "")
            {
                File.WriteAllLines(filePathToWrite, lst);
                lst.Clear();
            }
        }
    }
}
