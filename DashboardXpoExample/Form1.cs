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
                // Get all assemblies loaded in the current application domain
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Excluded assemblies that shouldn't be patched (add more as needed)
                var excludedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mscorlib",
            "System",
            "System.Core",
            "WindowsBase",
            "PresentationCore",
            "PresentationFramework",
            "Microsoft.CSharp",
            "netstandard",
            "System.Drawing",
            "System.Xml"
        };

                // Determine if an assembly should be patched
                bool ShouldPatchAssembly(Assembly assembly)
                {
                    string assemblyName = assembly.GetName().Name;

                    // Skip excluded system assemblies
                    if (excludedAssemblies.Contains(assemblyName))
                        return false;

                    // Optional: Skip dynamic assemblies to avoid potential issues
                    if (assembly.IsDynamic)
                        return false;

                    return true;
                }

                // Process each assembly
                foreach (var assembly in loadedAssemblies.Where(ShouldPatchAssembly))
                {
                    Debug.WriteLine($"Patching assembly: {assembly.GetName().Name}");

                    Type[] types;
                    try
                    {
                        // GetTypes can throw if assembly has unresolved references
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Continue with types that could be loaded
                        types = ex.Types.Where(t => t != null).ToArray();
                        Debug.WriteLine($"Some types couldn't be loaded in {assembly.GetName().Name}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error loading types from {assembly.GetName().Name}: {ex.Message}");
                        continue;
                    }

                    foreach (var type in types)
                    {
                        // Skip delegates, interfaces, abstract classes, etc.
                        if (type.IsInterface || type.IsAbstract || type.IsEnum ||
                            type.IsValueType || type.IsGenericTypeDefinition)
                            continue;

                        // Skip special types that shouldn't be patched
                        if (type.FullName?.StartsWith("System.") == true  ||
                           
                            type.FullName?.StartsWith("Microsoft.") == true ||
                            type.FullName?.StartsWith("<>") == true ||  // Skip compiler generated types
                            type.FullName?.StartsWith("Castle.") == true ||  // Skip DynamicProxy types
                            type == typeof(MethodTimeTracker))  // Skip our tracker to avoid infinite recursion
                            continue;

                        if (!type.FullName?.StartsWith("DevExpress.Dashboard.") == true
                             || !type.FullName?.StartsWith("DevExpress.Data.") == true
                              || !type.FullName?.StartsWith("DevExpress.Xpo.") == true
                              || !type.FullName?.StartsWith("DevExpress.Utils.") == true
                               || !type.FullName?.StartsWith("DevExpress.XtraGrid.") == true ||
                                type == typeof(MethodTimeTracker))  // Skip our tracker to avoid infinite recursion
                            continue;

                      

                        MethodInfo[] methods;
                        try
                        {
                            methods = type.GetMethods(BindingFlags.Public | 
                                                     BindingFlags.Instance | BindingFlags.Static |
                                                     BindingFlags.DeclaredOnly);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error getting methods from {type.FullName}: {ex.Message}");
                            continue;
                        }

                        foreach (var method in methods)
                        {
                            // Skip methods that shouldn't be patched
                            if (method.IsAbstract || method.IsGenericMethod ||
                                method.ContainsGenericParameters ||
                                method.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
                                method.Name.Contains("<") ||  // Skip compiler generated methods
                                method.DeclaringType != type) // Only patch methods directly declared in this type
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying Harmony patches: {ex.Message}",
                    "Patching Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private void ApplyMethodTracking()
        //{
        //    try
        //    {
        //        // Get all types from the current assembly
        //        var currentAssembly = Assembly.GetExecutingAssembly();
        //        var types = currentAssembly.GetTypes();

        //        // Also consider patching methods from other relevant assemblies
        //        // For DevExpress controls, you might want to be selective

        //        foreach (var type in types)
        //        {
        //            // Skip delegates, interfaces, abstract classes, etc.
        //            if (type.IsInterface || type.IsAbstract || type.IsEnum)
        //                continue;

        //            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
        //                                         BindingFlags.Instance | BindingFlags.Static |
        //                                         BindingFlags.DeclaredOnly);

        //            foreach (var method in methods)
        //            {
        //                // Skip certain methods that shouldn't be patched
        //                if (method.IsAbstract || method.IsGenericMethod ||
        //                    method.IsDefined(typeof(CompilerGeneratedAttribute), false))
        //                    continue;

        //                try
        //                {
        //                    // Create prefix and postfix methods
        //                    var prefix = new HarmonyMethod(typeof(MethodTimeTracker),
        //                                    nameof(MethodTimeTracker.Prefix));
        //                    var postfix = new HarmonyMethod(typeof(MethodTimeTracker),
        //                                    nameof(MethodTimeTracker.Postfix));

        //                    harmony.Patch(method, prefix, postfix);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.WriteLine($"Failed to patch method {method.Name} in {type.FullName}: {ex.Message}");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error applying Harmony patches: {ex.Message}",
        //            "Patching Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

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
