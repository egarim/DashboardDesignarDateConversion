using DevExpress.DashboardCommon;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DashboardXpoExample
{
    public partial class Form1 : XtraForm
    {
        // Harmony instance for patching
        private static Harmony harmony;

        public Form1()
        {
            InitializeComponent();

            // Initialize Harmony and apply patches before other operations
            InitializeHarmony();

            dashboardDesigner1.CreateRibbon();
            InitializeDashboard();
            dashboardDesigner1.ReloadData();
        }

        private void InitializeHarmony()
        {
            // Create a unique ID for this Harmony instance
            harmony = new Harmony("com.yourcompany.methodtracker");

            // Apply patches to all methods in the application and loaded assemblies
            ApplyMethodTracking();
        }

        private void ApplyMethodTracking()
        {
            try
            {
                // Get all types from the current assembly
                var currentAssembly = Assembly.GetExecutingAssembly();
                var types = currentAssembly.GetTypes();

                // Also consider patching methods from other relevant assemblies
                // For DevExpress controls, you might want to be selective

                foreach (var type in types)
                {
                    // Skip delegates, interfaces, abstract classes, etc.
                    if (type.IsInterface || type.IsAbstract || type.IsEnum)
                        continue;

                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Instance | BindingFlags.Static |
                                                 BindingFlags.DeclaredOnly);

                    foreach (var method in methods)
                    {
                        // Skip certain methods that shouldn't be patched
                        if (method.IsAbstract || method.IsGenericMethod ||
                            method.IsDefined(typeof(CompilerGeneratedAttribute), false))
                            continue;

                        try
                        {
                            // Create prefix and postfix methods
                            var prefix = new HarmonyMethod(typeof(MethodTimeTracker),
                                            nameof(MethodTimeTracker.Prefix));
                            var postfix = new HarmonyMethod(typeof(MethodTimeTracker),
                                            nameof(MethodTimeTracker.Postfix));

                            harmony.Patch(method, prefix, postfix);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to patch method {method.Name} in {type.FullName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying Harmony patches: {ex.Message}",
                    "Patching Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void InitializeDashboard()
        {
            MyCustomXpoDataStore.Register();
            Dashboard dashboard = new Dashboard();
            DashboardXpoDataSource xpoDataSource = CreateXpoDataSource();

            dashboard.DataSources.Add(xpoDataSource);

            dashboard.RebuildLayout();
            dashboard.LayoutRoot.Orientation = DashboardLayoutGroupOrientation.Vertical;
            dashboardDesigner1.Dashboard = dashboard;
        }

        public static MyCustomXpoDataSource CreateXpoDataSource()
        {
            MyCustomXpoDataSource dataSource = new MyCustomXpoDataSource();
            dataSource.ConnectionString = MyCustomXpoDataStore.GetConnectionString("Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Dd");
            dataSource.SetEntityType(typeof(nwind.Customers));
            return dataSource;
        }
    }

    // Static class to handle method time tracking
    public static class MethodTimeTracker
    {
        // Dictionary to store stopwatches for each method call
        private static readonly Dictionary<int, Stopwatch> methodStopwatches =
                new Dictionary<int, Stopwatch>();

        // Prefix method - called before the original method
        public static void Prefix(MethodBase __originalMethod)
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            string methodName = $"{__originalMethod.DeclaringType.FullName}.{__originalMethod.Name}";

            // Create and start stopwatch
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Store in dictionary using thread ID to handle concurrent calls
            lock (methodStopwatches)
            {
                methodStopwatches[threadId] = stopwatch;
            }

            Debug.WriteLine($"[METHOD START] {methodName}");
        }

        // Postfix method - called after the original method
        public static void Postfix(MethodBase __originalMethod)
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            string methodName = $"{__originalMethod.DeclaringType.FullName}.{__originalMethod.Name}";

            Stopwatch stopwatch = null;

            // Get the stopwatch from dictionary
            lock (methodStopwatches)
            {
                if (methodStopwatches.TryGetValue(threadId, out stopwatch))
                {
                    methodStopwatches.Remove(threadId);
                }
            }

            if (stopwatch != null)
            {
                stopwatch.Stop();
                Debug.WriteLine($"[METHOD END] {methodName} - Execution time: {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }
}
