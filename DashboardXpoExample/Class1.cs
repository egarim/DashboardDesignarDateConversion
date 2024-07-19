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
public class MyCustomXpoDataStore : IDataStore, ICommandChannel
{

    IDataStore dataStore;
    public MyCustomXpoDataStore(IDataStore dataStore)
    {
        this.dataStore = dataStore;
    }
    public static IDataStore CreateProviderFromString(string connectionString, AutoCreateOption autoCreateOption, out IDisposable[] objectsToDisposeOnDisconnect)
    {
        ConnectionStringParser parser = new ConnectionStringParser(connectionString);
        parser.RemovePartByName("XpoProvider");
        connectionString = parser.GetPartByName("RealCnx");

        objectsToDisposeOnDisconnect = new IDisposable[] { null };
        return new MyCustomXpoDataStore(XpoDefault.GetConnectionProvider(connectionString, autoCreateOption));
    }


    public new static void Register()
    {
        //HACK review this link https://supportcenter.devexpress.com/ticket/details/BC4261/data-access-library-connection-provider-registration-has-been-changed

        DataStoreBase.RegisterDataStoreProvider(XpoProviderTypeString, new DataStoreCreationFromStringDelegate(CreateProviderFromString));

    }
    public static string GetConnectionString(string RealCnx)
    {
        return $"XpoProvider={XpoProviderTypeString};RealCnx='{RealCnx}'";
    }
    public new const string XpoProviderTypeString = nameof(MyCustomXpoDataStore);
    public AutoCreateOption AutoCreateOption => this.dataStore.AutoCreateOption;

    public object Do(string command, object args)
    {
        throw new System.NotImplementedException();
    }

    public ModificationResult ModifyData(params ModificationStatement[] dmlStatements)
    {
        return dataStore.ModifyData(dmlStatements);
    }

    public SelectedData SelectData(params SelectStatement[] selects)
    {
        Dictionary<int,CriteriaOperatorCollection> values = new Dictionary<int, CriteriaOperatorCollection>();
        for (int i = 0; i < selects.Length; i++)
        {
            SelectStatement selectStatement = selects[i];
            Debug.WriteLine(selectStatement.Table);
            Debug.WriteLine(selectStatement.ToString());
            values.Add(i, selectStatement.Operands);

        }
        
        SelectedData selectedData = dataStore.SelectData(selects);
        foreach (KeyValuePair<int, CriteriaOperatorCollection> value in values)
        {
            for (int i = 0; i < value.Value.Count; i++)
            {
                var Operand=value.Value[i] as QueryOperand;
                if(Operand!=null)
                {
                    if(Operand.ColumnType== DBColumnType.DateTime)
                    {
                        foreach (SelectStatementResultRow selectStatementResultRow in selectedData.ResultSet[value.Key].Rows)
                        {
                            selectStatementResultRow.Values[i] = new DateTime(2022, 1, 1);
                        }
                        //selectedData.ResultSet[value.Key].Rows[i]
                    }
                }
                //
            }

        }
        return selectedData;
    }


    public UpdateSchemaResult UpdateSchema(bool doNotCreateIfFirstTableNotExist, params DBTable[] tables)
    {
        return UpdateSchemaResult.SchemaExists;
    }
}