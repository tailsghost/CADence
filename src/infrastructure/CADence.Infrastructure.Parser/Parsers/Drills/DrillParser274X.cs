using System.Text;
using System.Xml;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Parsers.Drills
{
    /// <summary>
    /// ������ ��� NC drill ������ ������� 274X.
    /// </summary>
    public class DrillParser274X : DrillParserBase
    {
        /// <summary>
        /// �������� ������� ��� ���������.
        /// </summary>
        private List<string> DRILLS = new();

        /// <summary>
        /// ������, ����������� �� �����.
        /// </summary>
        private readonly List<string> _lines;

        /// <summary>
        /// ������� ����������, ������������ ��� ��������.
        /// </summary>
        private string _currentTool;

        /// <summary>
        /// ��������� ��� ��������.
        /// </summary>
        private DrillParser274xSettings _settings;

        private Drill274xFabric _fabric = new();

        /// <summary>
        /// �������������� ����� ��������� ������� Drill 274X.
        /// </summary>
        /// <param name="lines">������ ����� ������ �� drill �����.</param>
        public DrillParser274X(List<string> lines)
        {
            _lines = lines;
            _settings = new DrillParser274xSettings();
            Execute();
        }

        /// <summary>
        /// ��������� ������� ������ �� drill �����.
        /// </summary>
        public override void Execute()
        {
            using var stream = new StringReader(string.Join(Environment.NewLine, _lines));
            bool terminated = false;
            bool isAttrib = false;
            var commandBuilder = new StringBuilder();

            while (stream.Peek() != -1)
            {
                var c = (char)stream.Read();

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                else if (c == '%')
                {
                    if (commandBuilder.Length > 0)
                        throw new InvalidOperationException("Attribute mid-command.");
                    isAttrib = !isAttrib;
                }
                else if (c == '*')
                {
                    if (commandBuilder.Length == 0)
                        throw new InvalidOperationException("Empty command.");

                    string cmd = commandBuilder.ToString();
                    _settings.cmd = cmd;

                    _settings = _fabric.ExecuteCommand(_settings);

                    if (!_settings.IsDone)
                    {
                        terminated = true;
                        break;
                    }

                    commandBuilder.Clear();
                }
                else if (isAttrib)
                {
                    if (commandBuilder.Length == 0)
                        throw new InvalidOperationException("Empty command.");

                    string cmd = commandBuilder.ToString();

                    ProcessCommand(cmd);

                    if (!_settings.IsDone)
                    {
                        terminated = true;
                        break;
                    }

                    commandBuilder.Clear();
                }
                else
                {
                    commandBuilder.Append(c);
                }
            }

            if (isAttrib)
                throw new InvalidOperationException("Unterminated attribute.");

            if (!terminated)
                throw new InvalidOperationException("Unterminated drill file.");

            if (_settings.ApertureStack.Count != 1)
                throw new InvalidOperationException("Unterminated block aperture.");
        }



        /// <summary>
        /// ������������ �������, �������� ����������� ���������.
        /// </summary>
        /// <param name="command">������� ��� ���������.</param>
        private void ProcessCommand(string command)
        {
            var parameters = ParseRegularCommand(command);
            // ��������� ���������� �������
            if (parameters.TryGetValue('T', out var tool))
            {
                _settings.cmd = "T";
                _currentTool = tool;
            }

            // ������ ��������� ������ ������ (��������, ����������)
            if (parameters.TryGetValue('X', out var x))
            {
                _settings.cmd = "X";
                _settings = _fabric.ExecuteCommand(_settings, x);
                // ��������� ���������� X
            }

            if (parameters.TryGetValue('Y', out var y))
            {
                _settings.cmd = "Y";
                _settings = _fabric.ExecuteCommand(_settings, y);
                // ��������� ���������� Y
            }
        }

        /// <summary>
        /// ��������� �������, �������� � ���������.
        /// </summary>
        /// <param name="command">������� ��� ��������.</param>
        /// <returns>������� ���������� �������.</returns>
        private Dictionary<char, string> ParseRegularCommand(string command)
        {
            var parameters = new Dictionary<char, string>();
            char code = '\0';
            int start = 0;
            for (int i = 0; i <= command.Length; i++)
            {
                char c = i < command.Length ? command[i] : '\0';

                if (i == command.Length || char.IsLetter(c))
                {
                    if (i > start)
                    {
                        parameters[code] = command.Substring(start, i - start);
                    }
                    code = c;
                    start = i + 1;
                }
            }
            return parameters;
        }
    }
}
