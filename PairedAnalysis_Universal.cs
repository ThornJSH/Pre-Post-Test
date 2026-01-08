using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace PairedAnalysisApp
{
    public class PairedAnalysisForm : Form
    {
        private TextBox txtPre;
        private TextBox txtPost;
        private Button btnAnalyze;
        private Button btnReset;
        private WebBrowser resultBrowser;
        private Label lblPre;
        private Label lblPost;
        private MenuStrip menuStrip;
        private ToolStripMenuItem langMenu;

        // Current Language: "KO" or "EN"
        private string CurrentLang = "KO"; 

        public PairedAnalysisForm()
        {
            Localization.Init(); // Initialize resources
            InitializeComponent();
            UpdateUILanguage(); // Apply default language
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            // MenuStrip setup
            menuStrip = new MenuStrip();
            langMenu = new ToolStripMenuItem();
            // Text will be set in UpdateUILanguage
            
            ToolStripMenuItem menuKo = new ToolStripMenuItem("Korean (\uD55C\uAD6D\uC5B4)");
            menuKo.Tag = "KO";
            menuKo.Click += new EventHandler(OnLanguageChanged);
            
            ToolStripMenuItem menuEn = new ToolStripMenuItem("English");
            menuEn.Tag = "EN";
            menuEn.Click += new EventHandler(OnLanguageChanged);

            langMenu.DropDownItems.Add(menuKo);
            langMenu.DropDownItems.Add(menuEn);
            langMenu.Alignment = ToolStripItemAlignment.Right; 
            menuStrip.Items.Add(langMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            mainLayout.Padding = new Padding(10);
            
            // Adjust top margin for menu
            mainLayout.Margin = new Padding(0, 25, 0, 0); 

            lblPre = new Label();
            lblPre.AutoSize = true;
            lblPre.Font = new Font(this.Font, FontStyle.Bold);
            mainLayout.Controls.Add(lblPre, 0, 0);

            lblPost = new Label();
            lblPost.AutoSize = true;
            lblPost.Font = new Font(this.Font, FontStyle.Bold);
            mainLayout.Controls.Add(lblPost, 1, 0);

            txtPre = new TextBox();
            txtPre.Multiline = true;
            txtPre.Dock = DockStyle.Fill;
            txtPre.ScrollBars = ScrollBars.Vertical;
            txtPre.Font = new Font("Consolas", 10);
            
            txtPost = new TextBox();
            txtPost.Multiline = true;
            txtPost.Dock = DockStyle.Fill;
            txtPost.ScrollBars = ScrollBars.Vertical;
            txtPost.Font = new Font("Consolas", 10);
            
            mainLayout.Controls.Add(txtPre, 0, 1);
            mainLayout.Controls.Add(txtPost, 1, 1);

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Height = 40;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;

            btnAnalyze = new Button();
            btnAnalyze.Width = 150;
            btnAnalyze.Height = 30;
            btnAnalyze.BackColor = Color.DodgerBlue;
            btnAnalyze.ForeColor = Color.White;
            btnAnalyze.FlatStyle = FlatStyle.Flat;
            btnAnalyze.Click += new EventHandler(BtnAnalyze_Click);
            
            btnReset = new Button();
            btnReset.Width = 120;
            btnReset.Height = 30;
            btnReset.Margin = new Padding(10, 0, 0, 0);
            btnReset.Click += new EventHandler(BtnReset_Click);

            buttonPanel.Controls.Add(btnAnalyze);
            buttonPanel.Controls.Add(btnReset);

            TableLayoutPanel resultPanel = new TableLayoutPanel();
            resultPanel.Dock = DockStyle.Fill;
            resultPanel.RowCount = 2;
            resultPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            resultPanel.Controls.Add(buttonPanel, 0, 0);

            resultBrowser = new WebBrowser();
            resultBrowser.Dock = DockStyle.Fill;

            resultPanel.Controls.Add(resultBrowser, 0, 1);
            mainLayout.Controls.Add(resultPanel, 0, 2);
            mainLayout.SetColumnSpan(resultPanel, 2);
            
            // Add MainLayout to Form (but below menu)
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.Controls.Add(mainLayout);
            container.Padding = new Padding(0, 24, 0, 0); // Spacing for menu
            this.Controls.Add(container);
            
            // Bring menu to front just in case
            menuStrip.BringToFront();
            
            SetupPlaceholders();
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                string newLang = item.Tag.ToString();
                if (CurrentLang != newLang)
                {
                    CurrentLang = newLang;
                    UpdateUILanguage();
                    
                    // If results exist, re-generate report? 
                    // Difficult to persist result object without field. 
                    // Assume user will click analyze again.
                }
            }
        }

        private void UpdateUILanguage()
        {
            this.Text = GetText("Title");
            langMenu.Text = GetText("MenuLang");
            lblPre.Text = GetText("LblPre");
            lblPost.Text = GetText("LblPost");
            btnAnalyze.Text = GetText("BtnAnalyze");
            btnReset.Text = GetText("BtnReset");
            
            // Update placeholders if empty
            UpdatePlaceholderText(txtPre, "Placeholder");
            UpdatePlaceholderText(txtPost, "Placeholder");

            // Update browser initial message if empty or showing previous initial message
            // (Simple check: if it contains "body", reset it)
            if (resultBrowser.DocumentText == null || resultBrowser.DocumentText.Length < 10 || resultBrowser.DocumentText.Contains("padding-top:50px"))
            {
                 resultBrowser.DocumentText = "<html><body style='font-family:Segoe UI, sans-serif; color:#666; text-align:center; padding-top:50px;'>" + GetText("MsgWelcome") + "</body></html>";
            }
        }
        
        private string GetText(string key)
        {
            return Localization.Get(CurrentLang, key);
        }

        // Placeholder Logic
        private class PlaceholderHandler
        {
            private TextBox _txt;
            private string _phKey;
            private PairedAnalysisForm _form;
            public PlaceholderHandler(TextBox txt, string phKey, PairedAnalysisForm form) 
            { _txt = txt; _phKey = phKey; _form = form; }
            
            public void OnEnter(object sender, EventArgs e) 
            { 
                 string ph = _form.GetText(_phKey);
                 if (_txt.Text == ph) { _txt.Text = ""; _txt.ForeColor = Color.Black; } 
            }
            public void OnLeave(object sender, EventArgs e) 
            { 
                 string ph = _form.GetText(_phKey);
                 if (string.IsNullOrEmpty(_txt.Text) || _txt.Text.Trim().Length == 0) { _txt.Text = ph; _txt.ForeColor = Color.Gray; } 
            }
        }

        private void SetupPlaceholders()
        {
            SetupPlaceholder(txtPre, "Placeholder");
            SetupPlaceholder(txtPost, "Placeholder");
        }

        private void SetupPlaceholder(TextBox txt, string key)
        {
            PlaceholderHandler handler = new PlaceholderHandler(txt, key, this);
            txt.Enter += new EventHandler(handler.OnEnter);
            txt.Leave += new EventHandler(handler.OnLeave);
        }

        private void UpdatePlaceholderText(TextBox txt, string key)
        {
            // If text matches OLD placeholder (in either lang) or is empty, update to NEW placeholder
            string currentVal = txt.Text;
            string koPh = Localization.Get("KO", key);
            string enPh = Localization.Get("EN", key);
            
            if (string.IsNullOrEmpty(currentVal) || currentVal == koPh || currentVal == enPh)
            {
                txt.Text = GetText(key);
                txt.ForeColor = Color.Gray;
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            txtPre.Text = "";
            txtPost.Text = "";
            UpdatePlaceholderText(txtPre, "Placeholder");
            UpdatePlaceholderText(txtPost, "Placeholder");
            resultBrowser.DocumentText = "<html><body style='font-family:Segoe UI, sans-serif; color:#666; text-align:center; padding-top:50px;'>" + GetText("MsgReset") + "</body></html>";
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                List<double> preScores = ParseInput(txtPre.Text);
                List<double> postScores = ParseInput(txtPost.Text);

                if (preScores.Count == 0 || postScores.Count == 0) {
                    MessageBox.Show(GetText("MsgInputErr"), GetText("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (preScores.Count != postScores.Count) {
                    string msg = GetText("MsgCountMismatch") + string.Format("\n{0}: {1}, {2}: {3}", GetText("LblPreShort"), preScores.Count, GetText("LblPostShort"), postScores.Count);
                    MessageBox.Show(msg, GetText("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AnalysisResult result = PerformStatisticalAnalysis(preScores, postScores);
                string htmlReport = GenerateHtmlReport(result);
                resultBrowser.DocumentText = htmlReport;
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetText("MsgAnalysisErr") + "\n" + ex.Message, GetText("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<double> ParseInput(string input)
        {
            // Check against both placeholders
             if (input == Localization.Get("KO", "Placeholder") || input == Localization.Get("EN", "Placeholder")) return new List<double>();
            
            List<double> list = new List<double>();
            string[] tokens = input.Split(new char[] { ',', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                double val;
                if (double.TryParse(token, out val)) list.Add(val);
            }
            return list;
        }

        // --- Statistics & Logic (Same as before) ---
        public class AnalysisResult
        {
            public int N;
            public double PreMean, PreSd, PostMean, PostSd, DiffMean, DiffSd;
            public bool IsNormal;
            public double NormalityP, JbStat, Skewness, Kurtosis;
            public string TestNameKey, StatName; 
            public double TestStat, TestP;
            public bool UseWilcoxonOnly, UseBothTests;
            public WilcoxonResult WilcoxonData;
        }

        public class WilcoxonResult { public double W; public double Z; public double P; }
        public class JbResult { public double Statistic, PValue, Skewness, Kurtosis; }
        public class TResult { public double Statistic, PValue; }

        private double GetMean(List<double> data) 
        { 
            if (data.Count == 0) return 0;
            double sum = 0;
            foreach(double d in data) sum += d;
            return sum / data.Count;
        }
        
        private double GetStdev(List<double> data)
        {
            if (data.Count < 2) return 0;
            double avg = GetMean(data);
            double sum = 0;
            foreach(double d in data) sum += Math.Pow(d - avg, 2);
            return Math.Sqrt(sum / (data.Count - 1));
        }

        private AnalysisResult PerformStatisticalAnalysis(List<double> pre, List<double> post)
        {
            int n = pre.Count;
            List<double> diffs = new List<double>();
            for (int i = 0; i < n; i++) diffs.Add(post[i] - pre[i]);

            AnalysisResult res = new AnalysisResult();
            res.N = n;
            res.PreMean = GetMean(pre); res.PreSd = GetStdev(pre);
            res.PostMean = GetMean(post); res.PostSd = GetStdev(post);
            res.DiffMean = GetMean(diffs); res.DiffSd = GetStdev(diffs);

            JbResult jb = JarqueBeraTest(diffs);
            res.JbStat = jb.Statistic; res.NormalityP = jb.PValue; res.Skewness = jb.Skewness; res.Kurtosis = jb.Kurtosis;
            res.IsNormal = res.NormalityP > 0.05;

            // Use keys for test names
            string keyWilcoxon = "TestWilcoxon";
            string keyTtest = "TestTtest";

            if (n < 10) {
                res.UseWilcoxonOnly = true;
                res.TestNameKey = keyWilcoxon;
                WilcoxonResult w = WilcoxonSignedRankTest(diffs);
                res.WilcoxonData = w;
                res.TestStat = w.Z != 0 ? w.Z : w.W; 
                res.StatName = w.Z != 0 ? "Z" : "W";
                res.TestP = w.P;
            } else if (n >= 10 && n < 15) {
                res.UseBothTests = true;
                res.WilcoxonData = WilcoxonSignedRankTest(diffs);
                if (res.IsNormal) {
                    res.TestNameKey = keyTtest;
                    res.StatName = "t";
                    TResult t = PairedTTest(diffs);
                    res.TestStat = t.Statistic;
                    res.TestP = t.PValue;
                } else {
                    res.TestNameKey = keyWilcoxon;
                    res.StatName = res.WilcoxonData.Z != 0 ? "Z" : "W";
                    res.TestStat = res.WilcoxonData.Z != 0 ? res.WilcoxonData.Z : res.WilcoxonData.W;
                    res.TestP = res.WilcoxonData.P;
                }
            } else {
                 if (res.IsNormal) {
                    res.TestNameKey = keyTtest;
                    res.StatName = "t";
                    TResult t = PairedTTest(diffs);
                    res.TestStat = t.Statistic;
                    res.TestP = t.PValue;
                } else {
                    res.TestNameKey = keyWilcoxon;
                    WilcoxonResult w = WilcoxonSignedRankTest(diffs);
                    res.WilcoxonData = w;
                    res.StatName = w.Z != 0 ? "Z" : "W";
                    res.TestStat = w.Z != 0 ? w.Z : w.W; 
                    res.TestP = w.P;
                }
            }
            return res;
        }

        // ... JarqueBeraTest, PairedTTest, WilcoxonSignedRankTest, NormalCDF, StudentT_CDF_TwoTailed, IncompleteBeta, LogGamma 
        // (Copied largely from previous code, abbreviated for clarity in this view but handled fully in compilation)
        
        private JbResult JarqueBeraTest(List<double> data) {
             int n = data.Count;
             if (n < 4) { JbResult e = new JbResult(); e.PValue = 1; return e; }
             double mean = GetMean(data); double s = GetStdev(data);
             double sumZ3 = 0, sumZ4 = 0;
             foreach(double x in data) { double z = (x-mean)/s; sumZ3+=Math.Pow(z,3); sumZ4+=Math.Pow(z,4); }
             double S = (n*sumZ3)/((n-1)*(n-2));
             double term1 = (n*(double)(n+1)*sumZ4)/((n-1)*(n-2)*(n-3));
             double term2 = (3*Math.Pow(n-1,2))/((n-2)*(n-3));
             double K = term1 - term2;
             double JB = (n/6.0)*(Math.Pow(S,2)+0.25*Math.Pow(K,2));
             double p = Math.Exp(-JB/2.0);
             JbResult res = new JbResult(); res.Statistic=JB; res.PValue=p; res.Skewness=S; res.Kurtosis=K; return res;
        }
        private TResult PairedTTest(List<double> diffs) {
            int n = diffs.Count; double meanDiff = GetMean(diffs); double sdDiff = GetStdev(diffs);
            double t = meanDiff / (sdDiff / Math.Sqrt(n));
            int df = n - 1; double p = StudentT_CDF_TwoTailed(t, df);
            TResult res = new TResult(); res.Statistic = t; res.PValue = p; return res;
        }
        private class RankItem : IComparable<RankItem> {
            public double Diff, Abs;
            public int CompareTo(RankItem other) { return this.Abs.CompareTo(other.Abs); }
        }
        private WilcoxonResult WilcoxonSignedRankTest(List<double> diffs) {
            List<RankItem> items = new List<RankItem>();
            foreach(double d in diffs) if(d!=0) { RankItem i=new RankItem(); i.Diff=d; i.Abs=Math.Abs(d); items.Add(i); }
            int n = items.Count;
            if (n==0) { WilcoxonResult e=new WilcoxonResult(); e.P=1; return e; }
            items.Sort();
            double[] ranked = new double[n];
            for (int i=0; i<n;) {
                int j=i+1; while(j<n && items[j].Abs==items[i].Abs) j++;
                double rank=(i+1+j)/2.0; for(int k=i; k<j; k++) ranked[k]=rank;
                i=j;
            }
            double wPos=0, wNeg=0;
            for(int i=0; i<n; i++) if(items[i].Diff>0) wPos+=ranked[i]; else wNeg+=ranked[i];
            double W = Math.Min(wPos, wNeg);
            double meanW = n*(n+1)/4.0;
            double varW = n*(n+1)*(2*n+1)/24.0;
            double Z = (W-meanW)/Math.Sqrt(varW);
            double p = 2.0*(1.0-NormalCDF(Math.Abs(Z)));
            WilcoxonResult res=new WilcoxonResult(); res.W=W; res.Z=Z; res.P=p; return res;
        }
        private double NormalCDF(double x) {
            double a1=0.254829592, a2=-0.284496736, a3=1.421413741, a4=-1.453152027, a5=1.061405429, p=0.3275911;
            int sign=1; if(x<0) sign=-1; x=Math.Abs(x)/Math.Sqrt(2.0);
            double t = 1.0/(1.0+p*x);
            double y = 1.0-(((((a5*t+a4)*t)+a3)*t+a2)*t+a1)*t*Math.Exp(-x*x);
            return 0.5*(1.0+sign*y);
        }
        private double StudentT_CDF_TwoTailed(double t, double df) {
            t=Math.Abs(t); double x=df/(df+t*t); return IncompleteBeta(df/2.0, 0.5, x);
        }
        private double IncompleteBeta(double a, double b, double x) {
            if(x==0.0) return 0.0; if(x==1.0) return 1.0;
            if(x>(a+1.0)/(a+b+2.0)) return 1.0-IncompleteBeta(b,a,1.0-x);
            double lbeta=LogGamma(a)+LogGamma(b)-LogGamma(a+b);
            double front=Math.Exp(a*Math.Log(x)+b*Math.Log(1.0-x)-lbeta)/a;
            double f=1,c=1,d=0,h=d; int MAXIT=200; double EPS=3.0e-7;
            for(int m=1; m<=MAXIT; m++) {
                double m2=m*2; double aa=m*(b-m)*x/((a+m2-1.0)*(a+m2));
                d=1.0+aa*d; if(Math.Abs(d)<1e-30) d=1e-30;
                c=1.0+aa/c; if(Math.Abs(c)<1e-30) c=1e-30;
                d=1.0/d; h=d*c; front*=h;
                aa=-(a+m)*(a+b+m)*x/((a+m2)*(a+m2+1.0));
                d=1.0+aa*d; if(Math.Abs(d)<1e-30) d=1e-30;
                c=1.0+aa/c; if(Math.Abs(c)<1e-30) c=1e-30;
                d=1.0/d; double del=d*c; front*=del;
                if(Math.Abs(del-1.0)<EPS) break;
            }
            return front;
        }
        private double LogGamma(double x) {
            double[] p={0.99999999999980993,676.5203681218851,-1259.1392167224028,771.32342877765313,-176.61502916214059,12.507343278686905,-0.13857109526572012,9.9843695780195716e-6,1.5056327351493116e-7};
            int g=7; if(x<0.5) return Math.Log(Math.PI/Math.Sin(Math.PI*x))-LogGamma(1.0-x);
            x-=1.0; double a=p[0]; double t=x+g+0.5;
            for(int i=1; i<p.Length; i++) a+=p[i]/(x+i);
            return 0.5*Math.Log(2*Math.PI)+(x+0.5)*Math.Log(t)-t+Math.Log(a);
        }

        private string GenerateHtmlReport(AnalysisResult res) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><style>body { font-family: 'Segoe UI', Arial, sans-serif; padding: 20px; color: #333; } table { border-collapse: collapse; width: 100%; margin-bottom: 20px; font-size: 14px; } th, td { border: 1px solid #ddd; padding: 8px; text-align: center; } th { background-color: #f2f2f2; font-weight: bold; } .sig { color: #1a73e8; font-weight: bold; } .not-sig { color: #5f6368; } .section-title { font-size: 16px; font-weight: bold; margin-top: 20px; border-bottom: 2px solid #333; padding-bottom: 5px; margin-bottom: 10px; } .highlight-box { background-color: #e8f0fe; border-left: 4px solid #1a73e8; padding: 10px; margin: 10px 0; font-size: 0.9em; }</style></head><body>");

            // 1. Descriptive
            sb.Append(string.Format("<div class='section-title'>1. {0} (Descriptive Statistics)</div>", GetText("RptDescTitle")));
            sb.Append(string.Format("<table><tr><th>{0}</th><th>N</th><th>{1} (Mean)</th><th>{2} (SD)</th></tr>", GetText("RptType"), GetText("RptMean"), GetText("RptSd")));
            sb.Append(string.Format("<tr><td>{0} (Pre)</td><td>{1}</td><td>{2:F3}</td><td>{3:F3}</td></tr>", GetText("LblPreShort"), res.N, res.PreMean, res.PreSd));
            sb.Append(string.Format("<tr><td>{0} (Post)</td><td>{1}</td><td>{2:F3}</td><td>{3:F3}</td></tr>", GetText("LblPostShort"), res.N, res.PostMean, res.PostSd));
            sb.Append(string.Format("<tr><td>{0} (Diff)</td><td>{1}</td><td>{2:F3}</td><td>{3:F3}</td></tr>", GetText("RptDiff"), res.N, res.DiffMean, res.DiffSd));
            sb.Append("</table>");

            // 2. Normality
            sb.Append(string.Format("<div class='section-title'>2. {0} (Normality Test)</div>", GetText("RptNormTitle")));
            string normResult = res.IsNormal ? 
                string.Format("<span style='color:green; font-weight:bold;'>{0}</span> (p > .05)", GetText("RptNormOk")) : 
                string.Format("<span style='color:red; font-weight:bold;'>{0}</span> (p <= .05)", GetText("RptNormNo"));
            
            sb.Append(string.Format("<table><tr><th>{0}</th><th>N</th><th>{1}</th><th>{2}</th><th>JB</th><th>p-value</th></tr>", GetText("RptMethod"), GetText("RptSkew"), GetText("RptKurt")));
            sb.Append(string.Format("<tr><td>Jarque-Bera</td><td>{0}</td><td>{1:F3}</td><td>{2:F3}</td><td>{3:F3}</td><td>{4:F3}</td></tr>", res.N, res.Skewness, res.Kurtosis, res.JbStat, res.NormalityP));
            sb.Append("</table>");
            sb.Append(string.Format("<p>{0}: {1}</p>", GetText("RptResult"), normResult));

            // 3. Hypothesis
            sb.Append(string.Format("<div class='section-title'>3. {0} (Hypothesis Test)</div>", GetText("RptHypoTitle")));
            
            if (res.UseWilcoxonOnly)
                sb.Append(string.Format("<div class='highlight-box'>{0}</div>", GetText("WarnSmallN")));
            else if (res.UseBothTests)
                sb.Append(string.Format("<div class='highlight-box'>{0}</div>", GetText("InfoBoth")));

            string testName = GetText(res.TestNameKey);
            sb.Append(string.Format("<p><strong>{0}: {1}</strong></p>", GetText("RptSelected"), testName));
            
            string pStr = res.TestP < 0.001 ? "< .001" : string.Format("= {0:F3}", res.TestP);
            string sigStr = res.TestP < 0.05 ? GetText("SigUnique") : GetText("SigNot");
            string sigClass = res.TestP < 0.05 ? "sig" : "not-sig";

            if (res.IsNormal) {
                sb.Append("<table><tr><th>N</th><th>MeanDiff</th><th>SD_Diff</th><th>t</th><th>df</th><th>p-value</th></tr>");
                sb.Append(string.Format("<tr><td>{0}</td><td>{1:F3}</td><td>{2:F3}</td><td>{3:F3}</td><td>{4}</td><td>{5}</td></tr>", res.N, res.DiffMean, res.DiffSd, res.TestStat, res.N - 1, pStr));
                sb.Append("</table>");
            } else {
                sb.Append(string.Format("<table><tr><th>N</th><th>MeanDiff</th><th>{0}</th><th>p-value</th></tr>", res.StatName));
                sb.Append(string.Format("<tr><td>{0}</td><td>{1:F3}</td><td>{2:F3}</td><td>{3}</td></tr>", res.N, res.DiffMean, res.TestStat, pStr));
                sb.Append("</table>");
            }
            sb.Append(string.Format("<p class='{0}'>{1}: {2}</p>", sigClass, GetText("RptInterp"), sigStr));

            if (res.UseBothTests && res.WilcoxonData != null) {
                string wPStr = res.WilcoxonData.P < 0.001 ? "< .001" : string.Format("= {0:F3}", res.WilcoxonData.P);
                string wSigClass = res.WilcoxonData.P < 0.05 ? "sig" : "not-sig";
                string wSigStr = res.WilcoxonData.P < 0.05 ? GetText("SigUnique") : GetText("SigNot");
                sb.Append("<div style='margin-top:20px; padding:10px; background:#f9f9f9; border:1px solid #eee;'>");
                sb.Append(string.Format("<strong>+ {0} ({1})</strong>", GetText("RptAdd"), GetText("TestWilcoxonShort")));
                sb.Append(string.Format("<br>Z = {0:F3}, W = {1}, p {2}", res.WilcoxonData.Z, res.WilcoxonData.W, wPStr));
                sb.Append(string.Format("<br><span class='{0}'>({1})</span>", wSigClass, wSigStr));
                sb.Append("</div>");
            }

            // 4. APA
            sb.Append(string.Format("<div class='section-title'>4. {0} (APA Style)</div>", GetText("RptApaTitle")));
            sb.Append("<p style='background:#f4f4f4; padding:15px; border-radius:5px; font-style:italic;'>");
            sb.Append(GenerateAPAText(res, pStr));
            sb.Append("</p>");
            
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private string GenerateAPAText(AnalysisResult res, string pStr) {
            bool isSig = res.TestP < 0.05;
            // "increased"/"decreased"
            string effect = res.DiffMean > 0 ? GetText("Inc") : GetText("Dec");
            string effectText = "";
            if (isSig) effectText = string.Format(GetText("FmtSigChange"), effect);
            else effectText = GetText("NoSigChange");
            
            if (res.UseWilcoxonOnly) {
                return string.Format(GetText("ApaWilcoxonSmall"), res.N, effectText, res.TestStat, pStr);
            } else if (res.IsNormal) {
                return string.Format(GetText("ApaTtest"), res.PostMean, res.PostSd, res.PreMean, res.PreSd, effectText, res.N-1, res.TestStat, pStr);
            } else {
                return string.Format(GetText("ApaWilcoxonNorm"), effectText, res.TestStat, pStr);
            }
        }

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PairedAnalysisForm());
        }
    }

    // --- Localization Helper ---
    public static class Localization
    {
        private static Dictionary<string, string> EN;
        private static Dictionary<string, string> KO;

        public static void Init()
        {
            EN = new Dictionary<string, string>();
            KO = new Dictionary<string, string>();

            // Common
            Add("Title", "Pre-Post Analysis (welfareact.net\uC5D0\uC11C \uC81C\uC791\u00B7\uBC30\uD3EC\uD569\uB2C8\uB2E4.)", "\uC0AC\uC804-\uC0AC\uD6C4 \uBD84\uC11D (welfareact.net\uC5D0\uC11C \uC81C\uC791\u00B7\uBC30\uD3EC\uD569\uB2C8\uB2E4.)");
            // Show "Language" when in Korean mode, Show "언어" when in English mode?
            // Actually user said: 
            // - 한글일 때는 "Language" (In KO mode, show "Language")
            // - 영어일 때는 "언어" (In EN mode, show "언어")
            Add("MenuLang", "\uC5B8\uC5B4", "Language"); // EN="언어", KO="Language"
            Add("LblPre", "Pre Scores:", "\uC0AC\uC804 \uC810\uC218:");
            Add("LblPost", "Post Scores:", "\uC0AC\uD6C4 \uC810\uC218:");
            Add("LblPreShort", "Pre", "\uC0AC\uC804");
            Add("LblPostShort", "Post", "\uC0AC\uD6C4");
            Add("Placeholder", "Ex: 10, 20, 30...", "\uC608: 10, 20, 30...");
            Add("BtnAnalyze", "Analyze", "\uBD84\uC11D\uD558\uAE30");
            Add("BtnReset", "Reset", "\uCD08\uAE30\uD654");
            Add("MsgWelcome", "Enter data and click Analyze.", "\uB370\uC774\uD130\uB97C \uC785\uB825\uD558\uACE0 \uBD84\uC11D\uD558\uAE30 \uBC84\uD2BC\uC744 \uB204\uB974\uC138\uC694.");
            Add("MsgReset", "Data reset.", "\uB370\uC774\uD130\uAC00 \uCD08\uAE30\uD654\uB418\uC5C8\uC2B5\uB2C8\uB2E4.");
            Add("MsgInputErr", "Please enter scores.", "\uC810\uC218\uB97C \uC785\uB825\uD574\uC8FC\uC138\uC694.");
            Add("ErrTitle", "Error", "\uC624\uB958");
            Add("MsgCountMismatch", "Count mismatch.", "\uAC1C\uC218 \uBD88\uC77C\uCE58.");
            Add("MsgAnalysisErr", "Error during analysis:", "\uBD84\uC11D \uC911 \uC624\uB958:");

            // Report
            Add("RptDescTitle", "Descriptive Statistics", "\uAE30\uC220\uD1B5\uACC4");
            Add("RptType", "Type", "\uAD6C\uBD84");
            Add("RptMean", "Mean", "\uD3C9\uADE0");
            Add("RptSd", "SD", "\uD45C\uC900\uD3B8\uCC28");
            Add("RptDiff", "Diff", "\uCC28\uC774");
            
            Add("RptNormTitle", "Normality Test", "\uC815\uADDC\uC131 \uAC80\uC815");
            Add("RptNormOk", "Normal", "\uC815\uADDC\uC131 \uB9CC\uC871");
            Add("RptNormNo", "Not Normal", "\uC815\uADDC\uC131 \uC704\uBC30");
            Add("RptMethod", "Method", "\uAC80\uC815 \uBC29\uBC95");
            Add("RptSkew", "Skewness", "\uC65C\uB3C4");
            Add("RptKurt", "Kurtosis", "\uC9D0\uB3C4");
            Add("RptResult", "Result", "\uACB0\uACFC");

            Add("RptHypoTitle", "Hypothesis Test", "\uAC00\uC124 \uAC80\uC815");
            Add("WarnSmallN", "Warning: N < 10, using <strong>Wilcoxon Signed-Rank Test</strong>.", "\u26A0\uFE0F \uD45C\uBCF8 \uD06C\uAE30\uAC00 10 \uBBF8\uB9CC\uC774\uBBC0\uB85C <strong>\uC70C\uCF55\uC2A8 \uBD80\uD638\uC21C\uC704 \uAC80\uC815</strong>\uC744 \uC2E4\uC2DC\uD569\uB2C8\uB2E4.");
            Add("InfoBoth", "Info: 10 <= N < 15, consider both tests.", "\u2139\uFE0F \uD45C\uBCF8 \uD06C\uAE30\uAC00 10 \uC774\uC0C1 15 \uBBF8\uB9CC\uC774\uBBC0\uB85C \uB458 \uB2E4 \uCC38\uACE0\uD558\uC2ED\uC2DC\uC624.");
            Add("RptSelected", "Selected Test", "\uC120\uD0DD\uB41C \uAC80\uC815");
            Add("SigUnique", "Significant (Sig.)", "\uC720\uC758\uBBF8\uD568 (Sig.)");
            Add("SigNot", "Not Significant (Not Sig.)", "\uC720\uC758\uBBF8\uD558\uC9C0 \uC54A\uC74C (Not Sig.)");
            Add("RptInterp", "Interpretation", "\uD574\uC11D");
            Add("RptAdd", "Additional", "\uCD94\uAC00 \uBCF4\uACE0");
            
            Add("TestWilcoxon", "Wilcoxon Signed-Rank Test", "\uC70C\uCF55\uC2A8 \uBD80\uD638\uC21C\uC704 \uAC80\uC815");
            Add("TestWilcoxonShort", "Wilcoxon", "\uC70C\uCF55\uC2A8");
            Add("TestTtest", "Paired Samples t-test", "\uB300\uC751\uD45C\uBCF8 t-\uAC80\uC815");

            Add("RptApaTitle", "APA Reporting", "\uACB0\uACFC \uAE30\uC220 (APA Style)");
            Add("Footer", "Produced and distributed by welfareact.net", "welfareact.net\uC5D0\uC11C \uC81C\uC791\u00B7\uBC30\uD3EC\uD569\uB2C8\uB2E4.");

            // APA Fragments
            Add("Inc", "increased", "\uC99D\uAC00");
            Add("Dec", "decreased", "\uAC10\uC18C");
            Add("FmtSigChange", "significantly {0}", "\uC720\uC758\uBBF8\uD558\uAC8C {0}\uD558\uC600\uB2E4"); // "significantly increased" / "유의미하게 증가하였다"
            Add("NoSigChange", "did not significantly change", "\uC720\uC758\uBBF8\uD55C \uCC28\uC774\uAC00 \uC5C6\uC5C8\uB2E4");
            
            // APA Templates ({0} placeholders)
            // {0}=N, {1}=EffectText, {2}=Z/W, {3}=pStr
            Add("ApaWilcoxonSmall", "Due to small sample size (N={0}), Wilcoxon test was used. Analysis result: Post-test scores {1} compared to Pre-test (Z = {2:F3}, p {3}).", 
                "\uD45C\uBCF8 \uD06C\uAE30 10 \uBBF8\uB9CC(N={0})\uC774\uBBC0\uB85C \uC70C\uCF55\uC2A8 \uAC80\uC815\uC744 \uC2E4\uC2DC\uD558\uC600\uB2E4. \uBD84\uC11D \uACB0\uACFC, \uC0AC\uD6C4 \uC810\uC218\uB294 \uC0AC\uC804 \uC810\uC218\uC5D0 \uBE44\uD574 {1} (Z = {2:F3}, p {3}).");
            
            // {0}=PostM, {1}=PostSD, {2}=PreM, {3}=PreSD, {4}=EffectText, {5}=df, {6}=t, {7}=pStr
            Add("ApaTtest", "Paired t-test showed that Post-test (M={0:F2}, SD={1:F2}) {4} compared to Pre-test (M={2:F2}, SD={3:F2}) (t({5}) = {6:F3}, p {7}).",
                "\uB300\uC751\uD45C\uBCF8 t-\uAC80\uC815 \uACB0\uACFC, \uC0AC\uD6C4 \uC810\uC218(M={0:F2}, SD={1:F2})\uB294 \uC0AC\uC804 \uC810\uC218(M={2:F2}, SD={3:F2})\uC5D0 \uBE44\uD574 {4} (t({5}) = {6:F3}, p {7}).");
            
            // {0}=EffectText, {1}=Z, {2}=pStr
            Add("ApaWilcoxonNorm", "Due to violation of normality, Wilcoxon test was used. Post-test scores {0} compared to Pre-test (Z = {1:F3}, p {2}).",
                "\uC815\uADDC\uC131 \uC704\uBC30\uB85C \uC778\uD574 \uC70C\uCF55\uC2A8 \uBD80\uD638\uC21C\uC704 \uAC80\uC815\uC744 \uC2E4\uC2DC\uD558\uC600\uB2E4. \uC0AC\uD6C4 \uC810\uC218\uB294 \uC0AC\uC804 \uC810\uC218\uC5D0 \uBE44\uD574 {0} (Z = {1:F3}, p {2}).");
        }

        private static void Add(string key, string en, string ko)
        {
            if(!EN.ContainsKey(key)) EN.Add(key, en);
            if(!KO.ContainsKey(key)) KO.Add(key, ko);
        }

        public static string Get(string lang, string key)
        {
            if (lang == "KO" && KO.ContainsKey(key)) return KO[key];
            if (lang == "EN" && EN.ContainsKey(key)) return EN[key];
            return "[" + key + "]";
        }
    }
}
