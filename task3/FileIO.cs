using System;
using System.Collections.Generic;
using System.IO;

namespace meetingsApp
{
    static class FileIO
    {
        /// <summary>
        /// Функция для проверки, является ли указанный файл текстовым
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Возвращает результат проверки в виде true/false</returns>
        public static bool IsFileInTextFormat(string path)
        {
            return Path.GetExtension(path).ToLower() == ".txt" ? true : false;
        }


        ///// <summary>
        ///// Функция для проверки, соответствует ли указанный текстовый файл кодировке UTF-8
        ///// </summary>
        ///// <param name="path">Путь к файлу</param>
        ///// <returns>Возвращает результат проверки в виде true/false</returns>
        //public static bool IsTextFileInUTF8(string path) {
        //    using (var reader = new StreamReader(path, Encoding.UTF8, true))
        //    {
        //        reader.Peek();
        //        if (reader != null) {
        //            Encoding encoding = reader.CurrentEncoding;
        //            if (encoding != Encoding.UTF8)
        //                return false;
        //        }
        //    }
        //    return true;
        //}


        ///// <summary>
        ///// Функция для записи встречи в текстовый файл
        ///// </summary>
        ///// <param name="path">Путь к текстовому файлу для встреч</param>
        ///// <param name="line">Строка для записи в файл, содержащая информацию о встрече</param>
        //public static void WriteMeetingToFile(string path, string line) {
        //    using (StreamWriter sw = new StreamWriter(path)) {
        //        sw.WriteLine(line);
        //    }
        //}


        /// <summary>
        /// Функция для актуализации информации о встречах внутри файла
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="meetList">Словарь со встречами</param>
        public static void UpdateFile(string path, Dictionary<int, List<string>> meetList)
        {
            if (File.Exists(path))
            {
                List<string> lines = new List<string>();
                foreach (var meet in meetList)
                    lines.Add($"ID = {meet.Key} | Название встречи: {meet.Value[0]} | Начало встречи: {meet.Value[1]} | Окончание встречи: {meet.Value[2]} | Время оповещения о встрече: {meet.Value[3]}");
                lines.Sort();
                File.WriteAllLines(path, lines);
                lines.Clear();
            }
            else Console.WriteLine($"\nОшибка: Указанный Вами файл ({path}) не существует!");
        }
    }
}
