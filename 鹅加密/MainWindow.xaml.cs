using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 鹅加密 {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        private String baseChar="鹅";
        private String[] deco = { "。", "…", "！", "？" };

        private String[] code4;
        private String[] code8;
        private String codeTerminal;
        private Dictionary<String, int> reverse = new Dictionary<string, int>();

        public MainWindow() {
            InitializeComponent();
            Init();
        }

        private void Init() {
            code4=new String[16];
            code8= new String[256];
            codeTerminal=baseChar+deco[0];
            code4[0]=baseChar+baseChar+baseChar+baseChar+baseChar;
            for (int i = 1; i < 16; ++i) {
                int l=i/4;
                code4[i]=baseChar;
                while(l-->0)  code4[i]+=baseChar;
                code4[i] += deco[i % 4];
            }
            for (int i = 0; i < 16; ++i) {
                for (int j = 0; j < 16; ++j) {
                    code8[(i << 4) + j] = code4[i] + code4[j];
                }
            }
            for (int i = 0; i < 256; ++i) {
                reverse[code8[i]] = i;
            }
            reverse[codeTerminal] = -1;
        }

        public String Encode(String input) {
            var bs = Encoding.UTF8.GetBytes(input);
            StringBuilder sb=new StringBuilder();
            int x = 0;
            foreach (var b in bs){
                x ^= b;
                sb.Append(code8[b]);
            }
            sb.Append(codeTerminal);
            sb.Append(code8[x]);
            return sb.ToString();
        }

        public String Decode(String input) {
            Regex r = new Regex("((?:"+baseChar+deco[0]+")|(?:"+baseChar + "{1,4}[" + String.Join("", deco) + baseChar + "]){2})*");
            using (MemoryStream s = new MemoryStream()) {
                var m = r.Match(input);
                StringBuilder sb = new StringBuilder();
                int x = 0;
                if (m.Success) {
                    bool terminal = false;
                    foreach (Capture e in m.Groups[1].Captures) {
                        int i;
                        if (!reverse.TryGetValue(e.Value, out i)) {
                            throw new ApplicationException("无效的输入");
                        }
                        if (terminal) {
                            if (x != i) throw new ApplicationException("无效的输入");
                        } else if (i == -1) {
                            terminal = true;
                        } else {
                            x ^= i;
                            s.WriteByte((byte)i);
                        }
                    }
                    if (!terminal) throw new ApplicationException("无效的输入");
                } else {
                    throw new ApplicationException("无效的输入");
                }
                s.Flush();
                return Encoding.UTF8.GetString(s.ToArray());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            output.Text = Encode(input.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs ev) {
            try {
                output.Text = Decode(input.Text);
            } catch(Exception e) {
                output.Text = e.Message;
            }
        }
    }
}
