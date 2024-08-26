using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace meetingsApp
{
    static class meetingsAPI
    {
        /// <summary>
        /// Функция для проверка данных введеной встречи на предмет пересечения с другими встречами
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <param name="dateTimeBegin">Время начала встречи</param>
        /// <param name="dateTimeEnd">Время окончания встречи</param>
        /// <param name="id">ID встречи, проверка для которой будет проигнорирована (для обновления информации, по умолч. -1)</param>
        /// <returns>Возвращает результат проверки в виде true/false</returns>
        public static bool CheckPeriod(Dictionary<int, List<string>> meetList, DateTime dateTimeBegin, DateTime dateTimeEnd, int id = -1)
        {
            if (DateTime.Compare(dateTimeBegin, dateTimeEnd) > 0)
            {
                Console.WriteLine("\nОшибка: Указанное Вами время начала встречи позднее, чем время конца встречи. Пожалуйста, повторите попытку ввода!");
                return false;
            }
            if (dateTimeBegin < DateTime.Now || dateTimeEnd < DateTime.Now)
            {
                Console.WriteLine($"\nОшибка: Время для указанной Вами встречи ({dateTimeBegin} - {dateTimeEnd}) определено некорректно!");
                return false;
            }
            if (meetList.Count == 0)
                return true;
            bool check = true;
            foreach (var meet in meetList)
            {
                if (((dateTimeBegin < DateTime.Parse(meet.Value[1]) && dateTimeEnd > DateTime.Parse(meet.Value[1]))
                        || (dateTimeBegin > DateTime.Parse(meet.Value[1]) && dateTimeEnd < DateTime.Parse(meet.Value[2]))
                        || (dateTimeBegin < DateTime.Parse(meet.Value[2]) && dateTimeEnd > DateTime.Parse(meet.Value[2]))) && meet.Key != id)
                {
                    check = false;
                    Console.WriteLine($"\nОшибка: Указанная Вами встреча ({dateTimeBegin} - {dateTimeEnd}) пересекается со встречей '{meet.Value[0]}' ({meet.Value[1]} - {meet.Value[2]}, ID = {meet.Key})");
                }
            }
            return check;
        }


        /// <summary>
        /// Функция для обновления информации о встрече по ID
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <param name="id">ID встречи</param>
        /// <param name="upd">Словарь с ключом и значением э-та, который необходимо обновить(0 - meetName, 1 - dateTimeBegin, 2 - dateTimeEnd, 3 - dateTimeAlarm)</param>
        public static void UpdateMeetingById(ref Dictionary<int, List<string>> meetList, int id, Dictionary<int, string> upd)
        {
            List<string> lst;
            if (meetList.TryGetValue(id, out lst))
            {
                lst[upd.First().Key] = upd.First().Value;
                meetList[id] = lst;
            }
        }


        /// <summary>
        /// Функция для удаления встречи по ID
        /// </summary>
        /// <param name="meetList">Словарь со встречами</param>
        /// <param name="id">ID встречи</param>
        public static void DeleteMeetingById(ref Dictionary<int, List<string>> meetList, int id)
        {
            if (!meetList.ContainsKey(id))
                Console.WriteLine($"\nОшибка: Указанная Вами встреча({id}) не существует!");
            else meetList.Remove(id);
        }


        /// <summary>
        /// Асинхронный метод вызова функции с задержкой
        /// </summary>
        /// <param name="meetStr">Строка с информацией о встрече</param>
        /// <param name="ms">Время задержки в мс</param>
        /// <param name="token">Токен отмены задачи</param>
        public static async void TimerAsync(string meetStr, double ms, CancellationToken token)
        {
            try
            {
                Thread t = new Thread(() => {
                    Thread.Sleep((int)ms);
                    Console.WriteLine($"========== Уведомление: У Вас запланирована встреча! ==========\n{meetStr}");
                });
                t.IsBackground = true;
                t.Start();
                await Task.Delay(20, token);

            }
            catch (TaskCanceledException)
            {
                // Обрабатываем отмену задачи
                throw;
            }
        }
    }
}
