using DashboardXpoExample.nwind;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DashboardXpoExample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            var Provider= XpoDefault.GetConnectionProvider("Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Dd", DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            SimpleDataLayer dataLayer = new SimpleDataLayer(Provider);

            UnitOfWork uow = new UnitOfWork(dataLayer);

            uow.UpdateSchema(typeof(Customers));

            //var Customers = uow.Query<Customers>().ToList();
            int i = 0;
            for (int j = 0; j <10; j++)
            {
                var item =new Customers(uow); 
                item.ContactTitle = "Test " + i.ToString();
                item.TestDate = new DateTime(2021, 1, 1);
                item.CustomerID = "Customer " + i.ToString();
                item.Country = "Country " + i.ToString();
            }
            uow.CommitChanges();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
