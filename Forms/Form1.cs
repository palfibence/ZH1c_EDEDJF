using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
namespace Forms
{
    public partial class Form1 : Form
    {

        Models.ReceptContext context = new Models.ReceptContext();
        public Form1()
        {
            InitializeComponent();
            NyersanyagListazas();
            HozzavaloListazas();
        }

        Excel.Application xlApp; // A Microsoft Excel alkalmaz�s
        Excel.Workbook xlWB;     // A l�trehozott munkaf�zet
        Excel.Worksheet xlSheet; // Munkalap a munkaf�zeten bel�l

        void CreateExcel()
        {
            try
            {
                // Excel elind�t�sa �s az applik�ci� objektum bet�lt�se
                xlApp = new Excel.Application();

                // �j munkaf�zet
                xlWB = xlApp.Workbooks.Add(Missing.Value);

                // �j munkalap
                xlSheet = xlWB.ActiveSheet;

                // T�bla l�trehoz�sa
                CreateTable(); // Ennek meg�r�sa a k�vetkez� feladatr�szben k�vetkezik

                // Control �tad�sa a felhaszn�l�nak
                xlApp.Visible = true;
                xlApp.UserControl = true;
            }
            catch (Exception ex) // Hibakezel�s a be�p�tett hiba�zenettel
            {
                string errMsg = string.Format("Error: {0}\nLine: {1}", ex.Message, ex.Source);
                MessageBox.Show(errMsg, "Error");

                // Hiba eset�n az Excel applik�ci� bez�r�sa automatikusan
                xlWB.Close(false, Type.Missing, Type.Missing);
                xlApp.Quit();
                xlWB = null;
                xlApp = null;
            }

            void CreateTable()
            {
                string[] fejlecek = new string[]
                {
                    "K�rd�s",
                    "1. v�lasz",
                    "2. v�lasz",
                    "3. v�lasz",
                    "Helyes v�lasz",
                    "k�p"
                };

                for (int i = 0; i < fejlecek.Length; i++)
                {
                    xlSheet.Cells[1, i + 1] = fejlecek[i];
                }

                Models.HajosContext hajosContext = new Models.HajosContext();
                var mindenKerdes = hajosContext.Questions.ToList();

                object[,] adatTomb = new object[mindenKerdes.Count(), fejlecek.Count()];

                for (int i = 0; i < mindenKerdes.Count; i++)
                {
                    adatTomb[i, 0] = mindenKerdes[i].Question1;
                    adatTomb[i, 1] = mindenKerdes[i].Answer1;
                    adatTomb[i, 2] = mindenKerdes[i].Answer2;
                    adatTomb[i, 3] = mindenKerdes[i].Answer3;
                    adatTomb[i, 4] = mindenKerdes[i].CorrectAnswer;
                    adatTomb[i, 5] = mindenKerdes[i].Image;
                }

                int sorokSzama = adatTomb.GetLength(0);
                int oszlopokSzama = adatTomb.GetLength(1);

                Excel.Range adatRange = xlSheet.get_Range("A2", Type.Missing).get_Resize(sorokSzama, oszlopokSzama);
                adatRange.Value2 = adatTomb;
                adatRange.Columns.AutoFit();
            }
        }

        private void buttonExcel_Click(object sender, EventArgs e)
        {
            CreateExcel();
        }

        private void textBoxNyersanyag_TextChanged(object sender, EventArgs e)
        {
            NyersanyagListazas();
        }
        private void NyersanyagListazas()
        {
            var hv = from x in context.Nyersanyagok
                     where x.NyersanyagNev.Contains(textBoxNyersanyag.Text)
                     select x;

            listBoxNyersanyag.DataSource = hv.ToList();
            listBoxNyersanyag.DisplayMember = "NyersanyagNev";
        }

        private void textBoxFogasok_TextChanged(object sender, EventArgs e)
        {
            HozzavaloListazas();
        }

        private void HozzavaloListazas()
        {
            var hv = from i in context.Fogasok
                     where i.FogasNev.Contains(textBoxFogasok.Text)
                     select i;

            listBoxFogasok.DataSource = hv.ToList();
            listBoxFogasok.DisplayMember = "FogasNev";
        }

        private void listBoxFogasok_SelectedIndexChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            var id = ((Models.Fogasok)listBoxFogasok.SelectedItem).FogasId;

            var hozz�val�k = from x in context.Receptek
                             where x.FogasId == id
                             select new Hozz�val�
                             {
                                 ReceptID = x.ReceptId,
                                 FogasID = x.FogasId,
                                 NyersanyagNev = x.Nyersanyag.NyersanyagNev,
                                 Mennyiseg_4fo = x.Mennyiseg4fo,
                                 EgysegNev = x.Nyersanyag.MennyisegiEgyseg.EgysegNev,
                                 �r = x.Mennyiseg4fo * (double?)x.Nyersanyag.Egysegar
                             };
            hozz�val�BindingSource.DataSource = hozz�val�k.ToList();
        }

        private void listBoxNyersanyag_SelectedIndexChanged(object sender, EventArgs e)
        {
            var kiv�lasztottNyersanyag = (Models.Nyersanyagok)listBoxNyersanyag.SelectedItem;
            var megyseg = (from x in context.MennyisegiEgysegek
                           where x.MennyisegiEgysegId == kiv�lasztottNyersanyag.MennyisegiEgysegId
                           select x).FirstOrDefault();
            
        }

        private void buttonPlus_Click(object sender, EventArgs e)
        {
            Models.Receptek r = new Models.Receptek();
            r.NyersanyagId = ((Models.Nyersanyagok)listBoxNyersanyag.SelectedItem).NyersanyagId;
            r.FogasId = ((Models.Fogasok)listBoxFogasok.SelectedItem).FogasId;
            double m;
            if (!double.TryParse(textBoxMennyiseg.Text, out m)) return;
            r.Mennyiseg4fo = m;
            context.Receptek.Add(r);
            context.SaveChanges();
            Refresh();
        }

        private void buttonMinus_Click(object sender, EventArgs e)
        {
            var receptID = ((Hozz�val�)hozz�val�BindingSource.Current).ReceptID;

            var t�rlend� = (from i in context.Receptek
                            where i.ReceptId == receptID
                            select i).FirstOrDefault();

            context.Receptek.Remove(t�rlend�);
            context.SaveChanges();
            Refresh();
        }
    }
}