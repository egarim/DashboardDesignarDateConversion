using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace DashboardXpoExample.nwind
{

    public partial class Customers
    {
        public Customers(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }


        DateTime testDate;

        public DateTime TestDate
        {
            get => testDate;
            set => SetPropertyValue(nameof(TestDate), ref testDate, value);
        }
    }

}
