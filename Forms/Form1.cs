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

        Excel.Application xlApp; // A Microsoft Excel alkalmazás
        Excel.Workbook xlWB;     // A létrehozott munkafüzet
        Excel.Worksheet xlSheet; // Munkalap a munkafüzeten belül

        void CreateExcel()
        {
            try
            {
                // Excel elindítása és az applikáció objektum betöltése
                xlApp = new Excel.Application();

                // Új munkafüzet
                xlWB = xlApp.Workbooks.Add(Missing.Value);

                // Új munkalap
                xlSheet = xlWB.ActiveSheet;

                // Tábla létrehozása
                CreateTable(); // Ennek megírása a következõ feladatrészben következik

                // Control átadása a felhasználónak
                xlApp.Visible = true;
                xlApp.UserControl = true;
            }
            catch (Exception ex) // Hibakezelés a beépített hibaüzenettel
            {
                string errMsg = string.Format("Error: {0}\nLine: {1}", ex.Message, ex.Source);
                MessageBox.Show(errMsg, "Error");

                // Hiba esetén az Excel applikáció bezárása automatikusan
                xlWB.Close(false, Type.Missing, Type.Missing);
                xlApp.Quit();
                xlWB = null;
                xlApp = null;
            }

            void CreateTable()
            {
                string[] fejlecek = new string[]
                {
                    "Kérdés",
                    "1. válasz",
                    "2. válasz",
                    "3. válasz",
                    "Helyes válasz",
                    "kép"
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

            var hozzávalók = from x in context.Receptek
                             where x.FogasId == id
                             select new Hozzávaló
                             {
                                 ReceptID = x.ReceptId,
                                 FogasID = x.FogasId,
                                 NyersanyagNev = x.Nyersanyag.NyersanyagNev,
                                 Mennyiseg_4fo = x.Mennyiseg4fo,
                                 EgysegNev = x.Nyersanyag.MennyisegiEgyseg.EgysegNev,
                                 Ár = x.Mennyiseg4fo * (double?)x.Nyersanyag.Egysegar
                             };
            hozzávalóBindingSource.DataSource = hozzávalók.ToList();
        }

        private void listBoxNyersanyag_SelectedIndexChanged(object sender, EventArgs e)
        {
            var kiválasztottNyersanyag = (Models.Nyersanyagok)listBoxNyersanyag.SelectedItem;
            var megyseg = (from x in context.MennyisegiEgysegek
                           where x.MennyisegiEgysegId == kiválasztottNyersanyag.MennyisegiEgysegId
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
            var receptID = ((Hozzávaló)hozzávalóBindingSource.Current).ReceptID;

            var törlendõ = (from i in context.Receptek
                            where i.ReceptId == receptID
                            select i).FirstOrDefault();

            context.Receptek.Remove(törlendõ);
            context.SaveChanges();
            Refresh();
        }
    }
}