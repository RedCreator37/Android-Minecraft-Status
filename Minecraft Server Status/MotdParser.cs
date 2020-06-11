using System.Text;

namespace Minecraft_Server_Status {
    public static class MotdParser {

        /// <summary>
        /// Parse the Minecraft Message-of-the-day format
        /// </summary>
        /// <param name="motd">Full motd with formatting codes</param>
        /// <returns>Formatted html code of the motd</returns>
        public static string ParseMotd(string motd) {
            var nextCode = false;
            var arr = motd.ToCharArray();
            var formatter = @"#FFFFFF";

            var formatted = new StringBuilder();
            formatted.Append("<html><body style=\"background-color:#2E2E2E;" +
                             "font-family:'monospace';font-size:14px;\">");
            for (var i = 0; i < motd.Length; i++)   // parse the '§' formatting codes
                if (arr[i] == '§') nextCode = true;
                else {
                    if (!nextCode) {    // if it's actual text, not the formatting code
                        if (arr[i] == '\n') formatted.Append("<br>");
                        else {
                            if (formatter.Contains('#')) {
                                formatted.Append("<font color = \"").Append(formatter)
                                    .Append("\">").Append(arr[i]).Append("</font>");
                            } else {    // format the entire section, including line breaks
                                var b = new StringBuilder();
                                var j = i;
                                while (arr[j] != '§') {
                                    b.Append(arr[j] == '\n' ? "<br>" : arr[j].ToString());
                                    j++;
                                }

                                i = j;
                                formatted.Append(formatter.Replace("{text}",
                                    b.ToString()));
                            }
                        }
                    } else formatter = GetFormatter(arr[i]);
                    nextCode = false;
                }
            formatted.Append("</body></html>");
            return formatted.ToString();
        }

        /// <summary>
        /// Get the matching formatter from a Minecraft hex
        /// formatting code
        /// </summary>
        /// <param name="code">A char with the code</param>
        /// <returns>The matching color code (starting with an #) or
        /// a formatted HTML string (replace {text} placeholder with
        /// actual text)</returns>
        private static string GetFormatter(char code) {
            return char.ToLower(code) switch
            {
                '0' => @"#000000",
                '1' => @"#0000AA",
                '2' => @"#00AA00",
                '3' => @"#00AAAA",
                '4' => @"#AA0000",
                '5' => @"#AA00AA",
                '6' => @"#FFAA00",
                '7' => @"#AAAAAA",
                '8' => @"#555555",
                '9' => @"#5555FF",
                'a' => @"#55FF55",
                'b' => @"#55FFFF",
                'c' => @"#FF5555",
                'd' => @"#FF55FF",
                'e' => @"#FFFF55",
                // formatting codes
                'k' => @"<mark>{text}</mark>",
                'l' => @"<strong>{text}</strong>",
                'm' => @"<del>{text}</del>",
                'n' => @"<ins>{text}</ins>",
                'o' => @"<i>{text}</i>",
                // to avoid re-parsing
                _ => @"#FFFFFF",
            };
        }

    }
}