using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CandidateSearchSystem.Contracts.Utils
{
    public static class InitialsSvgGenerator
    {
        /// <summary>
        /// Возвращает SVG как строку (UTF-8 текст) с инициалами, извлечёнными из fullName.
        /// </summary>
        /// <param name="fullName">ФИО или любая строка с именем</param>
        /// <param name="size">Размер в пикселях (width и height). По умолчанию 128.</param>
        /// <param name="shape">"circle" или "square"</param>
        public static string GenerateSvg(string fullName, int size = 128, string shape = "circle")
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "?";
            }

            // извлечь до 3 инициалов (первые буквы слов)
            var parts = fullName
                .Split(new[] { ' ', '\t', '\n', '\r', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Length > 0)
                .ToArray();

            // используем русскую культуру для ToUpperInvariant-аналога
            var culture = new CultureInfo("ru-RU");
            string initials;
            if (parts.Length == 0)
            {
                initials = "?";
            }
            else
            {
                initials = string.Concat(parts.Take(3).Select(p => p.Substring(0, 1).ToUpper(culture)));
            }

            // вычислить фон цвет на основе хэша fullName (детерминированно)
            var bg = ColorFromString(fullName);

            // выбрать цвет текста (black/white) для контраста
            var textColor = GetContrastColor(bg) ? "#000000" : "#FFFFFF";

            // настроить размер шрифта (примерно 50% высоты)
            int fontSize = (int)Math.Round(size * 0.5);

            // SVG с элементами: прямоугольник/круг и текст по центру
            var sb = new StringBuilder();
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\">");

            // background shape
            if (shape?.ToLowerInvariant() == "circle")
            {
                var cx = size / 2.0;
                var cy = size / 2.0;
                var r = size / 2.0;
                sb.AppendLine($"  <circle cx=\"{cx}\" cy=\"{cy}\" r=\"{r}\" fill=\"{bg}\" />");
            }
            else // square
            {
                sb.AppendLine($"  <rect x=\"0\" y=\"0\" width=\"{size}\" height=\"{size}\" rx=\"{Math.Round(size * 0.08)}\" ry=\"{Math.Round(size * 0.08)}\" fill=\"{bg}\" />");
            }

            // текст (центровка)
            // используем font-family: system-ui, sans-serif; (клиент может подставить)
            sb.AppendLine($"  <text x=\"50%\" y=\"50%\" text-anchor=\"middle\" dominant-baseline=\"central\" ");
            sb.AppendLine($"        font-family=\"system-ui, -apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif\" ");
            sb.AppendLine($"        font-size=\"{fontSize}px\" font-weight=\"600\" fill=\"{textColor}\">{EscapeXml(initials)}</text>");

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        /// <summary>
        /// Возвращает SVG в виде UTF-8 byte[] (удобно записать в файл или вернуть как image/svg+xml).
        /// </summary>
        public static byte[] GenerateSvgBytes(string fullName, int size = 128, string shape = "circle")
        {
            var svg = GenerateSvg(fullName, size, shape);
            return Encoding.UTF8.GetBytes(svg);
        }

        // --- вспомогательные методы ---

        // Детеминированное получение HEX цвета из строки
        private static string ColorFromString(string s)
        {
            // используем SHA1 и берём первые 3 байта -> RGB
            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(s ?? ""));
            // взять первые 3 байта
            int r = bytes[0];
            int g = bytes[1];
            int b = bytes[2];

            // можно слегка увеличить насыщенность/контраст, но базового хватит
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        // Возвращает true если лучше чёрный текст (темный фон -> white text => false)
        // здесь используем простую яркость: (R*299 + G*587 + B*114)/1000 - стандартная формула
        private static bool GetContrastColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#") || (hexColor.Length != 7 && hexColor.Length != 4))
                return true; // default black

            int r, g, b;
            if (hexColor.Length == 7)
            {
                r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
                g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
                b = Convert.ToInt32(hexColor.Substring(5, 2), 16);
            }
            else
            {
                // shorthand #RGB
                r = Convert.ToInt32(new string(hexColor[1], 2), 16);
                g = Convert.ToInt32(new string(hexColor[2], 2), 16);
                b = Convert.ToInt32(new string(hexColor[3], 2), 16);
            }

            double luminance = (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
            // если фон светлый -> ставим чёрный текст (true), иначе белый (false)
            return luminance > 0.6;
        }

        // простое экранирование XML-спецсимволов в тексте
        private static string EscapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }
    }
}
