using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lab12_New
{
    internal class Program
    {
        private static String getVernamCode(String input)
        {
            string result = "";
            string key = "1831";

            int i = 0;
            while (i < input.Length)
            {
                for (int j = 0; j < 4; j++)
                {
                    result += input[i] ^ key[j];
                    i++;
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            Console.Write("Введите дату рождения (ddMMyyyy): ");

            string input = Console.ReadLine();

            if (!Regex.IsMatch(input, "^(0?[1-9]|[12][0-9]|3[01])(0?[1-9]|1[012])(19([0-9]{2})|20([0-9]){2})$"))
            {
                throw new ArgumentException("Input string isnt correct");
            }

            string vernamCode = getVernamCode(input);
            Console.WriteLine($"Код Вернама: {vernamCode}");


            //First part
            List<String> parallels = new List<String>();     //Разбиваем входную строку по два символа
            for (int i = 0; i < vernamCode.Length; i += 2)
            {
                parallels.Add(vernamCode.Substring(i, 2));
            }

            Console.WriteLine($"\nГруппы: {String.Join(" ",parallels)}");

            List<String> parallelsAsBits = new List<String>(); //Добавляем к каждой группе контрольные символы
            foreach (string str in parallels)
            {
                parallelsAsBits.Add(getStringWithControlBits(str));
            }

            Console.WriteLine($"\nБитовое представление групп c контрольными битами:\n{String.Join("\n", parallelsAsBits)}");

            List<String> paralellsWithCorrectControlBits = new List<String>(); //Корректируем контрольные биты в зависимости от основной информации
            foreach (string str in parallelsAsBits)
            {
                paralellsWithCorrectControlBits.Add(calculateControlBits(str));
            }
            Console.WriteLine($"\nБитовое представление групп c корректными контрольными битами:\n{String.Join("\n", paralellsWithCorrectControlBits)}");



            //Second part
            StringBuilder sb = new StringBuilder(paralellsWithCorrectControlBits[0]);
            Console.WriteLine($"\nПервая группа исходный код: {paralellsWithCorrectControlBits[0]}");
            sb[10] = sb[10] == '1' ? '0' : '1';
            Console.WriteLine($"Первая группа код c ошибкой: {sb}");
            List<string> paralellsWithIncorrectBits = new List<string>   //Меняем бит у первой группы 10-го индекса
            {
                sb.ToString(),
            };

            for (int i = 1; i < paralellsWithCorrectControlBits.Count; i++)  //Дописываем остальные группы
            {
                paralellsWithIncorrectBits.Add(paralellsWithCorrectControlBits[i]);
            }


            List<String> paralellsWithIncorrectControlBits = new List<String>(); //Заново считаем контрольные биты
            foreach (string str in paralellsWithIncorrectBits)
            {
                paralellsWithIncorrectControlBits.Add(calculateControlBits(str));
            }


            List<int> indexesOfIncorrectBites = getIndexsOfIncorrectBites(paralellsWithCorrectControlBits, paralellsWithIncorrectControlBits); // Находим некорректные контрольные индексы
            int indexWhereIncorrectBit = indexesOfIncorrectBites.Sum(x => x) - 1; // получаем индекс нарушения
            Console.WriteLine($"\nНеккоректные контрольные биты: {String.Join(" ", indexesOfIncorrectBites)}");

            sb = new StringBuilder(paralellsWithIncorrectControlBits[0]);
            Console.WriteLine($"\nПервая группа неккоретный исходный код: {paralellsWithIncorrectControlBits[0]}");
            sb[indexWhereIncorrectBit] = sb[indexWhereIncorrectBit] == '0' ? '1' : '0'; // инвертируем бит
            Console.WriteLine($"Первая группа корректный код: {sb}");
            List<String> correctListWithControlBits = new List<string> //Изменяем первую группу
            {
                sb.ToString()
            };

            for (int i = 1; i < paralellsWithIncorrectControlBits.Count; i++) //Дописываем остальные группы
            {
                correctListWithControlBits.Add(paralellsWithIncorrectControlBits[i]);
            }



            List<String> correctList = getListWithoutControlBits(correctListWithControlBits); //Избавляемся от контрольных битов
            Console.WriteLine($"\nИсходный код символов:\n{string.Join("\n", correctList)}");
            String result = convertBitsToTheSymbol(correctList); // Декодируем из 2сс в строку
            Console.WriteLine($"\nЗакодированное сообщение: {result}\n");


            string decrypted = getVernamCode(result);

            Console.WriteLine($"Исходное сообщение: {decrypted}\n");

        }

        private static String convertBitsToTheSymbol(List<String> correctList)
        {
            string result = "";

            foreach (String str in correctList)
            {
                for (int i = 0; i < str.Length; i += 8)
                {
                    string tempString = str.Substring(i, 8);
                    result += Convert.ToChar(Convert.ToInt32(tempString, 2));
                }
            }

            return result;
        }

        private static List<String> getListWithoutControlBits(List<String> correctListWithControlBits)
        {
            List<String> correctList = new List<String>();
            Console.WriteLine("\nПомечаем контрольные биты звездочкой и избавляемся от них");
            for (int i = 0; i < correctListWithControlBits.Count; i++)
            {
                StringBuilder sb = new StringBuilder(correctListWithControlBits[i]);
                for (int j = 1; j < correctListWithControlBits[i].Length; j *= 2)
                {
                    if (sb[j - 1] == '0')
                    {
                        sb.Replace('0', '*', j - 1, 1);
                    }
                    else
                    {
                        sb.Replace('1', '*', j - 1, 1);
                    }

                }
                Console.WriteLine(sb.ToString());
                sb.Replace("*", "");
                correctList.Add(sb.ToString());
            }

            return correctList;
        }

        private static List<int> getIndexsOfIncorrectBites(List<String> paralellsWithCorrectControlBits, List<String> paralellsWithIncorrectControlBits)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < paralellsWithCorrectControlBits.Count; i++)
            {
                for (int j = 1; j < paralellsWithCorrectControlBits[i].Length; j *= 2)
                {
                    if (paralellsWithCorrectControlBits[i][j - 1] != paralellsWithIncorrectControlBits[i][j - 1])
                    {
                        result.Add(j);
                    }
                }
            }

            return result;
        }

        private static string calculateControlBits(String input)
        {
            int power = 0;
            int indexControlBit = (int)Math.Pow(2, power) - 1;

            StringBuilder sb = new StringBuilder(input);

            while (indexControlBit < input.Length)
            {
                if (getCountOfUnits(input, indexControlBit) % 2 == 0)
                {
                    sb.Replace('1', '0', indexControlBit, 1);
                }
                else
                {
                    sb.Replace('0', '1', indexControlBit, 1);
                }

                power++;
                indexControlBit = (int)Math.Pow(2, power) - 1;
            }

            return sb.ToString();
        }

        private static int getCountOfUnits(String input, int indexControlBit)
        {
            int step = indexControlBit + 1;
            int unitsCounter = 0;

            for (int i = indexControlBit; i < input.Length; i += step * 2)
            {
                string tempString;

                if (i + step > input.Length && i != indexControlBit)
                {
                    tempString = input.Substring(i);
                }
                else if (i == indexControlBit)
                {
                    if (i + step > input.Length)
                    {
                        tempString = input.Substring(i + 1);
                    }
                    else
                    {
                        tempString = input.Substring(i + 1, step - 1);
                    }

                    for (int j = i + 1; j < tempString.Length + i + 1; j++)
                    {
                        if (input[j] == '1')
                        {
                            unitsCounter++;
                        }
                    }

                    continue;
                }
                else
                {
                    tempString = input.Substring(i, step);
                }

                for (int j = i; j < tempString.Length + i; j++)
                {
                    if (input[j] == '1')
                    {
                        unitsCounter++;
                    }
                }
            }

            return unitsCounter;
        }

        private static string getStringWithControlBits(String input)
        {
            string str = "";

            for (int i = 0; i < input.Length; i++)
                str += Convert.ToString(input[i], 2).PadLeft(8, '0');

            StringBuilder stringBuilder = new StringBuilder(str);

            int power = 0, indexControlBit = (int)Math.Pow(2, power) - 1;

            while (indexControlBit < str.Length)
            {
                stringBuilder.Insert(indexControlBit, '0');
                power++;
                indexControlBit = (int)Math.Pow(2, power) - 1;
            }

            return stringBuilder.ToString();
        }
    }
}
