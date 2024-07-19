using DevExpress.DashboardCommon;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.Xpo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class MyCustomXpoDataSource: DashboardXpoDataSource
{
    
    public MyCustomXpoDataSource()
    {
       this.AfterFill += MyCustomXpoDataSource_AfterFill;   
    }

    private void MyCustomXpoDataSource_AfterFill(object sender, EventArgs e)
    {
       
        
    }
}
