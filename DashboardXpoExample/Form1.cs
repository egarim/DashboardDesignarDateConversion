using DevExpress.DashboardCommon;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DashboardXpoExample
{
    public partial class Form1 : XtraForm
    {
        public Form1()
        {
            InitializeComponent();
            dashboardDesigner1.CreateRibbon();
            InitializeDashboard();
            dashboardDesigner1.ReloadData();
        }

        public void InitializeDashboard()
        {
            MyCustomXpoDataStore.Register();
            Dashboard dashboard = new Dashboard();
            DashboardXpoDataSource xpoDataSource = CreateXpoDataSource();

            dashboard.DataSources.Add(xpoDataSource);

            //PivotDashboardItem pivot = new PivotDashboardItem();
            //pivot.DataSource = xpoDataSource;
            //pivot.Rows.Add(new Dimension("ContactTitle"));
            //pivot.Columns.Add(new Dimension("Country"));
            //pivot.Columns.Add(new Dimension("TestDate"));
            //pivot.Values.Add(new Measure("CustomerID", SummaryType.Count));

            //ChartDashboardItem chart = new ChartDashboardItem();
            //chart.DataSource = xpoDataSource;
            //chart.Arguments.Add(new Dimension("Country"));
            //chart.Panes.Add(new ChartPane());
            //SimpleSeries theSeries = new SimpleSeries(SimpleSeriesType.Bar);
            //theSeries.Value = new Measure("CustomerID", SummaryType.Count);
            //chart.Panes[0].Series.Add(theSeries);

            //dashboard.Items.AddRange(pivot, chart);
            dashboard.RebuildLayout();
            dashboard.LayoutRoot.Orientation = DashboardLayoutGroupOrientation.Vertical;
            dashboardDesigner1.Dashboard = dashboard;
        }

        //public static DashboardXpoDataSource CreateXpoDataSource()
        //{
        //    DashboardXpoDataSource dataSource = new DashboardXpoDataSource();
        //   dataSource.ConnectionString = MyCustomXpoDataStore.GetConnectionString("XpoProvider=SQLite;data source=nwind.sqlite");
        //    dataSource.SetEntityType(typeof(nwind.Customers));
        //    return dataSource;
        //}
        public static MyCustomXpoDataSource CreateXpoDataSource()
        {
            MyCustomXpoDataSource dataSource = new MyCustomXpoDataSource();
            dataSource.ConnectionString = MyCustomXpoDataStore.GetConnectionString("Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Dd");
            dataSource.SetEntityType(typeof(nwind.Customers));
            return dataSource;
        }

    }
}
