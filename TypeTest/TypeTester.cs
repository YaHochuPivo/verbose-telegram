using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeTest
{
    internal class TypeTester
    {
        public TypeTester(string recordsPath, string testText)
        {
            _recPath = recordsPath;
            _testText = testText;
        }

        public void Run()
        {
            _resetTitle();
            Records.Load(_recPath);
            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => Records.Dump(_recPath);

            while (true)
            {
                if (!_isLogined())
                    _login();

                switch (_menuPage())
                {
                    case 1:
                        if (_testBrifingPage())
                            _doTest();
                        break;
                    case 2:
                        _recordTablePage();
                        break;
                    case 3:
                        _logout();
                        break;
                }
            }
        }

        private void _resetTitle(string addon = "")
        {
            Console.Title = $"Type tester";
            if (!string.IsNullOrEmpty(addon))
                Console.Title += $": {addon}";
        }

        private void _login()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("\"Type tester\", я тестирую пользователя: ");
                string? logStr = Console.ReadLine();
                if (string.IsNullOrEmpty(logStr))
                    continue;

                if (!Records.GetUserData(logStr, out int sm, out int ss))
                    Records.AddUser(logStr);
                _curUser = logStr;
                break;
            }
        }

        private bool _isLogined() => !string.IsNullOrEmpty(_curUser);

        private void _logout()
        {
            _curUser = null;
            Records.Dump(_recPath);
        }

        private string _getUserCommand(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        private void _consolePause()
        {
            Console.Write("Для продолжения нажмите Enter");
            Console.ReadLine();
        }

        private void _printCurrentUser() => Console.WriteLine($"Текущий пользователь: {_curUser}");

        private int _menuPage()
        {
            Console.Clear();
            _printCurrentUser();
            Console.WriteLine("Меню программы \"Type tester\":");
            Console.WriteLine("1) Начать тестирование");
            Console.WriteLine("2) Вывести таблицу рекордов");
            Console.WriteLine("3) Логаут");
            while (true)
            {
                string cmd = _getUserCommand(">>> ");
                if (int.TryParse(cmd, out int code))
                {
                    if (code > 0 && code <= 3)
                        return code;
                }
            }
        }

        private bool _testBrifingPage()
        {
            Console.Clear();
            _printCurrentUser();
            Console.WriteLine("Тут мы тестируем вас на скоропечатанье. Вам предлагается ввести " +
                "нижеуказанный текст (как можно быстрее) с предельным лимитом времени в 1 минуту. " +
                "Тест заканчивается сразу после того как вы введете текст или по истечении 1 минуты. " +
                "Ваш результат заносится в таблицу рекордов.");
            Console.WriteLine($"Текст для ввода: { _testText }");
            string cmd = _getUserCommand("Вы готовы (д/н)? ");
            return cmd == "д";
        }

        private void _doTest()
        {
            int timeLimitSec = 60;
            int typedInd = 0;
            int lastTypedInd = 0;
            bool testOver = false;

            List<int> secCounts = new List<int>();
            Timer orbitor = new Timer((object obj) =>
            {
                _resetTitle($"Идет тест, оставшееся время: {timeLimitSec}");
                testOver = (--timeLimitSec) <= 0;
                secCounts.Add(typedInd - lastTypedInd);
                lastTypedInd = typedInd;

            }, null, 0, 1000);

            Action render = () =>
            {
                Console.Clear();
                string nonTyped = _testText.Substring(typedInd);
                string typed = _testText.Substring(0, typedInd);
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Write(typed);
                Console.ResetColor();
                Console.Write(nonTyped);
            };

            render();
            while (!testOver)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyinfo = Console.ReadKey();
                    if (keyinfo.KeyChar == _testText[typedInd])
                    {
                        typedInd++;
                        if (typedInd > _testText.Length - 1)
                            testOver = true;
                    }

                    render();
                }

            orbitor.Dispose();
            _resetTitle();
            Console.WriteLine();
            Console.WriteLine("Тест завершен.");

            int avgSec = (int)secCounts.Average();
            int avgMin = avgSec * 60;

            Console.WriteLine($"Среднее в секунду: {avgSec};");
            Console.WriteLine($"Среднее в минуту: {avgMin}");
            Records.SetRecord(_curUser, avgMin, avgSec);
            _consolePause();
        }

        private void _recordTablePage()
        {
            Console.Clear();
            _printCurrentUser();

           
            Console.WriteLine("Таблица рекордов в сортировке по убыванию:");
            Console.WriteLine(string.Format("|{0,-20}|{1,-27}|{2,-27}|", "Имя", "Кол-во символов в минуту", "Кол-во символов в секунду"));
            foreach (string user in Records.Users)
            {
                if (!Records.GetUserData(user, out int sm, out int ss))
                    continue;
                Console.WriteLine(string.Format("|{0,-20}|{1,-27}|{2,-27}|", user, sm, ss));
            }

            _consolePause();
        }

        private string _recPath;
        private string _testText;
        private string? _curUser;
    }
}
