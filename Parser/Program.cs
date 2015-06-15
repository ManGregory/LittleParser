using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Parser
{
        //Класс, выполняющий лексический анализ
    public sealed class Scanner
    {
        //результат анализа (список лексем)
        private readonly Dictionary<string, int> _result;

        /// <summary>
        /// Конструктор, использующий поток символов
        /// </summary>
        /// <param name="input">Поток символов с последовательным доступом</param>
        public Scanner(TextReader input)
        {
            _result = new Dictionary<string, int>();
            Scan(input);
        }

        /// <summary>
        /// Список лексем
        /// </summary>
        public Dictionary<string, int> Tokens
        {
            get { return _result; }
        }

        /// <summary>
        /// Метод, выполняющий лексический анализ строки
        /// </summary>
        /// <param name="input">Поток символов с последовательным доступом</param>
        private void Scan(TextReader input)
        {
            //пока во входном потке есть символы
            while (input.Peek() != -1)
            {
                //получаем текущий символ
                var ch = (char) input.Peek();

                //Обработка незначащих символов
                if (char.IsWhiteSpace(ch) || (ch == ',') || (ch == ':') || (ch == ';'))
                {
                    // Продолжаем считывать символы пропуская текущий
                    input.Read();
                }
                else if (char.IsLetter(ch) || ch == '_')
                {
                    // Ключевое слово или идентификатор
                    var accum = new StringBuilder();
                    while (char.IsLetter(ch) || ch == '_')
                    {
                        //добавляем символ в аккумулятор
                        accum.Append(ch);
                        //считываем следующий символ
                        input.Read();
                        if (input.Peek() == -1)
                        {
                            break;
                        }
                        ch = (char) input.Peek();
                    }
                    //добавляем лексему в список
                    AddLexem(accum.ToString());
                }
                else if (ch == '\'')
                {
                    // строковый литерал
                    var accum = new StringBuilder();

                    input.Read();

                    if (input.Peek() == -1)
                    {
                        throw new Exception("незавершенный строковый литерал");
                    }

                    while ((ch = (char) input.Peek()) != '\'')
                    {                        
                        accum.Append(ch);
                        input.Read();
                        if (input.Peek() == -1)
                        {
                            throw new Exception("незавершенный строковый литерал");
                        }
                    }
                    // пропускаем закрывающую кавычку '
                    input.Read();
                }
                else if (char.IsDigit(ch))
                {
                    //числовая константа
                    var accum = new StringBuilder();
                    while (char.IsDigit(ch))
                    {
                        accum.Append(ch);
                        input.Read();

                        if (input.Peek() == -1)
                        {
                            break;
                        }
                        ch = (char) input.Peek();
                    }
                    // число с плавающей запятой
                    if (ch == '.')
                    {
                        // добавляем запятую
                        accum.Append(ch);
                        // пропускаем '.'
                        input.Read();
                        // получаем следующий символ
                        ch = (char) input.Peek();
                        if (char.IsDigit(ch) || ch == 'e' || ch == 'E')
                        {
                            // получаем дробную часть
                            ch = (char) input.Peek();
                            while (char.IsDigit(ch))
                            {
                                accum.Append(ch);
                                input.Read();
                                if (input.Peek() == -1)
                                {
                                    break;
                                }
                                ch = (char) input.Peek();
                            }                            
                        }
                    }
                }
                else
                {
                    // обработка символов операций
                    switch (ch)
                    {
                        case '{':
                            // обработка комментариев 
                            input.Read();
                            // пропускаем все символы
                            do
                            {
                                var end = input.Peek();
                                if (end == -1)
                                {
                                    throw new Exception("Незавершенный коментарий");
                                }
                                var ch1 = (char) input.Read();
                                if (ch1 == '}')
                                {
                                    // комментарий закрыт, продолжаем обработку
                                    break;
                                }
                            } while (true);
                            input.Read();                           
                            break;
                        case '-':
                            input.Read();
                            ch = (char)input.Peek();
                            // если следующий символ то это унарный минус
                            if (char.IsDigit(ch))
                            {
                                var accum = new StringBuilder("-");
                                while (char.IsDigit(ch))
                                {
                                    accum.Append(ch);
                                    input.Read();

                                    if (input.Peek() == -1)
                                    {
                                        break;
                                    }
                                    ch = (char)input.Peek();
                                }
                                // число с плавающей запятой
                                if (ch == '.')
                                {
                                    accum.Append(ch);
                                    input.Read();
                                    ch = (char)input.Peek();
                                    if (char.IsDigit(ch) || ch == 'e' || ch == 'E')
                                    {
                                        ch = (char)input.Peek();
                                        while (char.IsDigit(ch))
                                        {
                                            accum.Append(ch);
                                            input.Read();
                                            if (input.Peek() == -1)
                                            {
                                                break;
                                            }
                                            ch = (char)input.Peek();
                                        }
                                    }
                                }
                            }                            
                            break;
                        default:
                            input.Read();
                            break;
                    }
                }
            }
        }

        private void AddLexem(string lexem)
        {
            if (_result.ContainsKey(lexem.ToLower()))
            {
                _result[lexem.ToLower()]++;
            }
            else
            {
                _result.Add(lexem.ToLower(), 1);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // получаем все идентификаторы
            var scanner = new Scanner(File.OpenText("prog.txt"));
            // вывод на экран
            Console.WriteLine("{0, -20}|{1, 10}", "Идентификтор", "Количество вхождений");
            foreach (var o in scanner.Tokens.OrderBy(o => o.Key))
            {
                Console.WriteLine("{0, -20}|{1, 10}", o.Key, o.Value);
            }           
            Console.ReadLine();
        }
    }
}
